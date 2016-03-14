/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace IntegrationTests.XamarinAndroid
{
	[Instrumentation(Name = "io.realm.xamarintests.TestRunner")]
	public class TestRunnerInstrumentation : Instrumentation
	{
		public TestRunnerInstrumentation (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate (Bundle arguments)
		{
			base.OnCreate (arguments);

			this.Start ();
		}

		public override void OnStart ()
		{
			using (var output = Context.OpenFileOutput ("TestResults.Android.xml", FileCreationMode.WorldReadable)) 
			{
				IntegrationTests.Shared.TestRunner.Run ("Xamarin.Android", output);
			}

			this.Finish (Result.Ok, null);
		}
	}
}

