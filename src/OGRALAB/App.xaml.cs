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
        public static IHost? AppHost { get; private set; }

        public App()
        {
            // InitializeComponent();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!SingleInstanceService.IsFirstInstance())
            {
                MessageBox.Show("OGRALAB is already running.", "Application Already Running",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                SingleInstanceService.BringExistingInstanceToFront();
                Current.Shutdown();
                return;
            }

            try
            {
                var configuration = BuildConfiguration();
                AppHost = CreateHostBuilder(configuration).Build();
                await AppHost.StartAsync();

                Current.Resources["ServiceProvider"] = AppHost.Services;

                await InitializeDatabaseAsync(AppHost.Services);

                var loginWindow = AppHost.Services.GetRequiredService<LoginWindow>();
                Current.MainWindow = loginWindow;
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                // لاحقًا، سنستخدم مسجل الأخطاء هنا بدلاً من MessageBox
                MessageBox.Show($"Application startup failed: {ex.Message}\n\n{ex.StackTrace}", "Startup Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                if (AppHost != null)
                {
                    await AppHost.StopAsync();
                    AppHost.Dispose();
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

                    // تسجيل قاعدة البيانات
                    services.AddDbContext<OgralabDbContext>(options =>
                        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

                    // تسجيل الخدمات (Services)
                    services.AddScoped<IAuthenticationService, AuthenticationService>();
                    services.AddScoped<INavigationService, NavigationService>();
                    services.AddScoped<PatientService>();
                    services.AddScoped<TestService>();
                    services.AddScoped<ResultService>();
                    // *** إضافة خدمات المرحلة الرابعة ***
                    services.AddScoped<IReportService, ReportService>();
                    services.AddScoped<IPrintService, PrintService>();


                    // تسجيل ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<AddPatientViewModel>();
                    services.AddTransient<EnterResultsViewModel>();
                    // *** إضافة ViewModels المرحلة الرابعة ***
                    services.AddTransient<SelectReportViewModel>();
                    services.AddTransient<ReportPreviewViewModel>();


                    // تسجيل النوافذ (Windows) و (UserControls)
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<DashboardUserControl>();
                    services.AddTransient<AddPatientUserControl>();
                    services.AddTransient<EnterResultsUserControl>();
                    // *** إضافة Views المرحلة الرابعة ***
                    services.AddTransient<SelectReportUserControl>();
                    services.AddTransient<ReportPreviewWindow>();


                    // تسجيل اللوجر (Logger)
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