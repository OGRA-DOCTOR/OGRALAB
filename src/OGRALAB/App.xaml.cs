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
        // حذف _serviceProvider لأنه يمكن الوصول إليه عبر _host.Services

        protected override async void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                if (ex.ExceptionObject is Exception exc)
                    ErrorLogger.Log(exc, "AppDomain.CurrentDomain.UnhandledException");
            };
            DispatcherUnhandledException += (s, ex) =>
            {
                ErrorLogger.Log(ex.Exception, "Application.DispatcherUnhandledException");
                ex.Handled = true;
            };
            TaskScheduler.UnobservedTaskException += (s, ex) =>
            {
                ErrorLogger.Log(ex.Exception, "TaskScheduler.UnobservedTaskException");
                ex.SetObserved();
            };

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

                // Start the host - هذا سيبني ServiceProvider أيضاً
                await _host.StartAsync();

                // Initialize database (يجب أن يتم هذا ضمن نطاق أيضاً)
                await InitializeDatabaseAsync(_host.Services);

                // Create and show login window
                // LoginWindow سيتم إنشاؤها وحقن الـ ViewModel بها من خلال ServiceProvider الخاص بالـ Host
                // لضمان أن الـ DbContext والخدمات الأخرى تعيش طالما الـ Host حي.
                // بدلاً من إنشاء scope مؤقت هنا، سنجعل LoginViewModel يعتمد على scope يتم إنشاؤه عند الحاجة أو
                // يتم تسجيل الخدمات بشكل يسمح بذلك.
                // الطريقة الأبسط هي أن يقوم LoginWindow بطلب الـ ViewModel من الـ ServiceProvider.

                var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();

                // Don't call base.OnStartup as we're handling startup manually
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "App.OnStartup");
                MessageBox.Show($"Application startup failed: {ex.Message}\n\n{ex.StackTrace}", "Startup Error", // أضفت StackTrace للمساعدة في التشخيص
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

                    // تسجيل DbContext كـ Scoped هو السلوك الافتراضي وهو جيد إذا تم إدارة النطاقات بشكل صحيح.
                    // في تطبيق WPF، كل نافذة "رئيسية" أو عملية يمكن أن تعتبر نطاقاً.
                    // عند استخدام AddDbContext، EF Core يعتني بإدارة دورة حياة Context.
                    services.AddDbContext<OgralabDbContext>(options =>
                        options.UseSqlite(configuration.GetConnectionString("DefaultConnection"))
                        // يمكنك إضافة .EnableSensitiveDataLogging() هنا أثناء التطوير لرؤية قيم المتغيرات في استعلامات EF Core
                        // .EnableSensitiveDataLogging() 
                        );

                    services.AddScoped<IAuthenticationService, AuthenticationService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>(); // افترض أنك ستحتاج MainViewModel لاحقاً

                    // تسجيل النوافذ كـ Transient حتى يتم إنشاء نسخة جديدة في كل مرة يتم طلبها
                    // ويتم حقن الـ ViewModels الخاصة بها.
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>(); // افترض أنك ستحتاج MainWindow لاحقاً

                    services.AddLogging(configure =>
                    {
                        configure.AddConsole();
                        configure.AddDebug();
                    });
                });

        // تم تعديل الدالة لتأخذ IServiceProvider كوسيط
        private async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            try
            {
                // إنشاء نطاق جديد لعمليات قاعدة البيانات هنا لضمان التخلص الصحيح من context
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<OgralabDbContext>();

                var connectionString = context.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var dataSource = GetDataSourceFromConnectionString(connectionString);
                    if (dataSource != null) // تحقق من أن dataSource ليس null
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
                throw new InvalidOperationException($"Database initialization failed: {ex.Message}", ex);
            }
        }

        // تم تعديل الدالة لتأخذ IServiceProvider كوسيط
        private string? GetDataSourceFromConnectionString(string connectionString) // تم تغيير نوع الإرجاع إلى string?
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
            // إرجاع null إذا لم يتم العثور عليه بدلاً من قيمة افتراضية قد تكون خاطئة
            return null;
        }

        // تم حذف دالة CreateLoginWindow لأننا سنحصل عليها من ServiceProvider مباشرة في OnStartup
        // تم حذف دالة GetService<T> لأنها تنشئ نطاقاً مؤقتاً قد يسبب نفس المشكلة،
        // والوصول للخدمات يجب أن يتم من خلال _host.Services أو IServiceProvider المحقون.
    }
}
