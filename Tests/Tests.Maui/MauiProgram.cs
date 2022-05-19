using Realms.Tests;
using NUnitLite;

namespace Tests.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            var args = new[] { "--labels=All" };
            var autorun = new AutoRun(typeof(TestHelpers).Assembly);
            var arguments = Realms.Tests.Sync.SyncTestHelpers.ExtractBaasSettings(args);

            using var reader = new System.IO.StringReader(string.Empty);
            using var writer = new NUnit.Common.ColorConsoleWriter(colorEnabled: false);
            autorun.Execute(arguments, writer, reader);

            var resultPath = args.FirstOrDefault(a => a.StartsWith("--result="))?.Replace("--result=", string.Empty);
            if (!string.IsNullOrEmpty(resultPath))
            {
                TestHelpers.TransformTestResults(resultPath);
            }

            System.Environment.Exit(0);
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
