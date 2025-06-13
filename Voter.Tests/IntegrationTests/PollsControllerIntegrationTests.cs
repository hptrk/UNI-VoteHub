using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voter.DataAccess;
using Voter.DataAccess.Models;
using Voter.Shared.Models;

namespace Voter.Tests.IntegrationTests
{
    public class PollsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private string _authToken = string.Empty;

        private static readonly LoginRequestDTO AdminLogin = new()
        {
            Email = "admin@example.com",
            Password = "Admin@123"
        };

        private static readonly LoginRequestDTO UserLogin = new()
        {
            Email = "user@example.com",
            Password = "User@123"
        };
        public PollsControllerIntegrationTests()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                {
                    _ = builder.ConfigureServices(services =>
                    {
                        // in-memory database
                        ServiceDescriptor? descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<VoterDbContext>));
                        if (descriptor != null)
                        {
                            _ = services.Remove(descriptor);
                        }

                        // unique database name for each test run to avoid conflicts                    
                        string databaseName = $"TestPollsIntegrationDatabase_{Guid.NewGuid()}";
                        _ = services.AddDbContext<VoterDbContext>(options =>
                        {
                            _ = options.UseInMemoryDatabase(databaseName);
                        });

                        // eed the database with initial data
                        using IServiceScope scope = services.BuildServiceProvider().CreateScope();
                        IServiceProvider scopedServices = scope.ServiceProvider;
                        VoterDbContext db = scopedServices.GetRequiredService<VoterDbContext>();
                        _ = db.Database.EnsureCreated();

                        // users and roles first
                        RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                        SeedRoles(roleManager);

                        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                        SeedUsers(userManager);

                        // Then seed polls which depend on users
                        SeedPolls(db);
                    });
                });

            _client = _factory.CreateClient();

            // get  token
            AuthenticateAsync().Wait();
        }

        private async Task AuthenticateAsync()
        {
            // login and get the authentication token
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/login", UserLogin);
            using HttpResponseMessage _ = response.EnsureSuccessStatusCode();

            LoginResponseDTO? loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
            _authToken = loginResponse?.Token ?? string.Empty;

            // set the token for subsequent requests
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }

        private static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            // add roles
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                IdentityRole role = new("Admin");
                roleManager.CreateAsync(role).Wait();
            }

            if (!roleManager.RoleExistsAsync("User").Result)
            {
                IdentityRole role = new("User");
                roleManager.CreateAsync(role).Wait();
            }
        }

        private static void SeedUsers(UserManager<User> userManager)
        {
            // add admin user
            if (userManager.FindByEmailAsync(AdminLogin.Email).Result == null)
            {
                User user = new()
                {
                    UserName = AdminLogin.Email,
                    Email = AdminLogin.Email,
                    EmailConfirmed = true
                };

                userManager.CreateAsync(user, AdminLogin.Password).Wait();
                userManager.AddToRoleAsync(user, "Admin").Wait();
            }

            // add regular user
            if (userManager.FindByEmailAsync(UserLogin.Email).Result == null)
            {
                User user = new()
                {
                    UserName = UserLogin.Email,
                    Email = UserLogin.Email,
                    EmailConfirmed = true
                };

                userManager.CreateAsync(user, UserLogin.Password).Wait();
                userManager.AddToRoleAsync(user, "User").Wait();
            }
        }
        private static void SeedPolls(VoterDbContext db)
        {
            // get references to the users
            User? adminUser = db.Users.FirstOrDefault(u => u.Email == AdminLogin.Email);
            User? regularUser = db.Users.FirstOrDefault(u => u.Email == UserLogin.Email);

            // if the users don't exist in db yet skip seeding (shouldnt happen)
            if (adminUser == null || regularUser == null)
            {
                return;
            }

            // create active polls
            Poll activePoll1 = new()
            {
                Id = 1,
                Question = "Active Poll 1",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7),
                CreatorId = "admin-id",
                Creator = adminUser,
                Options =
                [
                    new PollOption { Id = 1, Text = "Option 1" },
                    new PollOption { Id = 2, Text = "Option 2" }
                ]
            };

            Poll activePoll2 = new()
            {
                Id = 2,
                Question = "Active Poll 2",
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(5),
                CreatorId = "user-id",
                Creator = regularUser,
                Options =
                [
                    new PollOption { Id = 3, Text = "Option A" },
                    new PollOption { Id = 4, Text = "Option B" }
                ]
            };

            // create closed poll
            Poll closedPoll = new()
            {
                Id = 3,
                Question = "Closed Poll",
                StartDate = DateTime.UtcNow.AddDays(-14),
                EndDate = DateTime.UtcNow.AddDays(-2),
                CreatorId = "admin-id",
                Creator = adminUser,
                Options =
                [
                    new PollOption { Id = 5, Text = "Option X" },
                    new PollOption { Id = 6, Text = "Option Y" }
                ]
            };

            db.Polls.AddRange(activePoll1, activePoll2, closedPoll);

            // add votes
            Vote vote1 = new()
            {
                UserId = "user-id",
                PollId = 1,
                PollOptionId = 1
            };

            Vote vote2 = new()
            {
                UserId = "admin-id",
                PollId = 3,
                PollOptionId = 5
            };

            db.Votes.AddRange(vote1, vote2);

            _ = db.SaveChanges();
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        #region GetActivePolls

        [Fact]
        public async Task GetActivePolls_ReturnsActivePolls()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/polls/active");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            List<PollDTO>? polls = await response.Content.ReadFromJsonAsync<List<PollDTO>>();
            Assert.NotNull(polls);
            Assert.Equal(2, polls.Count); // should have 2 active polls
        }

        [Fact]
        public async Task GetActivePolls_WithFilter_ReturnsFilteredPolls()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/polls/active?QuestionFilter=Poll 1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            List<PollDTO>? polls = await response.Content.ReadFromJsonAsync<List<PollDTO>>();
            Assert.NotNull(polls);
            _ = Assert.Single(polls); // should have 1 poll matching "Poll 1"
            Assert.Equal("Active Poll 1", polls[0].Question);
        }

        #endregion

        #region GetClosedPolls

        [Fact]
        public async Task GetClosedPolls_ReturnsClosedPolls()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/polls/closed");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            List<PollDTO>? polls = await response.Content.ReadFromJsonAsync<List<PollDTO>>();
            Assert.NotNull(polls);
            _ = Assert.Single(polls); // should have 1 closed poll
            Assert.Equal("Closed Poll", polls[0].Question);
        }

        [Fact]
        public async Task GetClosedPolls_WithDateFilter_ReturnsFilteredPolls()
        {
            // Arrange
            string startDate = DateTime.UtcNow.AddDays(-15).ToString("yyyy-MM-dd");
            string endDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            string url = $"/api/polls/closed?StartDate={startDate}&EndDate={endDate}";

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            List<PollDTO>? polls = await response.Content.ReadFromJsonAsync<List<PollDTO>>();
            Assert.NotNull(polls);
            _ = Assert.Single(polls); // should have 1 poll in the date range
        }

        #endregion

        #region GetPoll

        [Fact]
        public async Task GetPoll_ReturnsPoll_WhenPollExists()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/polls/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            PollDTO? poll = await response.Content.ReadFromJsonAsync<PollDTO>();
            Assert.NotNull(poll);
            Assert.Equal(1, poll.Id);
            Assert.Equal("Active Poll 1", poll.Question);
        }

        [Fact]
        public async Task GetPoll_ReturnsNotFound_WhenPollDoesNotExist()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/polls/999"); // random bad poll ID

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region CreatePoll

        [Fact]
        public async Task CreatePoll_ReturnsCreatedPoll_WhenValid()
        {
            // Arrange
            CreatePollRequestDTO createPollRequest = new()
            {
                Question = "New Integration Test Poll",
                StartDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                EndDate = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd"),
                Options = ["Option 1", "Option 2", "Option 3"]
            };

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/polls", createPollRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            PollDTO? poll = await response.Content.ReadFromJsonAsync<PollDTO>();

            Assert.NotNull(poll);
            Assert.Equal("New Integration Test Poll", poll.Question);
            Assert.Equal(3, poll.Options.Count());
        }

        [Fact]
        public async Task CreatePoll_ReturnsBadRequest_WhenInvalid()
        {
            // Arrange - missing required fields
            CreatePollRequestDTO createPollRequest = new()
            {
                Question = "", // empty question invlaid
                Options = ["Option 1"]
            };

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/polls", createPollRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Vote

        [Fact]
        public async Task Vote_ReturnsOk_WhenVoteIsValid()
        {
            // Arrange
            VoteRequestDTO voteRequest = new()
            {
                PollId = 2, // poll where user hasn't voted yet
                OptionId = 3
            };

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/polls/vote", voteRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Vote_ReturnsNotFound_WhenPollDoesNotExist()
        {
            // Arrange
            VoteRequestDTO voteRequest = new()
            {
                PollId = 999, // non-existent poll
                OptionId = 1
            };

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/polls/vote", voteRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region PollResults

        [Fact]
        public async Task GetPollResults_ReturnsPollResults_WhenPollExists()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/polls/1/results");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            PollResultDTO? results = await response.Content.ReadFromJsonAsync<PollResultDTO>();

            Assert.NotNull(results);
            Assert.Equal(1, results.PollId);
            Assert.Equal("Active Poll 1", results.Question);
            Assert.NotNull(results.Results);
            Assert.Equal(2, results.Results.Count());
        }

        #endregion

        #region Unauthenticated Access

        [Fact]
        public async Task Endpoints_RequireAuthentication()
        {
            // Arrange - create a new client without authentication
            using HttpClient unauthenticatedClient = _factory.CreateClient();

            // Act - try to access endpoints without authentication
            HttpResponseMessage activeResponse = await unauthenticatedClient.GetAsync("/api/polls/active");
            HttpResponseMessage closedResponse = await unauthenticatedClient.GetAsync("/api/polls/closed");
            HttpResponseMessage pollResponse = await unauthenticatedClient.GetAsync("/api/polls/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, activeResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, closedResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, pollResponse.StatusCode);
        }

        #endregion
    }
}
