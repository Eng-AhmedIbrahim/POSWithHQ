using System.IO;
using System.Net.Http;
using System.Windows;
using HQ.Desktop.Auth;
using HQ.Desktop.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace HQ.Desktop;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
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
