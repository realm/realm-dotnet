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

