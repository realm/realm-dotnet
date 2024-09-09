////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Text;
using NUnit.Common;
using NUnitLite;
using Realms.Tests;

namespace Tests.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        _ = RunHeadless();
    }

    private void OnRunTestsClicked(object sender, EventArgs e)
    {
        _ = RunTests();
    }

    private async Task RunHeadless()
    {
        if (!TestHelpers.IsHeadlessRun(MauiProgram.Args))
        {
            return;
        }

        await RunTests();

        Environment.Exit(0);
    }

    private async Task RunTests()
    {
        if (!RunTestsButton.IsEnabled)
        {
            return;
        }

        try
        {
            RunTestsButton.IsEnabled = false;
            BusyIndicator.IsVisible = true;
            ResultsLabel.Text = "Running tests...";
            LogsStack.Children.Clear();

            var result = await Task.Run(() =>
            {
                var autorun = new AutoRun(typeof(TestHelpers).Assembly);
                var arguments = MauiProgram.Args;

                using var reader = new StringReader(string.Empty);
                using var writer = new DebugWriter((msg, style, newLine) =>
                {
                    Dispatcher.Dispatch(() =>
                    {
                        var span = GetSpan(msg, style);
                        var label = LogsStack.Children.LastOrDefault() as Label;
                        if (label == null)
                        {
                            label = new Label { FormattedText = new FormattedString() };
                            LogsStack.Children.Add(label);
                        }
                        label.FormattedText.Spans.Add(span);
                        if (newLine)
                        {
                            LogsStack.Children.Add(new Label { FormattedText = new FormattedString() });
                        }

                        if (ScrollLogsToggle.IsToggled)
                        {
                            _ = LogsScrollView.ScrollToAsync(0, 999999, false);
                        }
                    });
                });
                return autorun.Execute(arguments.Where(a => a != "--headless").ToArray(), writer, reader);
            });

            ResultsLabel.Text = $"Test run complete. Failed: {result}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            RunTestsButton.IsEnabled = true;
            BusyIndicator.IsVisible = false;
        }
    }

    private static Span GetSpan(string message, ColorStyle style)
    {
        var span = new Span { Text = message, FontSize = 12 };
        switch (style)
        {
            case ColorStyle.Header:
                span.FontSize = 18;
                break;
            case ColorStyle.SubHeader:
                span.FontSize = 16;
                break;
            case ColorStyle.SectionHeader:
                span.FontSize = 14;
                break;
            case ColorStyle.Output:
                span.FontAttributes = FontAttributes.Bold;
                span.TextColor = Colors.DarkGray;
                break;
            case ColorStyle.Default:
            case ColorStyle.Value:
                break;
            case ColorStyle.Label:
                span.FontAttributes = FontAttributes.Italic;
                break;
            case ColorStyle.Help:
                span.TextColor = Colors.LightGray;
                break;
            case ColorStyle.Pass:
                span.FontAttributes = FontAttributes.Bold;
                span.TextColor = Colors.DarkGreen;
                break;
            case ColorStyle.Failure:
            case ColorStyle.Error:
                span.FontAttributes = FontAttributes.Bold;
                span.TextColor = Colors.DarkRed;
                break;
            case ColorStyle.Warning:
                span.FontAttributes = FontAttributes.Italic;
                span.TextColor = Colors.DarkOrange;
                break;
        }

        return span;
    }

    private class DebugWriter : ExtendedTextWriter
    {
        private readonly Action<string, ColorStyle, bool> _logAction;

        public DebugWriter(Action<string, ColorStyle, bool> logAction)
        {
            _logAction = logAction;
        }

        public override Encoding Encoding => Encoding.Unicode;

        public override void Write(ColorStyle style, string value) => WriteImpl(value, style);

        public override void WriteLabel(string label, object option) => WriteImpl($"{label}{option}", ColorStyle.Label);

        public override void WriteLabel(string label, object option, ColorStyle valueStyle) => WriteImpl($"{label}{option}", ColorStyle.Default);

        public override void WriteLabelLine(string label, object option) => WriteImpl($"{label}{option}", ColorStyle.Label, newLine: true);

        public override void WriteLabelLine(string label, object option, ColorStyle valueStyle) => WriteImpl($"{label}{option}", valueStyle, newLine: true);

        public override void WriteLine(ColorStyle style, string value) => WriteImpl(value, style, newLine: true);

        private void WriteImpl(string value, ColorStyle style, bool newLine = false)
        {
            if (Debugger.IsAttached)
            {
                if (newLine)
                {
                    Debug.WriteLine(value);
                }
                else
                {
                    Debug.Write(value);
                }
            }
            else
            {
                if (newLine)
                {
                    Console.WriteLine(value);
                }
                else
                {
                    Console.Write(value);
                }
            }

            _logAction(value, style, newLine);
        }
    }
}

