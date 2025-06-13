using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Voter.DataAccess.Models;
using Voter.DataAccess.Services;
using Voter.Shared.Models;

namespace Voter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PollsController(IPollsService pollsService, IMapper mapper) : ControllerBase
    {
        private readonly IPollsService _pollsService = pollsService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<PollDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivePolls([FromQuery] GetActivePollsRequestDTO request)
        {
            IEnumerable<Poll> activePolls = await _pollsService.GetActivePolls(request.QuestionFilter); string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            IEnumerable<PollDTO> pollDTOs = _mapper.Map<IEnumerable<PollDTO>>(activePolls);

            foreach (PollDTO pollDTO in pollDTOs)
            {
                pollDTO.UserHasVoted = await _pollsService.HasUserVotedAsync(pollDTO.Id, userId);

                // If the user has voted, mark which option they voted for
                if (pollDTO.UserHasVoted)
                {
                    Vote? userVote = await _pollsService.GetUserVoteAsync(pollDTO.Id, userId);
                    if (userVote != null)
                    {
                        // Mark the option the user voted for
                        PollOptionDTO? votedOption = pollDTO.Options.FirstOrDefault(o => o.Id == userVote.PollOptionId);
                        if (votedOption != null)
                        {
                            votedOption.UserVoted = true;
                        }
                    }
                }
            }

            return Ok(pollDTOs);
        }
        [HttpGet("closed")]
        [ProducesResponseType(typeof(IEnumerable<PollDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClosedPolls([FromQuery] GetClosedPollsRequestDTO request)
        {
            IEnumerable<Poll> closedPolls = await _pollsService.GetClosedPolls(
                request.QuestionFilter,
                request.StartDate,
                request.EndDate);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            IEnumerable<PollDTO> pollDTOs = _mapper.Map<IEnumerable<PollDTO>>(closedPolls);

            // Add user vote information to each poll
            foreach (PollDTO pollDTO in pollDTOs)
            {
                pollDTO.UserHasVoted = await _pollsService.HasUserVotedAsync(pollDTO.Id, userId);

                // If the user has voted, mark which option they voted for
                if (pollDTO.UserHasVoted)
                {
                    Vote? userVote = await _pollsService.GetUserVoteAsync(pollDTO.Id, userId);
                    if (userVote != null)
                    {
                        // Mark the option the user voted for
                        PollOptionDTO? votedOption = pollDTO.Options.FirstOrDefault(o => o.Id == userVote.PollOptionId);
                        if (votedOption != null)
                        {
                            votedOption.UserVoted = true;
                        }
                    }
                }
            }

            return Ok(pollDTOs);
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(IEnumerable<PollDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPolls()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            IEnumerable<Poll> userPolls = await _pollsService.GetUserPolls(userId);

            return Ok(_mapper.Map<IEnumerable<PollDTO>>(userPolls));
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PollDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPoll(int id)
        {
            try
            {
                Poll poll = await _pollsService.GetPollByIdAsync(id);
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                PollDTO pollDTO = _mapper.Map<PollDTO>(poll);
                pollDTO.UserHasVoted = await _pollsService.HasUserVotedAsync(pollDTO.Id, userId);

                // If the user has voted, mark which option they voted for
                if (pollDTO.UserHasVoted)
                {
                    Vote? userVote = await _pollsService.GetUserVoteAsync(id, userId);
                    if (userVote != null)
                    {
                        // Mark the option the user voted for
                        PollOptionDTO? votedOption = pollDTO.Options.FirstOrDefault(o => o.Id == userVote.PollOptionId);
                        if (votedOption != null)
                        {
                            votedOption.UserVoted = true;
                        }
                    }
                }

                return Ok(pollDTO);
            }
            catch (DataAccess.Exceptions.EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(PollDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequestDTO createPollDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            Poll poll = _mapper.Map<Poll>(createPollDTO);
            poll.CreatorId = userId;

            // Add options to the poll
            foreach (string optionText in createPollDTO.Options)
            {
                poll.Options.Add(new PollOption
                {
                    Text = optionText
                });
            }

            Poll createdPoll = await _pollsService.CreatePollAsync(poll);
            PollDTO pollDTO = _mapper.Map<PollDTO>(createdPoll);

            return CreatedAtAction(nameof(GetPoll), new { id = pollDTO.Id }, pollDTO);
        }

        [HttpPost("vote")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Vote([FromBody] VoteRequestDTO voteRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                _ = await _pollsService.VoteAsync(voteRequest.PollId, voteRequest.OptionId, userId);

                return Ok();
            }
            catch (DataAccess.Exceptions.EntityNotFoundException)
            {
                return NotFound();
            }
            catch (DataAccess.Exceptions.PollNotActiveException)
            {
                return BadRequest("This poll is not currently active.");
            }
            // Removed VoteAlreadyCastException handler since the service now updates existing votes
        }
        [HttpGet("{id}/results")]
        [ProducesResponseType(typeof(PollResultDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPollResults(int id)
        {
            try
            {
                Poll poll = await _pollsService.GetPollByIdAsync(id);
                Dictionary<PollOption, int> results = await _pollsService.GetPollResultsAsync(id);
                int totalVotes = results.Values.Sum();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                // Get the user's vote if they've voted on this poll
                Vote? userVote = await _pollsService.GetUserVoteAsync(id, userId);

                PollResultDTO pollResultDTO = new()
                {
                    PollId = poll.Id,
                    Question = poll.Question,
                    StartDate = poll.StartDate,
                    EndDate = poll.EndDate,
                    CreatorUsername = poll.Creator?.UserName ?? string.Empty,
                    CreatorEmail = poll.Creator?.Email ?? string.Empty,
                    TotalVotes = totalVotes,
                    UserVoteOptionId = userVote?.PollOptionId,
                    Results = []
                };
                foreach (KeyValuePair<PollOption, int> result in results)
                {
                    double percentage = totalVotes > 0
                        ? (double)result.Value / totalVotes * 100
                        : 0;

                    pollResultDTO.Results.Add(new OptionResultDTO
                    {
                        OptionId = result.Key.Id,
                        OptionText = result.Key.Text,
                        VoteCount = result.Value,
                        Percentage = Math.Round(percentage, 2)
                    });
                }

                return Ok(pollResultDTO);
            }
            catch (DataAccess.Exceptions.EntityNotFoundException)
            {
                return NotFound();
            }
        }
        [HttpGet("{id}/voters")]
        [ProducesResponseType(typeof(IEnumerable<UserDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVoters(int id)
        {
            try
            {
                IEnumerable<User> voters = await _pollsService.GetVotersAsync(id);
                return Ok(_mapper.Map<IEnumerable<UserDTO>>(voters));
            }
            catch (DataAccess.Exceptions.EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
