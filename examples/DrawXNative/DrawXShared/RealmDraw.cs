using System;
using SkiaSharp;
namespace DrawXShared
{
    public class RealmDraw
    {

        public RealmDraw()
        {
        }

        public void DrawBackground(SKCanvas canvas, int width, int height)
        {
            canvas.Clear(SKColors.White);

            using (SKPaint paint = new SKPaint())
            using (SKPath path = new SKPath())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 10;
                paint.IsAntialias = true;
                paint.StrokeCap = SKStrokeCap.Round;
                paint.StrokeJoin = SKStrokeJoin.Round;
                paint.Color = RealmDrawerMedia.Colors.Dove;//RealmDrawerMedia.Colors.XamarinGreen;

                path.MoveTo(20, 20);
                path.LineTo(400, 50);
                path.LineTo(80, 100);
                path.LineTo(300, 150);

                canvas.DrawPath(path, paint);
            }
        }
    }
}
