using Microsoft.EntityFrameworkCore;
using Voter.DataAccess;
using Voter.DataAccess.Exceptions;
using Voter.DataAccess.Models;
using Voter.DataAccess.Services;

namespace Voter.Tests.UnitTests
{
    public class PollsServiceTests : IDisposable
    {
        private readonly VoterDbContext _context;
        private readonly IPollsService _pollsService;

        public PollsServiceTests()
        {
            // in-memory database
            DbContextOptions<VoterDbContext> options = new DbContextOptionsBuilder<VoterDbContext>()
                .UseInMemoryDatabase($"TestPollServiceDatabase_{Guid.NewGuid()}") // Unique database for each test run
                .Options;

            _context = new VoterDbContext(options);

            // init the PollsService
            _pollsService = new PollsService(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // clear database before seeding
            _ = _context.Database.EnsureDeleted();
            _ = _context.Database.EnsureCreated();

            // create test users
            User testUser1 = new()
            {
                Id = "user-1",
                UserName = "user1@example.com",
                Email = "user1@example.com"
            };

            User testUser2 = new()
            {
                Id = "user-2",
                UserName = "user2@example.com",
                Email = "user2@example.com"
            };

            _context.Users.AddRange(testUser1, testUser2);

            // create active poll
            Poll activePoll = new()
            {
                Id = 1,
                Question = "Active Poll Question",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                CreatorId = "user-1",
                Creator = testUser1,
                Options =
                [
                    new() { Id = 1, Text = "Option 1" },
                    new() { Id = 2, Text = "Option 2" }
                ]
            };

            // create closed poll
            Poll closedPoll = new()
            {
                Id = 2,
                Question = "Closed Poll Question",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(-1),
                CreatorId = "user-2",
                Creator = testUser2,
                Options =
                [
                    new() { Id = 3, Text = "Option 1" },
                    new() { Id = 4, Text = "Option 2" }
                ]
            };

            _context.Polls.AddRange(activePoll, closedPoll);

            // create votes
            Vote vote1 = new()
            {
                UserId = "user-1",
                PollId = 1,
                PollOptionId = 1
            };

            Vote vote2 = new()
            {
                UserId = "user-2",
                PollId = 1,
                PollOptionId = 2
            };

            Vote vote3 = new()
            {
                UserId = "user-1",
                PollId = 2,
                PollOptionId = 3
            };

            _context.Votes.AddRange(vote1, vote2, vote3);

            _ = _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        #region GetActivePolls

        [Fact]
        public async Task GetActivePolls_ReturnsOnlyActivePolls()
        {
            // Act
            IEnumerable<Poll> activePolls = await _pollsService.GetActivePolls();

            // Assert
            Assert.NotNull(activePolls);
            _ = Assert.Single(activePolls);
            Assert.Equal("Active Poll Question", activePolls.First().Question);
        }

        [Fact]
        public async Task GetActivePolls_WithFilter_ReturnsFilteredPolls()
        {
            // Act
            IEnumerable<Poll> activePolls = await _pollsService.GetActivePolls("Active");

            // Assert
            Assert.NotNull(activePolls);
            _ = Assert.Single(activePolls);
            Assert.Equal("Active Poll Question", activePolls.First().Question);

            // Verify no polls are returned with a non-matching filter
            IEnumerable<Poll> emptyResults = await _pollsService.GetActivePolls("NonExistent");
            Assert.Empty(emptyResults);
        }

        [Fact]
        public async Task GetActivePolls_ReturnsEmpty_WhenNoActivePolls()
        {
            // Arrange - Close the only active poll by setting its end date to the past
            Poll? activePoll = await _context.Polls.FindAsync(1);
            if (activePoll != null)
            {
                activePoll.EndDate = DateTime.UtcNow.AddDays(-1);
                _ = await _context.SaveChangesAsync();
            }

            // Act
            IEnumerable<Poll> activePolls = await _pollsService.GetActivePolls();

            // Assert
            Assert.NotNull(activePolls);
            Assert.Empty(activePolls);
        }

        #endregion        
        #region GetClosedPolls

        [Fact]
        public async Task GetClosedPolls_ReturnsOnlyClosedPolls()
        {
            // Act
            IEnumerable<Poll> closedPolls = await _pollsService.GetClosedPolls(string.Empty);

            // Assert
            Assert.NotNull(closedPolls);
            _ = Assert.Single(closedPolls);
            Assert.Equal("Closed Poll Question", closedPolls.First().Question);
        }

        [Fact]
        public async Task GetClosedPolls_WithDateFilter_ReturnsFilteredPolls()
        {
            // Arrange - Add another closed poll with different dates
            Poll anotherClosedPoll = new()
            {
                Id = 8,
                Question = "Another Closed Poll",
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(-15),
                CreatorId = "user-1",
                Options =
                [
                    new PollOption { Id = 15, Text = "Option X" },
                    new PollOption { Id = 16, Text = "Option Y" }
                ]
            };

            _ = await _context.Polls.AddAsync(anotherClosedPoll);
            _ = await _context.SaveChangesAsync();

            // Act - Filter by date range
            DateTime startFilter = DateTime.UtcNow.AddDays(-12);
            DateTime endFilter = DateTime.UtcNow;
            IEnumerable<Poll> filteredPolls = await _pollsService.GetClosedPolls(null, startFilter, endFilter);

            // Assert - Should only return polls that ended within the specified range
            Assert.NotNull(filteredPolls);
            _ = Assert.Single(filteredPolls);
            Assert.Equal("Closed Poll Question", filteredPolls.First().Question);
        }

        [Fact]
        public async Task GetClosedPolls_WithQuestionFilter_ReturnsFilteredPolls()
        {
            // Act
            IEnumerable<Poll> filteredPolls = await _pollsService.GetClosedPolls("Closed");

            // Assert
            Assert.NotNull(filteredPolls);
            _ = Assert.Single(filteredPolls);
            Assert.Equal("Closed Poll Question", filteredPolls.First().Question);

            // Test with non-matching filter
            IEnumerable<Poll> emptyResults = await _pollsService.GetClosedPolls("ActiveFilter");
            Assert.Empty(emptyResults);
        }

        [Fact]
        public async Task GetClosedPolls_ReturnsEmpty_WhenNoClosedPolls()
        {
            // Arrange - Make the closed poll active again
            Poll? closedPoll = await _context.Polls.FindAsync(2);
            if (closedPoll != null)
            {
                closedPoll.EndDate = DateTime.UtcNow.AddDays(5);
                _ = await _context.SaveChangesAsync();
            }

            // Act
            IEnumerable<Poll> closedPolls = await _pollsService.GetClosedPolls();

            // Assert
            Assert.NotNull(closedPolls);
            Assert.Empty(closedPolls);
        }

        #endregion

        #region GetPollById

        [Fact]
        public async Task GetPollByIdAsync_ReturnsPoll_WhenExists()
        {
            // Act
            Poll poll = await _pollsService.GetPollByIdAsync(1);

            // Assert
            Assert.NotNull(poll);
            Assert.Equal(1, poll.Id);
            Assert.Equal("Active Poll Question", poll.Question);
        }

        [Fact]
        public async Task GetPollByIdAsync_ThrowsEntityNotFound_WhenPollDoesNotExist()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() => _pollsService.GetPollByIdAsync(999));
        }

