using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using OGRALAB.Data;
using OGRALAB.Services;
using OGRALAB.ViewModels;
using OGRALAB.Views;

namespace OGRALAB
{
    public partial class App : Application
    {
        private IHost? _host;
        private IServiceProvider? _serviceProvider;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Check for single instance
            if (!SingleInstanceService.IsFirstInstance())
            {
                MessageBox.Show("OGRALAB is already running.", "Application Already Running", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
                SingleInstanceService.BringExistingInstanceToFront();
                Shutdown();
                return;
            }

            try
            {
                // Build configuration
                var configuration = BuildConfiguration();

                // Build host with services
                _host = CreateHostBuilder(configuration).Build();
                _serviceProvider = _host.Services;

                // Initialize database
                await InitializeDatabaseAsync();

                // Start the host
                await _host.StartAsync();

                // Create and show login window
                var loginWindow = CreateLoginWindow();
                loginWindow.Show();

                // Don't call base.OnStartup as we're handling startup manually
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup failed: {ex.Message}", "Startup Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_host != null)
                {
                    await _host.StopAsync();
                    _host.Dispose();
                }

                SingleInstanceService.ReleaseMutex();
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex.Message}");
            }
            finally
            {
                base.OnExit(e);
            }
        }

        private IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private IHostBuilder CreateHostBuilder(IConfiguration configuration) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add configuration
                    services.AddSingleton(configuration);

                    // Add Entity Framework
                    services.AddDbContext<OgralabDbContext>(options =>
                        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

                    // Add services
                    services.AddScoped<IAuthenticationService, AuthenticationService>();

                    // Add ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>();

                    // Add logging
                    services.AddLogging(configure =>
                    {
                        configure.AddConsole();
                        configure.AddDebug();
                    });
                });

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                using var scope = _serviceProvider!.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<OgralabDbContext>();

                // Ensure Data directory exists
                var connectionString = context.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var dataSource = GetDataSourceFromConnectionString(connectionString);
                    var directory = Path.GetDirectoryName(dataSource);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                // Ensure database is created and migrations are applied
                await context.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database initialization failed: {ex.Message}", ex);
            }
        }

        private string GetDataSourceFromConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2 && keyValue[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                {
                    return keyValue[1].Trim();
                }
            }
            return "Data/ogralab.db"; // Default fallback
        }

        private LoginWindow CreateLoginWindow()
        {
            using var scope = _serviceProvider!.CreateScope();
            var loginViewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();
            return new LoginWindow(loginViewModel);
        }

        public static T GetService<T>() where T : class
        {
            var app = Current as App;
            using var scope = app?._serviceProvider?.CreateScope();
            return scope?.ServiceProvider.GetRequiredService<T>() 
                ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
        }
    }
}
