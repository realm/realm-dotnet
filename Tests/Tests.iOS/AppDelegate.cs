// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using Foundation;
using UIKit;
using NUnit.Runner.Services;

namespace Tests.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            var nunit = new NUnit.Runner.App();

            // If you want to add tests in another assembly
            //nunit.AddTestAssembly(typeof(MyTests).Assembly);

            // nunit.AutoRun = false;
            nunit.Options = new TestOptions
            {
                AutoRun = false,
                // If True, the application will terminate automatically after running the tests.
                //TerminateAfterExecution = true,

                // Information about the tcp listener host and port.
                // For now, send result as XML to the listening server.
                // TcpWriterParameters = new TcpWriterInfo("192.168.0.108", 13000),
                CreateXmlResultFile = true
            };
            LoadApplication(nunit);

            return base.FinishedLaunching(app, options);
        }
    }
}
