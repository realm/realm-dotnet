// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Playground.XamarinIOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextView DebugText { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (DebugText != null) {
				DebugText.Dispose ();
				DebugText = null;
			}
		}
	}
}
