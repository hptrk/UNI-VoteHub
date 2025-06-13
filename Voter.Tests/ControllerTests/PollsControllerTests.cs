using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Voter.DataAccess;
using Voter.DataAccess.Models;
using Voter.DataAccess.Services;
using Voter.Shared.Models;
using Voter.WebAPI.Controllers;
using Voter.WebAPI.Infrastructure;

namespace Voter.Tests.ControllerTests
{
    public class PollsControllerTests : IDisposable
    {
        private readonly VoterDbContext _context;
        private readonly PollsController _controller;
        private readonly IPollsService _pollsService;
        private readonly IMapper _mapper;
        private readonly string _testUserId = "test-user-id";

        public PollsControllerTests()
        {
            // Set up the in-memory database with a unique name
            DbContextOptions<VoterDbContext> options = new DbContextOptionsBuilder<VoterDbContext>()
                .UseInMemoryDatabase($"TestPollsDatabase_{Guid.NewGuid()}")
                .Options;
            _context = new VoterDbContext(options);

            // init dependencies
            MapperConfiguration mapperConfig = new(cfg => cfg.AddProfile(new MappingProfile()));
            _mapper = mapperConfig.CreateMapper();
            //standard PollsService
            _pollsService = new PollsService(_context);

            // controller setup with mock User
            _controller = new PollsController(_pollsService, _mapper);

            SetupTestUser(_controller, _testUserId);

            // init the database
            SeedDatabase();
        }

        private void SetupTestUser(PollsController controller, string userId)
        {
            // mock identity and principal
            System.Security.Claims.Claim[] claims =
            [
                new(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
                new(System.Security.Claims.ClaimTypes.Name, "testuser@example.com")
            ];

            System.Security.Claims.ClaimsIdentity identity = new(claims);
            System.Security.Claims.ClaimsPrincipal principal = new(identity);

            // set the User property on the controller
            ControllerContext controllerContext = new()
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
            };

            controller.ControllerContext = controllerContext;
        }

