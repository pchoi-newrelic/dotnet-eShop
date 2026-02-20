using eShop.HybridApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using NewRelic.MAUI.Plugin;

namespace eShop.HybridApp;

public static class MauiProgram
{
    // NOTE: Must have a trailing slash on base URLs to ensure the full BaseAddress URL is used to resolve relative URLs
    internal static string MobileBffHost = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:11632/" : "http://localhost:11632/";


    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.ConfigureLifecycleEvents(AppLifecycle =>
        {
#if ANDROID
            AppLifecycle.AddAndroid(android => android
                .OnCreate((activity, savedInstanceState) => StartNewRelic()));
#endif

#if IOS
            AppLifecycle.AddiOS(iOS => iOS.WillFinishLaunching((_, __) =>
            {
                StartNewRelic();
                return false;
            }));
#endif
        });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif


        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new(MobileBffHost))
            .ConfigurePrimaryHttpMessageHandler(() => CrossNewRelic.Current.GetHttpMessageHandler());
        builder.Services.AddSingleton<WebAppComponents.Services.IProductImageUrlProvider, ProductImageUrlProvider>();

        return builder.Build();
    }

    private static void StartNewRelic()
    {
        CrossNewRelic.Current.HandleUncaughtException();

        AgentStartConfiguration agentConfig = new AgentStartConfiguration(crashReportingEnabled:true,
            loggingEnabled:true,
            logLevel: NewRelic.MAUI.Plugin.LogLevel.VERBOSE,
            collectorAddress: "staging-mobile-collector.newrelic.com",
            crashCollectorAddress: "staging-mobile-crash.newrelic.com");

        if (DeviceInfo.Current.Platform == DevicePlatform.Android)
        {
            CrossNewRelic.Current.Start("AA6111255e2a7c80dbf06f012d1b522fe22fd59fcc-NRMA", agentConfig);
        }
        else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            CrossNewRelic.Current.Start("AA117845c30a5074b9719fb94493411087e2ab77e6-NRMA", agentConfig);
        }
    }
}