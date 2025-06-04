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
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestRequest> TestRequests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

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

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasIndex(e => e.PatientCode).IsUnique();
                entity.Property(e => e.PatientCode).HasMaxLength(12).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(20);
                entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.MobileNumber).HasMaxLength(11);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.AmountAfterDiscount).HasPrecision(18, 2);
                entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
                entity.Property(e => e.RemainingAmount).HasPrecision(18, 2);

                // Configure relationships
                entity.HasOne(p => p.Doctor)
                      .WithMany(d => d.Patients)
                      .HasForeignKey(p => p.DoctorId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Entity)
                      .WithMany(e => e.Patients)
                      .HasForeignKey(p => p.EntityId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Doctor entity
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // Configure Entity entity
            modelBuilder.Entity<Entity>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.ResponsiblePerson).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.FaxNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // Configure Test entity
            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasIndex(e => e.TestCode).IsUnique();
                entity.Property(e => e.TestCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.TestName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Abbreviation).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.SampleType).HasMaxLength(100);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.NormalRangeMale).HasMaxLength(100);
                entity.Property(e => e.NormalRangeFemale).HasMaxLength(100);
                entity.Property(e => e.NormalRangeChildren).HasMaxLength(100);
                entity.Property(e => e.MinNormalValue).HasPrecision(18, 4);
                entity.Property(e => e.MaxNormalValue).HasPrecision(18, 4);
                entity.Property(e => e.CriticalLowValue).HasPrecision(18, 4);
                entity.Property(e => e.CriticalHighValue).HasPrecision(18, 4);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Preparation).HasMaxLength(500);
            });

            // Configure TestRequest entity
            modelBuilder.Entity<TestRequest>(entity =>
            {
                entity.Property(e => e.PaidPrice).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Configure relationships
                entity.HasOne(tr => tr.Patient)
                      .WithMany(p => p.TestRequests)
                      .HasForeignKey(tr => tr.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.Test)
                      .WithMany(t => t.TestRequests)
                      .HasForeignKey(tr => tr.TestId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tr => tr.TestResult)
                      .WithOne(result => result.TestRequest)
                      .HasForeignKey<TestResult>(result => result.TestRequestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TestResult entity
            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.Property(e => e.TextResult).HasMaxLength(500);
                entity.Property(e => e.NumericResult).HasPrecision(18, 4);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.AppliedNormalRange).HasMaxLength(100);
                entity.Property(e => e.Comments).HasMaxLength(1000);
                entity.Property(e => e.EnteredBy).HasMaxLength(100);
                entity.Property(e => e.ReviewedBy).HasMaxLength(100);

                // Configure relationships
                entity.HasOne(result => result.Test)
                      .WithMany(t => t.TestResults)
                      .HasForeignKey(result => result.TestId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed sample data
            SeedSampleData(modelBuilder);
        }

        private void SeedSampleData(ModelBuilder modelBuilder)
        {
            // Seed sample doctors
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { Id = 1, FullName = "د. أحمد محمد", Specialization = "طب عام", PhoneNumber = "01234567890", IsActive = true, CreatedDate = DateTime.Now },
                new Doctor { Id = 2, FullName = "د. فاطمة علي", Specialization = "أمراض باطنة", PhoneNumber = "01234567891", IsActive = true, CreatedDate = DateTime.Now },
                new Doctor { Id = 3, FullName = "د. محمد حسن", Specialization = "أطفال", PhoneNumber = "01234567892", IsActive = true, CreatedDate = DateTime.Now }
            );

            // Seed sample entities
            modelBuilder.Entity<Entity>().HasData(
                new Entity { Id = 1, Name = "عيادة الأمل", Type = "عيادة", Address = "شارع الملك فهد", ResponsiblePerson = "أحمد محمد", PhoneNumber = "011234567", IsActive = true, CreatedDate = DateTime.Now },
                new Entity { Id = 2, Name = "مستشفى النور", Type = "مستشفى", Address = "طريق الملك عبدالعزيز", ResponsiblePerson = "فاطمة علي", PhoneNumber = "011234568", IsActive = true, CreatedDate = DateTime.Now },
                new Entity { Id = 3, Name = "مركز الشفاء الطبي", Type = "مركز طبي", Address = "حي الملز", ResponsiblePerson = "محمد حسن", PhoneNumber = "011234569", IsActive = true, CreatedDate = DateTime.Now }
            );

            // Seed sample tests
            modelBuilder.Entity<Test>().HasData(
                new Test { Id = 1, TestCode = "CBC", TestName = "تعداد الدم الكامل", Abbreviation = "CBC", Category = "أمراض الدم", SampleType = "دم", Unit = "10^3/μL", NormalRangeMale = "4.5-11.0", NormalRangeFemale = "4.5-11.0", NormalRangeChildren = "4.0-10.0", MinNormalValue = 4.5m, MaxNormalValue = 11.0m, CriticalLowValue = 2.0m, CriticalHighValue = 20.0m, Price = 50, IsActive = true, DisplayOrder = 1, CreatedDate = DateTime.Now },
                new Test { Id = 2, TestCode = "GLU", TestName = "السكر العشوائي", Abbreviation = "Glucose", Category = "كيمياء الدم", SampleType = "دم", Unit = "mg/dL", NormalRangeMale = "70-140", NormalRangeFemale = "70-140", NormalRangeChildren = "70-140", MinNormalValue = 70, MaxNormalValue = 140, CriticalLowValue = 40, CriticalHighValue = 400, Price = 30, IsActive = true, DisplayOrder = 2, CreatedDate = DateTime.Now },
                new Test { Id = 3, TestCode = "CHOL", TestName = "الكولسترول الكلي", Abbreviation = "Cholesterol", Category = "كيمياء الدم", SampleType = "دم", Unit = "mg/dL", NormalRangeMale = "<200", NormalRangeFemale = "<200", NormalRangeChildren = "<170", MinNormalValue = 0, MaxNormalValue = 200, CriticalLowValue = 0, CriticalHighValue = 300, Price = 40, IsActive = true, DisplayOrder = 3, CreatedDate = DateTime.Now },
                new Test { Id = 4, TestCode = "HbA1c", TestName = "السكر التراكمي", Abbreviation = "HbA1c", Category = "كيمياء الدم", SampleType = "دم", Unit = "%", NormalRangeMale = "<5.7", NormalRangeFemale = "<5.7", NormalRangeChildren = "<5.7", MinNormalValue = 0, MaxNormalValue = 5.7m, CriticalLowValue = 0, CriticalHighValue = 15, Price = 80, IsActive = true, DisplayOrder = 4, CreatedDate = DateTime.Now },
                new Test { Id = 5, TestCode = "TSH", TestName = "هرمون الغدة الدرقية", Abbreviation = "TSH", Category = "الهرمونات", SampleType = "دم", Unit = "mIU/L", NormalRangeMale = "0.4-4.0", NormalRangeFemale = "0.4-4.0", NormalRangeChildren = "0.7-6.4", MinNormalValue = 0.4m, MaxNormalValue = 4.0m, CriticalLowValue = 0.1m, CriticalHighValue = 20, Price = 100, IsActive = true, DisplayOrder = 5, CreatedDate = DateTime.Now }
            );
        }
    }
}
