using AnalyticsTelemetry.Views;

namespace AnalyticsTelemetry;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("telemetry", typeof(TelemetryPage));
        Routing.RegisterRoute("analytics", typeof(AnalyticsPage));
    }
}
