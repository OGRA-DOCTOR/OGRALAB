using Microsoft.EntityFrameworkCore;
using OGRALAB.Models;
using OGRALAB.Enums;
using System;

namespace OGRALAB.Data
{
    public class OgralabDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<TestRequest> TestRequests { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<TestReferenceRange> TestReferenceRanges { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }

        public OgralabDbContext(DbContextOptions<OgralabDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Role).HasConversion<int>();
            });

            // Patient entity configuration
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PatientCode).IsRequired().HasMaxLength(12);
                entity.Property(e => e.Title).HasMaxLength(20);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MobileNumber).HasMaxLength(11);
                entity.Property(e => e.Gender).HasConversion<int>();
                entity.Property(e => e.AgeUnit).HasConversion<int>();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.AmountAfterDiscount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.RemainingAmount).HasColumnType("decimal(10,2)");
            });

            // Test entity configuration
            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TestCode).IsUnique();
                entity.Property(e => e.TestCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TestName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            });

            // UserSettings entity configuration
            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // --- بيانات User الأولية ---
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    FullName = "مدير النظام",
                    Email = "admin@ogralab.com",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            // --- بيانات Test الأولية ---
            modelBuilder.Entity<Test>().HasData(
                new Test { Id = 1, TestCode = "CBC", TestName = "تعداد الدم الكامل", Category = "أمراض الدم", Unit = "10^3/μL", Price = 50, IsActive = true, CreatedDate = DateTime.Now, DisplayOrder = 1 },
                new Test { Id = 2, TestCode = "RBS", TestName = "السكر العشوائي", Category = "كيمياء الدم", Unit = "mg/dL", Price = 30, IsActive = true, CreatedDate = DateTime.Now, DisplayOrder = 2 },
                new Test { Id = 3, TestCode = "CHOL", TestName = "الكولسترول الكلي", Category = "كيمياء الدم", Unit = "mg/dL", Price = 40, IsActive = true, CreatedDate = DateTime.Now, DisplayOrder = 3 }
            );
        }
    }
}
