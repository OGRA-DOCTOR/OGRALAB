using System;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Models;
using OGRALAB.Enums;

namespace OGRALAB.Data
{
    public class OgralabDbContext : DbContext
    {
        public OgralabDbContext(DbContextOptions<OgralabDbContext> options) : base(options)
        {
        }

        // --- DbSets (الجداول) ---
        public DbSet<User> Users { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestRequest> TestRequests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<TestReferenceRange> TestReferenceRanges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. إعدادات User ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired(); // هذه صحيحة لأن نموذج User يستخدم CreatedAt
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
            });

            // --- 2. إعدادات Test (مصححة) ---
            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TestName).IsRequired().HasMaxLength(200); // تم التصحيح من Name إلى TestName
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)"); // تم استخدام النوع من الكود الأصلي
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                // --- هذا هو السطر الذي تم تصحيحه ---
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasMany(e => e.ReferenceRanges)
                      .WithOne(e => e.Test)
                      .HasForeignKey(e => e.TestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- 3. إعدادات TestReferenceRange ---
            modelBuilder.Entity<TestReferenceRange>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // --- 4. الحفاظ على الإعدادات القديمة ---
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasIndex(e => e.PatientCode).IsUnique();
                entity.Property(e => e.PatientCode).HasMaxLength(12).IsRequired();
            });

            modelBuilder.Entity<TestRequest>(entity =>
            {
                entity.HasOne(tr => tr.Test)
                      .WithMany(t => t.TestRequests)
                      .HasForeignKey(tr => tr.TestId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasOne(result => result.Test)
                      .WithMany(t => t.TestResults)
                      .HasForeignKey(result => result.TestId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- 5. البيانات الأولية (Seeding) ---
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    FullName = "System Administrator"
                }
            );

            SeedSampleData(modelBuilder);
        }

        private void SeedSampleData(ModelBuilder modelBuilder)
        {
            // ... (بيانات Doctor و Entity كما هي) ...

            // --- بيانات Test الأولية (مصححة) ---
            modelBuilder.Entity<Test>().HasData(
                new Test { Id = 1, TestName = "تعداد الدم الكامل", Category = "أمراض الدم", Unit = "10^3/μL", Price = 50, IsActive = true, CreatedDate = DateTime.Now },
                new Test { Id = 2, TestName = "السكر العشوائي", Category = "كيمياء الدم", Unit = "mg/dL", Price = 30, IsActive = true, CreatedDate = DateTime.Now },
                new Test { Id = 3, TestName = "الكولسترول الكلي", Category = "كيمياء الدم", Unit = "mg/dL", Price = 40, IsActive = true, CreatedDate = DateTime.Now }
            );
        }
    }
}