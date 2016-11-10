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

using Android.App;
using Android.Widget;
using Android.OS;
using SkiaSharp.Views;
using DrawXShared;

namespace DrawX.Droid
{
    [Activity(Label = "DrawX", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        RealmDraw _drawer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _drawer = new RealmDraw();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            SKCanvasView canvas = FindViewById<SKCanvasView>(Resource.Id.canvas);
            canvas.PaintSurface += OnPaintSample;

        }

        private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer.DrawBackground(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }
    }
}

