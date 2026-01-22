using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Text> Texts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RequestHistory> RequestHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Text>(entity =>
            {
                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasMany(e => e.RequestHistories)
                      .WithOne(e => e.User)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RequestHistory>(entity =>
            {
                entity.HasOne(e => e.User)
                      .WithMany(e => e.RequestHistories)
                      .HasForeignKey(e => e.UserId);
            });


            modelBuilder.Entity<Text>(entity =>
            {
                entity.HasOne(e => e.User)
                      .WithMany() 
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}