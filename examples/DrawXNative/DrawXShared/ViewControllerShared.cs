////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.Diagnostics;
using DrawXShared;
using Foundation;
using SkiaSharp.Views.iOS;
using UIKit;

namespace DrawX.IOS
{
    public class ViewControllerShared : UIViewController
    {
        private RealmDraw _drawer;
        private bool _hasShownCredentials;  // flag to show on initial layout only

        public ViewControllerShared(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Debug.WriteLine($"Opened view with bounds {View.Bounds.Size}");

            // relies on override to point its canvas at our OnPaintSample
            // see ViewDidLayoutSubviews for triggering EditCredentials
            DrawXSettingsManager.InitLocalSettings();
            if (DrawXSettingsManager.HasCredentials())
            {
                // assume we can login and be able to draw
                // TODO handle initial failure to login despite saved credentials
                SetupDrawer();
            }
        }

        private void SetupDrawer()
        {
            // scale bounds to match the pixel dimensions of the SkiaSurface
            _drawer = new RealmDraw(
                                    2.0f * (float)View.Bounds.Width,
                                    2.0f * (float)View.Bounds.Height);
            _drawer.CredentialsEditor = () =>
            {
                InvokeOnMainThread(EditCredentials);
            };
            _drawer.RefreshOnRealmUpdate = () =>
            {
                Debug.WriteLine("Refresh callback triggered by Realm");
                View?.SetNeedsDisplay();  // just refresh on notification
            };
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // this is the earliest we can show the modal login
            // show unconditionally on launch
            if (!_hasShownCredentials)
            {
                EditCredentials();
                _hasShownCredentials = true;
            }
        }

        protected void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer?.DrawTouches(e.Surface.Canvas);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer?.StartDrawing((float)point.X * 2.0f, (float)point.Y * 2.0f);
                Debug.WriteLine("TouchesBegan before SetNeedsDisplay");
                View.SetNeedsDisplay();  // probably after touching Pencils
                Debug.WriteLine("TouchesBegan afer SetNeedsDisplay");
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer?.AddPoint((float)point.X * 2.0f, (float)point.Y * 2.0f);
                Debug.WriteLine("TouchesMoved returned from AddPoint, about to SetNeedsDisplay.");
                View.SetNeedsDisplay();
                Debug.WriteLine("TouchesMoved after SetNeedsDisplay.");
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                _drawer?.CancelDrawing();
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var point = touch.LocationInView(View);
                _drawer?.StopDrawing((float)point.X * 2.0f, (float)point.Y * 2.0f);
            }

            View.SetNeedsDisplay();
        }

        public override void MotionBegan(UIEventSubtype eType, UIEvent evt)
        {
            if (eType == UIEventSubtype.MotionShake)
            {
                _drawer.ErasePaths();
                //// unlike other gesture actions, don't call View.SetNeedsDisplay but let major Realm change prompt redisplay
            }
        }

        private void EditCredentials()
        {
            // TODO generalise this to work in either this or DrawX.iOS project
            var sb = UIStoryboard.FromName("LoginScreen", null);
            var loginVC = sb.InstantiateViewController("Login") as LoginViewController;
            loginVC.OnCloseLogin = (bool changedServer) =>
            {
                DismissModalViewController(false);
                if (changedServer || _drawer == null)
                {
                    if (DrawXSettingsManager.HasCredentials())
                    {
                        SetupDrawer();  // pointless unless contact server
                        _drawer.LoginToServerAsync();
                    }
                    //// TODO allow user to launch locally if server not available
                }

                View.SetNeedsDisplay();
            };
            PresentViewController(loginVC, false, null);
        }
    }
}
