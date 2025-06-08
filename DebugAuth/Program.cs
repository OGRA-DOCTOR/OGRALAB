using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// Define minimal versions of the models and context to test
namespace OGRALAB.Models
{
    public enum UserRole
    {
        Admin = 1,
        Technician = 2,
        User = 3
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Test
    {
        public int Id { get; set; }
        public string TestCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UserSettings
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public DateTime? RememberMeExpiry { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

namespace OGRALAB.Data
{
    public class OgralabDbContext : DbContext
    {
        public DbSet<OGRALAB.Models.User> Users { get; set; }
        public DbSet<OGRALAB.Models.Test> Tests { get; set; }
        public DbSet<OGRALAB.Models.UserSettings> UserSettings { get; set; }

        public OgralabDbContext(DbContextOptions<OgralabDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<OGRALAB.Models.User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Role).HasConversion<int>();
            });

            // Test entity configuration
            modelBuilder.Entity<OGRALAB.Models.Test>(entity =>
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
            modelBuilder.Entity<OGRALAB.Models.UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            });

            // Seed data
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            modelBuilder.Entity<OGRALAB.Models.User>().HasData(
                new OGRALAB.Models.User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    FullName = "مدير النظام",
                    Email = "admin@ogralab.com",
                    Role = OGRALAB.Models.UserRole.Admin,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );

            modelBuilder.Entity<OGRALAB.Models.Test>().HasData(
                new OGRALAB.Models.Test { Id = 1, TestCode = "CBC", TestName = "تعداد الدم الكامل", Category = "أمراض الدم", Unit = "10^3/μL", Price = 50, IsActive = true, CreatedDate = DateTime.Now, DisplayOrder = 1 },
                new OGRALAB.Models.Test { Id = 2, TestCode = "RBS", TestName = "السكر العشوائي", Category = "كيمياء الدم", Unit = "mg/dL", Price = 30, IsActive = true, CreatedDate = DateTime.Now, DisplayOrder = 2 }
            );
        }
    }
}

namespace OGRALAB.Services
{
    public class AuthenticationService
    {
        private readonly OGRALAB.Data.OgralabDbContext _context;

        public AuthenticationService(OGRALAB.Data.OgralabDbContext context)
        {
            _context = context;
        }

        public async Task<OGRALAB.Models.User?> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

            if (user == null)
                return null;

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isValidPassword ? user : null;
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== OGRALAB Authentication Debug Tool ===");
        
        // Create DbContext
        var options = new DbContextOptionsBuilder<OGRALAB.Data.OgralabDbContext>()
            .UseSqlite("Data Source=ogralab_debug.db")
            .Options;
            
        using var context = new OGRALAB.Data.OgralabDbContext(options);
        
        Console.WriteLine("1. Ensuring database exists...");
        await context.Database.EnsureDeletedAsync(); // Delete first to start fresh
        await context.Database.EnsureCreatedAsync();
        
        Console.WriteLine("2. Checking if admin user exists...");
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        
        if (adminUser == null)
        {
            Console.WriteLine("   ERROR: Admin user not found!");
            return;
        }
        
        Console.WriteLine($"   Admin user found: {adminUser.Username}");
        Console.WriteLine($"   Full Name: {adminUser.FullName}");
        Console.WriteLine($"   Email: {adminUser.Email}");
        Console.WriteLine($"   Role: {adminUser.Role}");
        Console.WriteLine($"   IsActive: {adminUser.IsActive}");
        Console.WriteLine($"   Password Hash: {adminUser.PasswordHash}");
        
        Console.WriteLine("3. Testing password verification...");
        
        var authService = new OGRALAB.Services.AuthenticationService(context);
        
        // Test correct password
        Console.WriteLine("   Testing with 'Admin@123'...");
        var result1 = await authService.AuthenticateAsync("admin", "Admin@123");
        Console.WriteLine($"   Result: {(result1 != null ? "SUCCESS" : "FAILED")}");
        
        // Test wrong password
        Console.WriteLine("   Testing with 'wrong_password'...");
        var result2 = await authService.AuthenticateAsync("admin", "wrong_password");
        Console.WriteLine($"   Result: {(result2 != null ? "SUCCESS" : "FAILED")}");
        
        // Test direct BCrypt verification
        Console.WriteLine("4. Testing direct BCrypt verification...");
        bool bcryptResult = BCrypt.Net.BCrypt.Verify("Admin@123", adminUser.PasswordHash);
        Console.WriteLine($"   BCrypt.Verify('Admin@123', hash): {bcryptResult}");
        
        // Generate new hash for comparison
        Console.WriteLine("5. Generating new hash for 'Admin@123'...");
        string newHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        Console.WriteLine($"   New hash: {newHash}");
        bool newHashVerify = BCrypt.Net.BCrypt.Verify("Admin@123", newHash);
        Console.WriteLine($"   New hash verification: {newHashVerify}");
        
        Console.WriteLine("=== Debug Complete ===");
    }
}
