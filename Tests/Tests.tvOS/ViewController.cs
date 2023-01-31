////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

using System;
using Foundation;
using UIKit;
using NUnitLite;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using NUnit.Common;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace Realms.Tests.XamarinTVOS
{
    public partial class ViewController : UIViewController
    {
        private Task testsTask;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.TestLogsView.Selectable = true;
            this.TestLogsView.PanGestureRecognizer.AllowedTouchTypes = new[] { new NSNumber((long)UITouchType.Indirect) };


            if (TestHelpers.IsHeadlessRun(Application.Args))
            {
                RunTests();
                Environment.Exit(0);
            }
        }

        public override void PressesEnded(NSSet<UIPress> presses, UIPressesEvent evt)
        {
            if ((presses as IEnumerable<UIPress>).Any(p => p.Type == UIPressType.PlayPause))
            {
                RunTests();
            }
            else
            {
                base.PressesEnded(presses, evt);
            }
        }

        private void RunTests()
        {
            if (this.testsTask?.Status == TaskStatus.Running)
            {
                return;
            }

            this.ActivityIndicator.Hidden = false;
            this.ActivityIndicator.StartAnimating();
            this.TestLogsView.AttributedText = new NSAttributedString();

            this.testsTask = Task.Run(() =>
            {
                using var reader = new StringReader(string.Empty);
                using var writer = new DebugWriter((msg, style, newLine) =>
                {
                    this.InvokeOnMainThread(() =>
                    {
                        var newText = this.TestLogsView.AttributedText.MutableCopy() as NSMutableAttributedString;
                        newText.Append(GetString(msg, style));
                        if (newLine)
                        {
                            newText.Append(new NSAttributedString("\n"));
                        }
                        this.TestLogsView.AttributedText = newText;

                        var bottomOffset = new CoreGraphics.CGPoint(0, this.TestLogsView.ContentSize.Height - this.TestLogsView.Bounds.Size.Height + this.TestLogsView.ContentInset.Bottom);
                        this.TestLogsView.SetContentOffset(bottomOffset, animated: false);
                    });
                });

                var autorun = new AutoRun(typeof(TestHelpers).Assembly);

                autorun.Execute(Application.Args.Where(a => a != "--headless").ToArray(), writer, reader);

                this.InvokeOnMainThread(() =>
                {
                    this.ActivityIndicator.StopAnimating();

                    if (TestHelpers.IsHeadlessRun(Application.Args))
                    {
                        var resultPath = TestHelpers.GetResultsPath(Application.Args);
                        TestHelpers.TransformTestResults(resultPath);

                        UIApplication.SharedApplication.PerformSelector(new ObjCRuntime.Selector("terminateWithSuccess"));
                    }
                });
            });
        }

        private static NSAttributedString GetString(string message, ColorStyle style)
        {
            nfloat fontSize = 16;
            var fontWeight = UIFontWeight.Regular;
            var textColor = UIColor.Label;
            var underlineStyle = NSUnderlineStyle.None;

            switch (style)
            {
                case ColorStyle.Header:
                    fontSize = 22;
                    break;
                case ColorStyle.SubHeader:
                    fontSize = 20;
                    break;
                case ColorStyle.SectionHeader:
                    fontSize = 18;
                    break;
                case ColorStyle.Output:
                    fontWeight = UIFontWeight.Bold;
                    textColor = UIColor.DarkGray;
                    break;
                case ColorStyle.Default:
                case ColorStyle.Value:
                    break;
                case ColorStyle.Label:
                    underlineStyle = NSUnderlineStyle.Single;
                    break;
                case ColorStyle.Help:
                    textColor = UIColor.LightGray;
                    break;
                case ColorStyle.Pass:
                    fontWeight = UIFontWeight.Bold;
                    textColor = UIColor.Green;
                    break;
                case ColorStyle.Failure:
                case ColorStyle.Error:
                    fontWeight = UIFontWeight.Bold;
                    textColor = UIColor.Red;
                    break;
                case ColorStyle.Warning:
                    underlineStyle = NSUnderlineStyle.Thick; ;
                    textColor = UIColor.Orange;
                    break;
            }

            var font = UIFont.GetMonospacedSystemFont(fontSize, fontWeight);
            return new NSAttributedString(message, font: font, foregroundColor: textColor, underlineStyle:underlineStyle);
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
}
