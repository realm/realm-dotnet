////////////////////////////////////////////////////////////////////////////
//
// Copyright 2014 Realm Inc.
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
using SkiaSharp.Views;
using UIKit;
using Foundation;
using DrawXShared;

namespace DrawX.iOS
{
    public partial class ViewController : UIViewController
    {
        RealmDraw _drawer;

        public ViewController(IntPtr handle) : base(handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            canvas.PaintSurface += OnPaintSample;
            _drawer = new RealmDraw();
            // TODO draw the 
        }


        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.        
        }

        private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer.DrawTouches(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }


        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer.StartDrawing(point.X, point.Y);
            }
            View.SetNeedsDisplay();
        }


        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer.AddPoint(point.X, point.Y);
            }
            View.SetNeedsDisplay();
        }


        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                _drawer.CancelDrawing();
            }
            //TODO             View.SetNeedsDisplay();  ????
        }


        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer.StopDrawing(point.X, point.Y);
            }
            View.SetNeedsDisplay();
        }

        public override void MotionBegan(UIEventSubtype eType, UIEvent evt)
        {
            if (eType == UIEventSubtype.MotionShake)
            {
                _drawer.ErasePaths();
                View.SetNeedsDisplay();
            }
        }
    }
}
