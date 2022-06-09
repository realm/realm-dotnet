using Realms.Tests;
using NUnitLite;
using NUnit.Common;
using System.Text;
using System.Diagnostics;

namespace Tests.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            var args = new[] { "--labels=After" };
            var autorun = new AutoRun(typeof(TestHelpers).Assembly);
            var arguments = Realms.Tests.Sync.SyncTestHelpers.ExtractBaasSettings(args);

            using var reader = new StringReader(string.Empty);
            using ExtendedTextWriter writer = Debugger.IsAttached ? new DebugWriter() : new ColorConsoleWriter(colorEnabled: false);
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

    private class DebugWriter : ExtendedTextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(ColorStyle style, string value) => Debug.Write(value);

        public override void WriteLabel(string label, object option) => Debug.Write(label);

        public override void WriteLabel(string label, object option, ColorStyle valueStyle) => Debug.Write(label);

        public override void WriteLabelLine(string label, object option) => Debug.WriteLine(label);

        public override void WriteLabelLine(string label, object option, ColorStyle valueStyle) => Debug.WriteLine(label);

        public override void WriteLine(ColorStyle style, string value) => Debug.WriteLine(value);
    }
}