        private void SeedDatabase()
        {
            // clear database before seeding
            _ = _context.Database.EnsureDeleted();
            _ = _context.Database.EnsureCreated();

            // create test user
            User testUser = new()
            {
                Id = _testUserId,
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                EmailConfirmed = true
            };

            _ = _context.Users.Add(testUser);

            // create test polls
            Poll activePoll = new()
            {
                Id = 1,
                Question = "Active Poll Question",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                CreatorId = _testUserId,
                Creator = testUser,
                Options =
                [
                    new PollOption { Id = 1, Text = "Option 1" },
                    new PollOption { Id = 2, Text = "Option 2" }
                ]
            };

            Poll closedPoll = new()
            {
                Id = 2,
                Question = "Closed Poll Question",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(-1),
                CreatorId = _testUserId,
                Creator = testUser,
                Options =
                [
                    new PollOption { Id = 3, Text = "Option 1" },
                    new PollOption { Id = 4, Text = "Option 2" }
                ]
            };

            Vote vote = new()
            {
                UserId = _testUserId,
                PollId = 1,
                PollOptionId = 1
            };

            _context.Polls.AddRange(activePoll, closedPoll);
            _ = _context.Votes.Add(vote);

            _ = _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetActivePolls

        [Fact]
        public async Task GetActivePolls_ReturnsActivePolls_WithUserVoteInfo()
        {
            // Arrange
            GetActivePollsRequestDTO request = new() { QuestionFilter = string.Empty };

            // Act
            IActionResult result = await _controller.GetActivePolls(request);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<PollDTO> returnedPolls = Assert.IsAssignableFrom<IEnumerable<PollDTO>>(okResult.Value);

            List<PollDTO> pollsList = [.. returnedPolls];
            _ = Assert.Single(pollsList);

            PollDTO poll = pollsList.First();
            Assert.Equal(1, poll.Id);
            Assert.Equal("Active Poll Question", poll.Question);
            Assert.True(poll.UserHasVoted);

            // verify that the option the user voted for is marked
            PollOptionDTO? votedOption = poll.Options.FirstOrDefault(o => o.UserVoted);
            Assert.NotNull(votedOption);
            Assert.Equal(1, votedOption.Id);
        }

        [Fact]
        public async Task GetActivePolls_WithFilter_ReturnsFilteredPolls()
        {
            // Arrange
            GetActivePollsRequestDTO request = new() { QuestionFilter = "Active" };

            // Act
            IActionResult result = await _controller.GetActivePolls(request);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<PollDTO> returnedPolls = Assert.IsAssignableFrom<IEnumerable<PollDTO>>(okResult.Value);

            List<PollDTO> pollsList = [.. returnedPolls];
            _ = Assert.Single(pollsList);

            // verify no polls are returned with a non-matching filter
            request = new GetActivePollsRequestDTO { QuestionFilter = "NonExistentText" };
            result = await _controller.GetActivePolls(request);
            okResult = Assert.IsType<OkObjectResult>(result);
            returnedPolls = Assert.IsAssignableFrom<IEnumerable<PollDTO>>(okResult.Value);
            Assert.Empty(returnedPolls);
        }

        #endregion

        #region GetClosedPolls

        [Fact]
        public async Task GetClosedPolls_ReturnsClosedPolls()
        {
            // Arrange
            GetClosedPollsRequestDTO request = new() { QuestionFilter = string.Empty };

            // Act
            IActionResult result = await _controller.GetClosedPolls(request);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<PollDTO> returnedPolls = Assert.IsAssignableFrom<IEnumerable<PollDTO>>(okResult.Value);

            List<PollDTO> pollsList = [.. returnedPolls];
            _ = Assert.Single(pollsList);

            PollDTO poll = pollsList.First();
            Assert.Equal(2, poll.Id);
            Assert.Equal("Closed Poll Question", poll.Question);
            Assert.False(poll.UserHasVoted); // user has not voted on this closed poll
        }

        #endregion

        #region GetPoll

        [Fact]
        public async Task GetPoll_ReturnsCorrectPoll_WhenExists()
        {
            // Act
            IActionResult result = await _controller.GetPoll(1);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            PollDTO returnedPoll = Assert.IsType<PollDTO>(okResult.Value);

            Assert.Equal(1, returnedPoll.Id);
            Assert.Equal("Active Poll Question", returnedPoll.Question);
            Assert.True(returnedPoll.UserHasVoted);
        }

        [Fact]
        public async Task GetPoll_ReturnsNotFound_WhenPollDoesNotExist()
        {
            // Create a mock service to simulate entity not found
            Mock<IPollsService> mockService = new();
            _ = mockService.Setup(s => s.GetPollByIdAsync(999))
                .ThrowsAsync(new DataAccess.Exceptions.EntityNotFoundException(nameof(Poll), 999));

            // mock service for this test
            PollsController controller = new(mockService.Object, _mapper);
            SetupTestUser(controller, _testUserId);

            // Act
            IActionResult result = await controller.GetPoll(999); // random badd poll ID

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Vote

        [Fact]
        public async Task Vote_ReturnsOk_WhenVoteIsSuccessful()
        {
            // Arrange
            VoteRequestDTO voteRequest = new()
            {
                PollId = 1,
                OptionId = 2 // voting on a different option this time
            };

            // Act
            IActionResult result = await _controller.Vote(voteRequest);

            // Assert
            _ = Assert.IsType<OkResult>(result);

            // Verify the vote was updated in the database
            List<Vote> votes = await _context.Votes
                .Where(v => v.UserId == _testUserId && v.PollId == 1)
                .ToListAsync();

            _ = Assert.Single(votes);
            Assert.Equal(2, votes[0].PollOptionId); // should now be option 2
        }

        #endregion

        #region CreatePoll

        [Fact]
        public async Task CreatePoll_ReturnsCreatedPoll_WhenValid()
        {
            // Arrange
            CreatePollRequestDTO createPollRequest = new()
            {
                Question = "New Test Poll",
                StartDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                EndDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
                Options = ["Option A", "Option B", "Option C"]
            };

            // Act
            IActionResult result = await _controller.CreatePoll(createPollRequest);

            // Assert
            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result);
            PollDTO returnedPoll = Assert.IsType<PollDTO>(createdResult.Value);

            Assert.Equal("New Test Poll", returnedPoll.Question);
            Assert.Equal(3, returnedPoll.Options.Count());

            // verfiy the poll was added to the database
            Poll? dbPoll = await _context.Polls.FindAsync(returnedPoll.Id);
            Assert.NotNull(dbPoll);
            Assert.Equal("New Test Poll", dbPoll.Question);
            Assert.Equal(_testUserId, dbPoll.CreatorId);
        }

        [Fact]
        public async Task CreatePoll_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Question", "Question is required");
            CreatePollRequestDTO createPollRequest = new()
            {
                Question = "", // invalid - empty
                Options = ["Option A", "Option B"]
            };

            // Act
            IActionResult result = await _controller.CreatePoll(createPollRequest);

            // Assert
            _ = Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetPollResults

        [Fact]
        public async Task GetPollResults_ReturnsPollResults_WhenPollExists()
        {
            // Act
            IActionResult result = await _controller.GetPollResults(1);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            PollResultDTO pollResult = Assert.IsType<PollResultDTO>(okResult.Value);

            Assert.Equal(1, pollResult.PollId);
            Assert.Equal("Active Poll Question", pollResult.Question);
            Assert.NotNull(pollResult.Results);
            Assert.Equal(2, pollResult.Results.Count());
        }

        [Fact]
        public async Task GetPollResults_ReturnsNotFound_WhenPollDoesNotExist()
        {
            // cretae a mock service to simulate entity not found
            Mock<IPollsService> mockService = new();
            _ = mockService.Setup(s => s.GetPollByIdAsync(999)).ThrowsAsync(new DataAccess.Exceptions.EntityNotFoundException(nameof(Poll), 999));

            // use the mock service for this test
            PollsController controller = new(mockService.Object, _mapper);
            SetupTestUser(controller, _testUserId);

            // Act
            IActionResult result = await controller.GetPollResults(999);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }
}
