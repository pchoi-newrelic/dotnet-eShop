using NewRelic.MAUI.Plugin;
using eShop.HybridApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

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

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif


        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new(MobileBffHost))
            .ConfigurePrimaryHttpMessageHandler(() => CrossNewRelic.Current.GetHttpMessageHandler());
        builder.Services.AddSingleton<WebAppComponents.Services.IProductImageUrlProvider, ProductImageUrlProvider>();


        // NEW INITIALIZATION PATTERN
        builder.ConfigureLifecycleEvents(AppLifecycle => {
        #if ANDROID
            AppLifecycle.AddAndroid(android => android
              .OnCreate((activity, savedInstanceState) => StartNewRelic()));
        #endif
        #if IOS
            AppLifecycle.AddiOS(iOS => iOS.WillFinishLaunching((app, options) => {
                StartNewRelic();
                return false;
            }));
        #endif
        });
        return builder.Build();
    }

    private static void StartNewRelic()
    {
        // 1. Initialize Exception Handler FIRST
        CrossNewRelic.Current.HandleUncaughtException();
      // Set optional agent configuration
      // Options are: crashReportingEnabled, loggingEnabled, logLevel, collectorAddress, crashCollectorAddress,analyticsEventEnabled, networkErrorRequestEnabled, networkRequestEnabled, interactionTracingEnabled,webViewInstrumentation, fedRampEnabled,offlineStorageEnabled,newEventSystemEnabled,backgroundReportingEnabled
      // AgentStartConfiguration agentConfig = new AgentStartConfiguration(crashReportingEnabled:false);
        AgentStartConfiguration agentConfig = new AgentStartConfiguration(crashReportingEnabled:true, collectorAddress: "staging-mobile-collector.newrelic.com",
    crashCollectorAddress: "staging-mobile-crash.newrelic.com",analyticsEventEnabled: true, networkErrorRequestEnabled: true, networkRequestEnabled: true, interactionTracingEnabled: true, webViewInstrumentation: true, fedRampEnabled: false, offlineStorageEnabled: true, newEventSystemEnabled: true, backgroundReportingEnabled: true);
        // 2. Start Agent based on Platform
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
