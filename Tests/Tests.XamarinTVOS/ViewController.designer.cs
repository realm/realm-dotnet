// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Realms.Tests.XamarinTVOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView ActivityIndicator { get; set; }

		[Outlet]
		UIKit.UIButton RunTestsButton { get; set; }

		[Outlet]
		UIKit.UITextView TestLogsView { get; set; }

		[Action ("RunTests")]
		partial void RunTests ();
		
		void ReleaseDesignerOutlets ()
		{
			if (ActivityIndicator != null) {
				ActivityIndicator.Dispose ();
				ActivityIndicator = null;
			}

			if (TestLogsView != null) {
				TestLogsView.Dispose ();
				TestLogsView = null;
			}

			if (RunTestsButton != null) {
				RunTestsButton.Dispose ();
				RunTestsButton = null;
			}
		}
	}
}
