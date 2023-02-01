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
using System.Collections.Concurrent;

namespace Realms.Tests.XamarinTVOS
{
    public partial class ViewController : UIViewController, IUITableViewDataSource
    {
        private readonly ConcurrentQueue<(string Message, ColorStyle Style, bool NewLine)> _logsQueue = new();
        private readonly List<NSAttributedString> _logs = new();

        private Task _testsTask;
        private Task _streamLogsTask;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            LogsTableView.DataSource = this;
            LogsTableView.PanGestureRecognizer.AllowedTouchTypes = new[] { new NSNumber((long)UITouchType.Indirect) };

            if (_streamLogsTask == null)
            {
                _streamLogsTask = StreamLogs();
            }

            if (TestHelpers.IsHeadlessRun(Application.Args))
            {
                RunTests();
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
            if (_testsTask?.Status == TaskStatus.Running)
            {
                return;
            }

            ActivityIndicator.Hidden = false;
            ActivityIndicator.StartAnimating();

            _logs.Clear();

            _testsTask = Task.Run(() =>
            {
                using var reader = new StringReader(string.Empty);
                using var writer = new DebugWriter((msg, style, newLine) => _logsQueue.Enqueue((msg, style, newLine)));
           
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

        private async Task StreamLogs()
        {
            var indexPaths = new List<NSIndexPath>();
            var currentMsg = new NSMutableAttributedString();

            while (true)
            {
                var counter = 0;
                while (_logsQueue.TryDequeue(out var item) && counter++ < 50)
                {
                    currentMsg.Append(GetString(item.Message, item.Style));

                    if (item.NewLine)
                    {
                        indexPaths.Add(NSIndexPath.FromRowSection(_logs.Count, 0));
                        _logs.Add(currentMsg);
                        currentMsg = new NSMutableAttributedString();
                    }
                }

                if (counter > 0)
                {
                    LogsTableView.ReloadData();
                    //LogsTableView.InsertRows(indexPaths.ToArray(), UITableViewRowAnimation.None);
                    LogsTableView.ScrollToRow(NSIndexPath.FromRowSection(_logs.Count - 1, 0), UITableViewScrollPosition.Bottom, false);

                    indexPaths.Clear();
                }

                await Task.Delay(100);
            }
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
                    underlineStyle = NSUnderlineStyle.Thick;
                    textColor = UIColor.Orange;
                    break;
            }

            var font = UIFont.GetMonospacedSystemFont(fontSize, fontWeight);
            return new NSAttributedString(message, font: font, foregroundColor: textColor, underlineStyle:underlineStyle);
        }

        public nint RowsInSection(UITableView tableView, nint section) => _logs.Count;

        public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("LogCell", indexPath) as LogCell;
            var text = _logs[indexPath.Row];
            cell.SetText(text);
            return cell;
        }

        private class DebugWriter : ExtendedTextWriter
        {
            // NUnit will hijack Console.Out so we need to capture it before we run the tests
            private readonly TextWriter _oldConsole;
            private readonly Action<string, ColorStyle, bool> _logAction;

            public DebugWriter(Action<string, ColorStyle, bool> logAction)
            {
                _logAction = logAction;
                _oldConsole = Console.Out;
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
                        _oldConsole.WriteLine(value);
                    }
                    else
                    {
                        _oldConsole.Write(value);
                    }
                }

                _logAction(value, style, newLine);
            }
        }
    }
}
