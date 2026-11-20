using System.IO;
using System.Net.Http;
using System.Windows;
using HQ.Desktop.Auth;
using HQ.Desktop.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Pos.Repository.Data;

namespace HQ.Desktop;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        this.DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"خطأ في واجهة التطبيق:\n{args.Exception.Message}\n\n{args.Exception.InnerException?.Message}", 
                "خطأ New Tech HQ", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"خطأ غير متوقع:\n{ex.Message}\n\n{ex.InnerException?.Message}", 
                    "خطأ New Tech HQ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<MainWindow>();
        services.AddWpfBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();
        services.AddAuthorizationCore();

        // Database Context Configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=.\\SQLEXPRESS;Database=PointOfSaleDBk;User Id=sa;Password=NewTechn0;TrustServerCertificate=true";
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString), ServiceLifetime.Transient);

        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
            config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
        });

        // HTTP Client Configuration
        var apiName = configuration["ApiSettings:ApiName"] ?? "POSAPI";
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://100.79.100.86:5050";

        services.AddHttpClient(apiName, client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = true,
            AllowAutoRedirect = true
        });

        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient(apiName);
        });

        // Authentication & Business Services
        services.AddSingleton<HqAuthStateProvider>();
        services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<HqAuthStateProvider>());
        services.AddSingleton<HqDataSyncService>();
        services.AddSingleton<HqAnalyticsService>();

        _serviceProvider = services.BuildServiceProvider();
        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
