using System;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Models;

namespace OGRALAB.Data
{
    public class OgralabDbContext : DbContext
    {
        public OgralabDbContext(DbContextOptions<OgralabDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.Role).HasMaxLength(50);
            });

            // Configure UserSettings entity
            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(50);
            });

            // Seed default admin user
            var defaultAdmin = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@ogralab.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Role = "Administrator"
            };

            modelBuilder.Entity<User>().HasData(defaultAdmin);
        }
    }
}
