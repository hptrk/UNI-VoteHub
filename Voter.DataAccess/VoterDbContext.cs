using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Voter.DataAccess.Models;

namespace Voter.DataAccess
{
    public class VoterDbContext(DbContextOptions<VoterDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<Vote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // relationships
            _ = builder.Entity<Poll>()
                .HasOne(p => p.Creator)
                .WithMany(u => u.CreatedPolls)
                .HasForeignKey(p => p.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.Entity<PollOption>()
                .HasOne(po => po.Poll)
                .WithMany(p => p.Options)
                .HasForeignKey(po => po.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.Entity<Vote>()
                .HasOne(v => v.Poll)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.Entity<Vote>()
                .HasOne(v => v.PollOption)
                .WithMany(po => po.Votes)
                .HasForeignKey(v => v.PollOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // unique constraint to prevent multiple votes from the same user on a poll
            _ = builder.Entity<Vote>()
                .HasIndex(v => new { v.PollId, v.UserId })
                .IsUnique();
        }
    }
}
