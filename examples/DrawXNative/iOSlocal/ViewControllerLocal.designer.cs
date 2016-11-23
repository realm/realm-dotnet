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

namespace DrawX.IOS
{
    [Register ("ViewControllerLocal")]
    partial class ViewControllerLocal
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        SkiaSharp.Views.iOS.SKCanvasView canvas { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (canvas != null) {
                canvas.Dispose ();
                canvas = null;
            }
        }
    }
}