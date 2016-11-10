using System;
using SkiaSharp.Views;
using UIKit;
using DrawXShared;

namespace DrawX.iOS
{
    public partial class ViewController : UIViewController
    {
        int count = 1;
        RealmDraw _drawer;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            canvas.PaintSurface += OnPaintSample;
            _drawer = new RealmDraw();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.        
        }

        private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
        {
            _drawer.DrawBackground(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }
    }
}
