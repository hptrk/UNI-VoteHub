using Microsoft.EntityFrameworkCore;
using Voter.DataAccess.Exceptions;
using Voter.DataAccess.Models;

namespace Voter.DataAccess.Services
{
    public class PollsService(VoterDbContext dbContext) : IPollsService
    {
        private readonly VoterDbContext _dbContext = dbContext;

        public async Task<IEnumerable<Poll>> GetAllPollsAsync()
        {
            return await _dbContext.Polls
                .Include(p => p.Options)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<Poll>> GetActivePolls(string? questionFilter = null)
        {
            DateTime now = DateTime.UtcNow;
            IQueryable<Poll> query = _dbContext.Polls
                .Where(p => p.StartDate <= now && p.EndDate > now);

            if (!string.IsNullOrWhiteSpace(questionFilter))
            {
                query = query.Where(p => p.Question.Contains(questionFilter));
            }

            return await query
                .Include(p => p.Options)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Poll>> GetClosedPolls(string? questionFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime now = DateTime.UtcNow;
            IQueryable<Poll> query = _dbContext.Polls
                .Where(p => p.EndDate <= now);

            if (!string.IsNullOrWhiteSpace(questionFilter))
            {
                query = query.Where(p => p.Question.Contains(questionFilter));
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.EndDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.EndDate <= endDate.Value);
            }

            return await query
                .Include(p => p.Options)
                .OrderByDescending(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Poll>> GetUserPolls(string userId)
        {
            return await _dbContext.Polls
                .Where(p => p.CreatorId == userId)
                .Include(p => p.Options)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Poll> GetPollByIdAsync(int id)
        {
            Poll? poll = await _dbContext.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            return poll ?? throw new EntityNotFoundException(nameof(Poll), id);
        }
        public async Task<Poll> CreatePollAsync(Poll poll)
        {
            // Validate poll dates
            DateTime today = DateTime.UtcNow.Date;

            if (poll.StartDate.Date < today)
            {
                throw new ArgumentException("Start date cannot be in the past.");
            }

            if (poll.EndDate <= poll.StartDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }

            // Ensure minimum duration (15 minutes)
            TimeSpan minDuration = TimeSpan.FromMinutes(15);
            if (poll.EndDate - poll.StartDate < minDuration)
            {
                throw new ArgumentException($"Poll duration must be at least {minDuration.TotalMinutes} minutes.");
            }

            // Validate options
            if (poll.Options.Count < 2)
            {
                throw new ArgumentException("Poll must have at least 2 options.");
            }

            _ = _dbContext.Polls.Add(poll);

            try
            {
                _ = await _dbContext.SaveChangesAsync();
                return poll;
            }
            catch (Exception ex)
            {
                throw new SaveFailedException("Failed to create poll", ex);
            }
        }

        public async Task<bool> HasUserVotedAsync(int pollId, string userId)
        {
            return await _dbContext.Votes
                .AnyAsync(v => v.PollId == pollId && v.UserId == userId);
        }

        public async Task<Vote> VoteAsync(int pollId, int optionId, string userId)
        {
            Poll? poll = await _dbContext.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId) ?? throw new EntityNotFoundException(nameof(Poll), pollId);
            DateTime now = DateTime.UtcNow;
            if (poll.StartDate > now || poll.EndDate < now)
            {
                throw new PollNotActiveException();
            }

            PollOption? option = poll.Options.FirstOrDefault(o => o.Id == optionId) ?? throw new EntityNotFoundException(nameof(PollOption), optionId);
            Vote? existingVote = await _dbContext.Votes
                .FirstOrDefaultAsync(v => v.PollId == pollId && v.UserId == userId);

            if (existingVote != null)
            {
                // Update the existing vote instead of throwing an exception
                existingVote.PollOptionId = optionId;
                existingVote.VoteDate = DateTime.UtcNow;

                try
                {
                    _ = await _dbContext.SaveChangesAsync();
                    return existingVote;
                }
                catch (Exception ex)
                {
                    throw new SaveFailedException("Failed to update vote", ex);
                }
            }

            Vote vote = new()
            {
                PollId = pollId,
                PollOptionId = optionId,
                UserId = userId,
                VoteDate = DateTime.UtcNow
            };

            _ = _dbContext.Votes.Add(vote);

            try
            {
                _ = await _dbContext.SaveChangesAsync();
                return vote;
            }
            catch (Exception ex)
            {
                throw new SaveFailedException("Failed to save vote", ex);
            }
        }

        public async Task<Vote?> GetUserVoteAsync(int pollId, string userId)
        {
            return await _dbContext.Votes
                .FirstOrDefaultAsync(v => v.PollId == pollId && v.UserId == userId);
        }

        public async Task<IEnumerable<User>> GetVotersAsync(int pollId)
        {
            // Check if the poll exists
            Poll? poll = await _dbContext.Polls.FindAsync(pollId);
            return poll == null
                ? throw new EntityNotFoundException(nameof(Poll), pollId)
                : (IEnumerable<User>)await _dbContext.Votes
                .Where(v => v.PollId == pollId)
                .Include(v => v.User)
                .Select(v => v.User!)
                .ToListAsync();
        }

        public async Task<Dictionary<PollOption, int>> GetPollResultsAsync(int pollId)
        {
            Poll? poll = await _dbContext.Polls
                .Include(p => p.Options)
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == pollId) ?? throw new EntityNotFoundException(nameof(Poll), pollId);
            Dictionary<PollOption, int> results = [];

            // Count votes for each option
            foreach (PollOption option in poll.Options)
            {
                int voteCount = await _dbContext.Votes
                    .CountAsync(v => v.PollOptionId == option.Id);

                results.Add(option, voteCount);
            }

            return results;
        }
    }
}
