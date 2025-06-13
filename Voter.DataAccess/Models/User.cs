using Microsoft.AspNetCore.Identity;

namespace Voter.DataAccess.Models
{
    public class User : IdentityUser
    {
        public virtual ICollection<Poll> CreatedPolls { get; set; } = [];
        public virtual ICollection<Vote> Votes { get; set; } = [];
    }
}
