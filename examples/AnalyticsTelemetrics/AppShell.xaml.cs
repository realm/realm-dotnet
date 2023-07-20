using AnalyticsTelemetrics.Views;

namespace AnalyticsTelemetrics;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("telemetry", typeof(TelemetryPage));
        Routing.RegisterRoute("analytics", typeof(AnalyticsPage));
    }
}
