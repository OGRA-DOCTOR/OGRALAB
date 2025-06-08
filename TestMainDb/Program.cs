using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OGRALAB.Data;
using OGRALAB.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Testing Main Application Database Flow ===");
        
        try
        {
            var configuration = BuildConfiguration();
            var host = CreateHostBuilder(configuration).Build();
            await host.StartAsync();
            
            await InitializeDatabaseAsync(host.Services);
            
            // Test authentication after database initialization
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OgralabDbContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            
            Console.WriteLine("Testing authentication...");
            var result = await authService.AuthenticateAsync("admin", "Admin@123");
            Console.WriteLine($"Authentication result: {(result != null ? "SUCCESS" : "FAILED")}");
            
            if (result != null)
            {
                Console.WriteLine($"User: {result.Username}, Role: {result.Role}");
            }
            else
            {
                // Check if admin user exists in database
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser == null)
                {
                    Console.WriteLine("Admin user not found in database!");
                }
                else
                {
                    Console.WriteLine($"Admin user found but authentication failed. User active: {adminUser.IsActive}");
                    Console.WriteLine($"Password hash: {adminUser.PasswordHash}");
                    
                    // Test direct BCrypt verification
                    bool directVerify = BCrypt.Net.BCrypt.Verify("Admin@123", adminUser.PasswordHash);
                    Console.WriteLine($"Direct BCrypt verification: {directVerify}");
                }
            }
            
            await host.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
    
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    private static IHostBuilder CreateHostBuilder(IConfiguration configuration) =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(configuration);
                services.AddDbContext<OgralabDbContext>(options =>
                    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

                services.AddScoped<IAuthenticationService, AuthenticationService>();
            });
    
    private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Initializing database...");
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OgralabDbContext>();

            var connectionString = context.Database.GetConnectionString();
            Console.WriteLine($"Connection string: {connectionString}");
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                var dataSource = GetDataSourceFromConnectionString(connectionString);
                if (dataSource != null)
                {
                    var directory = Path.GetDirectoryName(dataSource);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }
            }
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Database initialized successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }

    private static string? GetDataSourceFromConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) return null;
        
        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            if (trimmedPart.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                return trimmedPart.Substring("Data Source=".Length);
            }
        }
        return null;
    }
}