        #endregion        
        #region HasUserVoted

        [Fact]
        public async Task HasUserVotedAsync_ReturnsTrue_WhenUserHasVoted()
        {
            // Act
            bool hasVoted = await _pollsService.HasUserVotedAsync(1, "user-1");

            // Assert
            Assert.True(hasVoted);
        }

        [Fact]
        public async Task HasUserVotedAsync_ReturnsFalse_WhenUserHasNotVoted()
        {
            // Arrange - Add a new poll where user hasn't voted
            Poll newPoll = new()
            {
                Id = 3,
                Question = "New Poll",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                CreatorId = "user-2",
                Options =
                [
                    new PollOption { Id = 5, Text = "Option X" },
                    new PollOption { Id = 6, Text = "Option Y" }
                ]
            };

            _ = await _context.Polls.AddAsync(newPoll);
            _ = await _context.SaveChangesAsync();

            // Act
            bool hasVoted = await _pollsService.HasUserVotedAsync(3, "user-1");

            // Assert
            Assert.False(hasVoted);
        }

        [Fact]
        public async Task HasUserVotedAsync_ReturnsFalse_WhenPollDoesNotExist()
        {
            // Act - Check for a non-existent poll
            bool hasVoted = await _pollsService.HasUserVotedAsync(999, "user-1");

            // Assert
            Assert.False(hasVoted);
        }

