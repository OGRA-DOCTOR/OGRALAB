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

        protected override async void OnStartup(StartupEventArgs e)
        {
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
                var configuration = BuildConfiguration();
                _host = CreateHostBuilder(configuration).Build();
                await _host.StartAsync();

                // Initialize database using the host's service provider
                await InitializeDatabaseAsync(_host.Services);

                // Resolve and show LoginWindow from the host's services
                var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup failed: {ex.Message}\n\n{ex.StackTrace}", "Startup Error",
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
                    services.AddSingleton(configuration);

                    services.AddDbContext<OgralabDbContext>(options =>
                        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

                    services.AddScoped<IAuthenticationService, AuthenticationService>();
                    services.AddScoped<INavigationService, NavigationService>();
                    
                    // New Phase 3 Services
                    services.AddScoped<PatientService>();
                    services.AddScoped<TestService>();
                    services.AddScoped<ResultService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    
                    // New Phase 3 ViewModels
                    services.AddTransient<AddPatientViewModel>();
                    services.AddTransient<EnterResultsViewModel>();

                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<DashboardUserControl>();
                    
                    // New Phase 3 Windows
                    services.AddTransient<AddPatientWindow>();
                    services.AddTransient<EnterResultsWindow>();

                    services.AddLogging(configure =>
                    {
                        configure.AddConsole();
                        configure.AddDebug();
                    });
                });

        private async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<OgralabDbContext>();

                var connectionString = context.Database.GetConnectionString();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}", "DB Init Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private string? GetDataSourceFromConnectionString(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return null;

            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2 && keyValue[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                {
                    return keyValue[1].Trim();
                }
            }
            return null;
        }
    }
}