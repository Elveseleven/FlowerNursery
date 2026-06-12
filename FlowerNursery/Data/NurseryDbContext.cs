using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FlowerNursery.Models;

namespace FlowerNursery.Data
{
    public class NurseryDbContext : IdentityDbContext<IdentityUser>
    {
        public NurseryDbContext(DbContextOptions<NurseryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Greenhouse> Greenhouses { get; set; }
        public DbSet<FlowerGroup> FlowerGroups { get; set; }
        public DbSet<WateringSchedule> WateringSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Greenhouse -> FlowerGroups (cascade delete)
            modelBuilder.Entity<FlowerGroup>()
                .HasOne(fg => fg.Greenhouse)
                .WithMany(g => g.FlowerGroups)
                .HasForeignKey(fg => fg.GreenhouseId)
                .OnDelete(DeleteBehavior.Cascade);

            // FlowerGroup -> WateringSchedules (cascade delete)
            modelBuilder.Entity<WateringSchedule>()
                .HasOne(ws => ws.FlowerGroup)
                .WithMany(fg => fg.WateringSchedules)
                .HasForeignKey(ws => ws.FlowerGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
