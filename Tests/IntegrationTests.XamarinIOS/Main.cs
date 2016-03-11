/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Linq;
using System.Collections.Generic;

using Foundation;
using UIKit;

namespace IntegrationTests.XamarinIOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main (string[] args)
        {
			// run unit tests in a headless mode when we're in Jenkins CI
			var ci = Environment.GetEnvironmentVariable("WORKSPACE");
			if (!string.IsNullOrEmpty (ci)) {
				using (var output = System.IO.File.OpenWrite (System.IO.Path.Combine (ci, "TestResults.iOS.xml"))) 
				{
					IntegrationTests.Shared.TestRunner.Run ("Xamarin.iOS", output);
				}

				return;
			}

            // if you want to use a different Application Delegate class from "UnitTestAppDelegate"
            // you can specify it here.
            UIApplication.Main (args, null, "UnitTestAppDelegate");
        }
    }
}
