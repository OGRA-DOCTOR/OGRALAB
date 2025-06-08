using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Services;
using OGRALAB.Models;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== OGRALAB Authentication Debug Tool ===");
        
        // Create DbContext
        var options = new DbContextOptionsBuilder<OgralabDbContext>()
            .UseSqlite("Data Source=./Data/ogralab.db")
            .Options;
            
        using var context = new OgralabDbContext(options);
        
        Console.WriteLine("1. Ensuring database exists...");
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
        
        var authService = new AuthenticationService(context);
        
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
