using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace AnalyticsTelemetry;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}