        [Fact]
        public async Task HasUserVotedAsync_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Act - Check with a non-existent user
            bool hasVoted = await _pollsService.HasUserVotedAsync(1, "non-existent-user");

            // Assert
            Assert.False(hasVoted);
        }

        [Fact]
        public async Task HasUserVotedAsync_ReturnsTrue_AfterUserVotesOnNewPoll()
        {
            // Arrange - Create new poll and have user vote on it
            Poll newPoll = new()
            {
                Id = 9,
                Question = "New Poll for Vote Test",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                CreatorId = "user-2",
                Options =
                [
                    new PollOption { Id = 17, Text = "Option 1" },
                    new PollOption { Id = 18, Text = "Option 2" }
                ]
            };

            _ = await _context.Polls.AddAsync(newPoll);
            _ = await _context.SaveChangesAsync();

            // Verify user hasn't voted initially
            bool hasVotedBefore = await _pollsService.HasUserVotedAsync(9, "user-1");
            Assert.False(hasVotedBefore);

            // Act - User votes
            _ = await _pollsService.VoteAsync(9, 17, "user-1");

            // Act - Check if user has voted
            bool hasVotedAfter = await _pollsService.HasUserVotedAsync(9, "user-1");

            // Assert
            Assert.True(hasVotedAfter);
        }

        #endregion

        #region VoteAsync
        [Fact]
        public async Task VoteAsync_CreatesVote_WhenUserHasNotVotedBefore()
        {
            // Arrange - create a new poll
            Poll newPoll = new()
            {
                Id = 4,
                Question = "Vote Test Poll",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10),
                CreatorId = "user-2",
                Options =
                [
                    new PollOption { Id = 7, Text = "Option 1" },
                    new PollOption { Id = 8, Text = "Option 2" }
                ]
            };

            _ = await _context.Polls.AddAsync(newPoll);
            _ = await _context.SaveChangesAsync();

            // Act
            Vote vote = await _pollsService.VoteAsync(4, 7, "user-1");

            // Assert
            Assert.NotNull(vote);
            Assert.Equal(7, vote.PollOptionId);

