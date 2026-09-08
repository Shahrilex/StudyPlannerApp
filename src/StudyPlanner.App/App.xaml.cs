using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyPlanner.Core.Interfaces;
using StudyPlanner.Data;
using StudyPlanner.Data.Repositories;
using StudyPlanner.Services;
using StudyPlanner.App.ViewModels;

namespace StudyPlanner.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // مهم: قبلاً اگر بعد از لاگین خطایی رخ می‌داد، برنامه بی‌صدا بسته می‌شد.
        // این هندلر تضمین می‌کند از این به بعد متن دقیق خطا را در یک پیام ببینیم.
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(
                $"خطای پیش‌بینی‌نشده:\n\n{args.Exception}",
                "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        try
        {
            // در شروع برنامه، Migration های در انتظار اعمال می‌شوند (دیگر EnsureCreated نیست -
            // اگر migration اولیه هنوز ساخته نشده، این خط خطا می‌دهد؛ به README مراجعه کن).
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StudyPlannerDbContext>();
                db.Database.Migrate();
            }

            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا هنگام راه‌اندازی برنامه:\n\n{ex}",
                "خطا در راه‌اندازی", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        AppPaths.EnsureAppDataFolderExists();

        services.AddDbContext<StudyPlannerDbContext>(options =>
            options.UseSqlite($"Data Source={AppPaths.DbPath}"));

        services.AddScoped<IStudyDayRepository, StudyDayRepository>();
        services.AddSingleton<IPersianDateService, PersianDateService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();

        services.AddScoped<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
