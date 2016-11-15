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

using UIKit;
using Foundation;
using System;
using System.Diagnostics;
using SkiaSharp.Views.iOS;
using DrawXShared;

namespace DrawX.iOS
{
    public class ViewControllerShared : UIViewController
    {

        RealmDraw _drawer;

        public ViewControllerShared(IntPtr handle) : base(handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Debug.WriteLine($"Opened view with bounds {View.Bounds.Size}");
            // scale bounds to match the pixel dimensions of the SkiaSurface
            _drawer = new RealmDraw( 
                2.0f * (float)View.Bounds.Width, 
                2.0f * (float)View.Bounds.Height,
                (sender, args) => {
                    View?.SetNeedsDisplay();  // just refresh on notification
            });
            // relies on override to point its canvas at our OnPaintSample
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            // this is the earliest we can show the modal login
            bool canLogin = DrawXSettingsManager.HasCredentials();
            if (!canLogin)
            {
                EditCredentials();
                canLogin = DrawXSettingsManager.HasCredentials();
            }
            if (canLogin)
            {
                _drawer.LoginToServerAsync();
            }
        }


        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.        
        }


        protected void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer.DrawTouches(e.Surface.Canvas);
        }


        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer.StartDrawing((float)point.X * 2.0f, (float)point.Y * 2.0f);
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
                _drawer.AddPoint((float)point.X * 2.0f, (float)point.Y * 2.0f);
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
                _drawer.StopDrawing((float)point.X * 2.0f, (float)point.Y * 2.0f);
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

        private void EditCredentials()
        {
            // TODO generalise this to work in either this or DrawX.iOS project
            var sb = UIStoryboard.FromName("LoginScreen", null);
            var loginVC = sb.InstantiateViewController("Login") as LoginViewController;
            loginVC.Invoker = this;
            PresentViewController(loginVC, false, null);
        }
    }
}