            Vote? dbVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == "user-1" && v.PollId == 4);

            Assert.NotNull(dbVote);
            Assert.Equal(7, dbVote.PollOptionId);
        }

        [Fact]
        public async Task VoteAsync_UpdatesVote_WhenUserHasVotedBefore()
        {
            // Act
            Vote updatedVote = await _pollsService.VoteAsync(1, 2, "user-1"); // User-1 already voted for option 1

            // Assert
            Assert.NotNull(updatedVote);
            Assert.Equal(2, updatedVote.PollOptionId);

            Vote? dbVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == "user-1" && v.PollId == 1);

            Assert.NotNull(dbVote);
            Assert.Equal(2, dbVote.PollOptionId); // Should now be option 2
        }

        [Fact]
        public async Task VoteAsync_ThrowsEntityNotFoundException_WhenPollDoesNotExist()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.VoteAsync(999, 1, "user-1"));
        }

        [Fact]
        public async Task VoteAsync_ThrowsPollNotActiveException_WhenPollHasNotStarted()
        {
            // Arrange - create a future poll
            Poll futurePoll = new()
            {
                Id = 5,
                Question = "Future Poll",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(10),
                CreatorId = "user-1",
                Options =
                [
                    new PollOption { Id = 9, Text = "Option 1" },
                    new PollOption { Id = 10, Text = "Option 2" }
                ]
            };

            _ = await _context.Polls.AddAsync(futurePoll);
            _ = await _context.SaveChangesAsync();

            // Act & Assert
            _ = await Assert.ThrowsAsync<PollNotActiveException>(() =>
                _pollsService.VoteAsync(5, 9, "user-1"));
        }

        [Fact]
        public async Task VoteAsync_ThrowsPollNotActiveException_WhenPollHasEnded()
        {
            // Act & Assert - trying to vote on the closed poll (ID: 2)
            _ = await Assert.ThrowsAsync<PollNotActiveException>(() =>
                _pollsService.VoteAsync(2, 3, "user-2"));
        }

        [Fact]
        public async Task VoteAsync_ThrowsEntityNotFoundException_WhenOptionDoesNotExist()
        {
            // Act & Assert - trying to vote with non-existent option ID
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.VoteAsync(1, 999, "user-1"));
        }

        [Fact]
        public async Task VoteAsync_ThrowsEntityNotFoundException_WhenOptionBelongsToDifferentPoll()
        {
            // Act & Assert - trying to vote on poll 1 with option from poll 2
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.VoteAsync(1, 3, "user-1")); // Option 3 belongs to poll 2
        }

        #endregion        
        #region GetPollResults

        [Fact]
        public async Task GetPollResultsAsync_ReturnsCorrectVoteCounts()
        {
            // Act
            Dictionary<PollOption, int> results = await _pollsService.GetPollResultsAsync(1);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);

            // Poll 1 has 1 vote for option 1 and 1 vote for option 2
            KeyValuePair<PollOption, int> option1 = results.FirstOrDefault(r => r.Key.Id == 1);
            KeyValuePair<PollOption, int> option2 = results.FirstOrDefault(r => r.Key.Id == 2);

            Assert.NotNull(option1.Key);
            Assert.Equal(1, option1.Value);

            Assert.NotNull(option2.Key);
            Assert.Equal(1, option2.Value);
        }

        [Fact]
        public async Task GetPollResultsAsync_ThrowsEntityNotFound_WhenPollDoesNotExist()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() => _pollsService.GetPollResultsAsync(999));
        }


        #endregion
        #region GetVotersAsync

        [Fact]
        public async Task GetVotersAsync_ReturnsCorrectVoters()
        {
            // Act
            IEnumerable<User> voters = await _pollsService.GetVotersAsync(1);

            // Assert
            Assert.NotNull(voters);
            Assert.Equal(2, voters.Count());

            // check that both users who voted are included
            Assert.Contains(voters, user => user.Id == "user-1");
            Assert.Contains(voters, user => user.Id == "user-2");
        }

        [Fact]
        public async Task GetVotersAsync_ThrowsEntityNotFoundException_WhenPollDoesNotExist()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() => _pollsService.GetVotersAsync(999));
        }

        [Fact]
        public async Task GetVotersAsync_ReturnsEmptyList_WhenNoVotesCast()
        {
            // Arrange - create a poll with no votes
            Poll pollWithNoVotes = new()
            {
                Id = 7,
                Question = "Poll with no votes for voters test",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                CreatorId = "user-1",
                Options =
                [
                    new PollOption { Id = 13, Text = "Option A" },
                    new PollOption { Id = 14, Text = "Option B" }
                ]
            };

            _ = await _context.Polls.AddAsync(pollWithNoVotes);
            _ = await _context.SaveChangesAsync();

            // Act
            IEnumerable<User> voters = await _pollsService.GetVotersAsync(7);

            // Assert
            Assert.NotNull(voters);
            Assert.Empty(voters);
        }

        #endregion        
        #region Edge Cases and Boundary Tests

        [Fact]
        public async Task GetActivePolls_WithNullFilter_ReturnsAllActivePolls()
        {
            // Act
            IEnumerable<Poll> activePolls = await _pollsService.GetActivePolls(null);

            // Assert
            Assert.NotNull(activePolls);
            _ = Assert.Single(activePolls);
            Assert.Equal("Active Poll Question", activePolls.First().Question);
        }

        [Fact]
        public async Task GetActivePolls_WithEmptyStringFilter_ReturnsAllActivePolls()
        {
            // Act
            IEnumerable<Poll> activePolls = await _pollsService.GetActivePolls("");

            // Assert
            Assert.NotNull(activePolls);
            _ = Assert.Single(activePolls);
            Assert.Equal("Active Poll Question", activePolls.First().Question);
        }

        [Fact]
        public async Task GetActivePolls_WithWhitespaceFilter_ReturnsAllActivePolls()
        {
            // Act
            IEnumerable<Poll> activePolls = await _pollsService.GetActivePolls("   ");

            // Assert
            Assert.NotNull(activePolls);
            _ = Assert.Single(activePolls);
            Assert.Equal("Active Poll Question", activePolls.First().Question);
        }

        [Fact]
        public async Task GetClosedPolls_WithNullParameters_ReturnsAllClosedPolls()
        {
            // Act
            IEnumerable<Poll> closedPolls = await _pollsService.GetClosedPolls(null, null, null);

            // Assert
            Assert.NotNull(closedPolls);
            _ = Assert.Single(closedPolls);
            Assert.Equal("Closed Poll Question", closedPolls.First().Question);
        }

        [Fact]
        public async Task GetClosedPolls_WithFutureDateRange_ReturnsEmpty()
        {
            // Arrange
            DateTime futureStart = DateTime.UtcNow.AddDays(1);
            DateTime futureEnd = DateTime.UtcNow.AddDays(5);

            // Act
            IEnumerable<Poll> closedPolls = await _pollsService.GetClosedPolls(null, futureStart, futureEnd);

            // Assert
            Assert.NotNull(closedPolls);
            Assert.Empty(closedPolls);
        }

        [Fact]
        public async Task GetClosedPolls_WithInvalidDateRange_StartAfterEnd_ReturnsEmpty()
        {
            // Arrange - Start date after end date
            DateTime startDate = DateTime.UtcNow.AddDays(-1);
            DateTime endDate = DateTime.UtcNow.AddDays(-5);

            // Act
            IEnumerable<Poll> closedPolls = await _pollsService.GetClosedPolls(null, startDate, endDate);

            // Assert
            Assert.NotNull(closedPolls);
            Assert.Empty(closedPolls);
        }

        [Fact]
        public async Task HasUserVotedAsync_WithNullUserId_ReturnsFalse()
        {
            // Act
            bool hasVoted = await _pollsService.HasUserVotedAsync(1, null!);

            // Assert
            Assert.False(hasVoted);
        }

        [Fact]
        public async Task HasUserVotedAsync_WithEmptyUserId_ReturnsFalse()
        {
            // Act
            bool hasVoted = await _pollsService.HasUserVotedAsync(1, "");

            // Assert
            Assert.False(hasVoted);
        }

        [Fact]
        public async Task HasUserVotedAsync_WithWhitespaceUserId_ReturnsFalse()
        {
            // Act
            bool hasVoted = await _pollsService.HasUserVotedAsync(1, "   ");

            // Assert
            Assert.False(hasVoted);
        }

        [Fact]
        public async Task VoteAsync_WithNullUserId_ThrowsException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAnyAsync<Exception>(() => _pollsService.VoteAsync(1, 1, null!));
        }

        [Fact]
        public async Task VoteAsync_ChangingVoteToSameOption_DoesNotThrow()
        {
            // Arrange - User-1 has already voted for option 1 in poll 1

            // Act - Vote for the same option again
            Vote vote = await _pollsService.VoteAsync(1, 1, "user-1");

            // Assert
            Assert.NotNull(vote);
            Assert.Equal(1, vote.PollOptionId);

            // Verify vote is still recorded correctly
            Vote? dbVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == "user-1" && v.PollId == 1);
            Assert.NotNull(dbVote);
            Assert.Equal(1, dbVote.PollOptionId);
        }

        [Fact]
        public async Task VoteAsync_WithNegativePollId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.VoteAsync(-1, 1, "user-1"));
        }

        [Fact]
        public async Task VoteAsync_WithZeroPollId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.VoteAsync(0, 1, "user-1"));
        }

        [Fact]
        public async Task VoteAsync_WithNegativeOptionId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.VoteAsync(1, -1, "user-1"));
        }

        [Fact]
        public async Task VoteAsync_WithZeroOptionId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.VoteAsync(1, 0, "user-1"));
        }

        [Fact]
        public async Task GetPollResultsAsync_WithNegativePollId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.GetPollResultsAsync(-1));
        }

        [Fact]
        public async Task GetPollResultsAsync_WithZeroPollId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.GetPollResultsAsync(0));
        }

        [Fact]
        public async Task GetVotersAsync_WithNegativePollId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.GetVotersAsync(-1));
        }

        [Fact]
        public async Task GetVotersAsync_WithZeroPollId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.GetVotersAsync(0));
        }

        [Fact]
        public async Task GetPollByIdAsync_WithNegativeId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.GetPollByIdAsync(-1));
        }

        [Fact]
        public async Task GetPollByIdAsync_WithZeroId_ThrowsEntityNotFoundException()
        {
            // Act & Assert
            _ = await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                _pollsService.GetPollByIdAsync(0));
        }

        [Fact]
        public async Task GetActivePolls_WithSpecialCharactersInFilter_HandlesCorrectly()
        {
            // Arrange - Create poll with special characters
            Poll specialPoll = new()
            {
                Id = 12,
                Question = "Poll with special characters: @#$%^&*()",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                CreatorId = "user-1",
                Options =
                [
                    new PollOption { Id = 23, Text = "Option 1" },
                    new PollOption { Id = 24, Text = "Option 2" }
                ]
            };

            _ = await _context.Polls.AddAsync(specialPoll);
            _ = await _context.SaveChangesAsync();

            // Act
            IEnumerable<Poll> polls = await _pollsService.GetActivePolls("@#$");

            // Assert
            Assert.NotNull(polls);
            _ = Assert.Single(polls);
            Assert.Contains("@#$%^&*()", polls.First().Question);
        }

        [Fact]
        public async Task GetActivePolls_WithUnicodeCharactersInFilter_HandlesCorrectly()
        {
            // Arrange - Create poll with unicode characters
            Poll unicodePoll = new()
            {
                Id = 13,
                Question = "测试 éáűőúöüó",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                CreatorId = "user-1",
                Options =
                [
                    new PollOption { Id = 25, Text = "Option 1" },
                    new PollOption { Id = 26, Text = "Option 2" }
                ]
            };

            _ = await _context.Polls.AddAsync(unicodePoll);
            _ = await _context.SaveChangesAsync();

            // Act
            IEnumerable<Poll> polls = await _pollsService.GetActivePolls("测试");

            // Assert
            Assert.NotNull(polls);
            _ = Assert.Single(polls);
            Assert.Contains("测试", polls.First().Question);
        }

        [Fact]
        public async Task GetClosedPolls_WithVeryLongQuestionFilter_HandlesCorrectly()
        {
            // Arrange - Create poll with very long question
            string longQuestion = new('A', 1000); // 1000 character question
            Poll longQuestionPoll = new()
            {
                Id = 15,
                Question = $"Poll with long question: {longQuestion}",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(-1),
                CreatorId = "user-1",
                Options =
                [
                    new PollOption { Id = 32, Text = "Option 1" },
                    new PollOption { Id = 33, Text = "Option 2" }
                ]
            };

            _ = await _context.Polls.AddAsync(longQuestionPoll);
            _ = await _context.SaveChangesAsync();

            // Act
            IEnumerable<Poll> polls = await _pollsService.GetClosedPolls(longQuestion[..100]); // Use first 100 chars

            // Assert
            Assert.NotNull(polls);
            _ = Assert.Single(polls);
            Assert.Contains(longQuestion[..100], polls.First().Question);
        }

        [Fact]
        public async Task GetUserVoteAsync_WithNonExistentPoll_ReturnsNull()
        {
            // Act
            Vote? userVote = await _pollsService.GetUserVoteAsync(999, "user-1");

            // Assert
            Assert.Null(userVote);
        }

        [Fact]
        public async Task GetUserVoteAsync_WithNonExistentUser_ReturnsNull()
        {
            // Act
            Vote? userVote = await _pollsService.GetUserVoteAsync(1, "non-existent-user");

            // Assert
            Assert.Null(userVote);
        }

        [Fact]
        public async Task GetUserVoteAsync_WhenUserHasVoted_ReturnsCorrectVote()
        {
            // Act
            Vote? userVote = await _pollsService.GetUserVoteAsync(1, "user-1");

            // Assert
            Assert.NotNull(userVote);
            Assert.Equal(1, userVote.PollId);
            Assert.Equal("user-1", userVote.UserId);
        }

        [Fact]
        public async Task GetUserVoteAsync_WithNullUserId_ReturnsNull()
        {
            // Act
            Vote? userVote = await _pollsService.GetUserVoteAsync(1, null!);

            // Assert
            Assert.Null(userVote);
        }

        [Fact]
        public async Task GetUserVoteAsync_WithEmptyUserId_ReturnsNull()
        {
            // Act
            Vote? userVote = await _pollsService.GetUserVoteAsync(1, "");

            // Assert
            Assert.Null(userVote);
        }

        #endregion
    }
}
