using AutoMapper;
using Voter.DataAccess.Models;
using Voter.Shared.Models;

namespace Voter.WebAPI.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            _ = CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            // Poll mappings
            _ = CreateMap<Poll, PollDTO>()
                .ForMember(dest => dest.CreatorEmail, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Email : string.Empty))
                .ForMember(dest => dest.UserHasVoted, opt => opt.Ignore());

            _ = CreateMap<CreatePollRequestDTO, Poll>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Options, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Votes, opt => opt.Ignore());

            // PollOption mappings
            _ = CreateMap<PollOption, PollOptionDTO>();
        }
    }
}
