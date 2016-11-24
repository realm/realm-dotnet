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

using Android.App;
using Android.Views;
using Android.OS;
using DrawXShared;
using SkiaSharp.Views.Android;

namespace DrawX.Droid
{
    [Activity(Label = "DrawX", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        RealmDraw _drawer;
        SKCanvasView _canvas;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _canvas = FindViewById<SKCanvasView>(Resource.Id.canvas);
            _canvas.PaintSurface += OnPaintSample;
            _canvas.Touch += OnTouch;

            DrawXSettingsManager.InitLocalSettings();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (_drawer == null)
            {
                // deferred update until can get view bounds
                _drawer = new RealmDraw(_canvas.CanvasSize.Width, _canvas.CanvasSize.Height);
                _drawer.CredentialsEditor = () =>
                {
                    ////TODO Android eqivalent InvokeOnMainThread(() => EditCredentials());
                };
                _drawer.RefreshOnRealmUpdate = () =>
                {
                    Debug.WriteLine("Refresh callback triggered by Realm");
                    _canvas.Invalidate();
                };
            }
        }

        private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer?.DrawTouches(e.Surface.Canvas);
        }

        private void OnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            if (_drawer == null)
                return;  // in case managed to trigger before focus event finished setup
            
            float fx = touchEventArgs.Event.GetX();
            float fy = touchEventArgs.Event.GetY();
            bool needsRefresh = false;
            switch (touchEventArgs.Event.Action & MotionEventActions.Mask)
            {
                case MotionEventActions.Down:
                    _drawer.StartDrawing(fx, fy);
                    needsRefresh = true;
                    break;

                case MotionEventActions.Move:
                    _drawer.AddPoint(fx, fy);
                    needsRefresh = true;
                    break;

                case MotionEventActions.Up:
                    _drawer.StopDrawing(fx, fy);
                    needsRefresh = true;
                    break;
            }
            if (needsRefresh)
            {
                _canvas.Invalidate();
            }
        }

        public override void OnBackPressed()
        {
            _drawer.ErasePaths();
            _canvas.Invalidate();
        }
    }
}

