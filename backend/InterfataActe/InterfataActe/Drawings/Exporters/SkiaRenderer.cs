using System.IO;
using System.Linq;
using InterfataActe.Drawings.Core;
using SkiaSharp;

namespace InterfataActe.Drawings.Exporters
{
    // Shared vector renderer for the PDF and PNG exports.
    public class SkiaRenderer
    {
        private readonly SKTypeface _regular;
        private readonly SKTypeface _bold;
        private readonly string _stampPath;

        public SkiaRenderer(DrawingOptions options)
        {
            var fonts = options.ResolveFontsPath();
            _regular = Load(Path.Combine(fonts, "DejaVuSans.ttf"), SKFontStyle.Normal);
            _bold = Load(Path.Combine(fonts, "DejaVuSans-Bold.ttf"), SKFontStyle.Bold);
            _stampPath = options.StampImagePath;
        }

        private static SKTypeface Load(string path, SKFontStyle style) =>
            File.Exists(path) ? SKTypeface.FromFile(path) : SKTypeface.FromFamilyName("DejaVu Sans", style);

        // scale = device units per millimetre
        public void Render(Drawing d, SKCanvas canvas, float scale)
        {
            canvas.Clear(SKColors.White);
            canvas.Scale(scale);
            foreach (var shape in d.Ordered)
                Draw(canvas, shape, scale);
            DrawStamp(d, canvas);
        }

        private void Draw(SKCanvas canvas, Shape shape, float scale)
        {
            var color = SKColor.Parse(Layers.Get(shape.Layer).Color);
            using var stroke = new SKPaint
            {
                Color = color,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (float)shape.Width,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                PathEffect = shape.Dashed ? SKPathEffect.CreateDash(new[] { 2f, 1.2f }, 0) : null,
            };

            switch (shape)
            {
                case LineShape l:
                    canvas.DrawLine((float)l.X1, (float)l.Y1, (float)l.X2, (float)l.Y2, stroke);
                    break;
                case PolylineShape p when p.Points.Count > 1:
                    using (var path = new SKPath())
                    {
                        path.MoveTo((float)p.Points[0].X, (float)p.Points[0].Y);
                        foreach (var pt in p.Points.Skip(1)) path.LineTo((float)pt.X, (float)pt.Y);
                        if (p.Closed) path.Close();
                        canvas.DrawPath(path, stroke);
                    }
                    break;
                case RectShape r:
                    var rect = SKRect.Create((float)r.X, (float)r.Y, (float)r.W, (float)r.H);
                    if (r.Fill != null)
                    {
                        using var fill = new SKPaint { Color = SKColor.Parse(r.Fill), Style = SKPaintStyle.Fill, IsAntialias = true };
                        canvas.DrawRect(rect, fill);
                    }
                    canvas.DrawRect(rect, stroke);
                    break;
                case CircleShape c:
                    if (c.Filled)
                    {
                        using var fill = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true };
                        canvas.DrawCircle((float)c.Cx, (float)c.Cy, (float)c.R, fill);
                    }
                    canvas.DrawCircle((float)c.Cx, (float)c.Cy, (float)c.R, stroke);
                    break;
                case TextShape t:
                    using (var paint = new SKPaint
                    {
                        Color = color,
                        IsAntialias = true,
                        Typeface = t.Bold ? _bold : _regular,
                        TextSize = (float)t.Size,
                        TextAlign = t.Anchor switch { TextAnchor.Middle => SKTextAlign.Center, TextAnchor.End => SKTextAlign.Right, _ => SKTextAlign.Left },
                        SubpixelText = true,
                    })
                    {
                        canvas.Save();
                        canvas.Translate((float)t.X, (float)t.Y);
                        if (t.Rotation != 0) canvas.RotateDegrees((float)-t.Rotation);
                        canvas.DrawText(t.Value, 0, 0, paint);
                        canvas.Restore();
                    }
                    break;
            }
        }

        private void DrawStamp(Drawing d, SKCanvas canvas)
        {
            if (d.StampArea == null || string.IsNullOrWhiteSpace(_stampPath) || !File.Exists(_stampPath)) return;
            using var image = SKBitmap.Decode(_stampPath);
            if (image == null) return;
            var area = d.StampArea;
            float ratio = System.Math.Min((float)area.W / image.Width, (float)area.H / image.Height);
            float w = image.Width * ratio, h = image.Height * ratio;
            var dest = SKRect.Create((float)(area.X + (area.W - w) / 2), (float)(area.Y + (area.H - h) / 2), w, h);
            using var paint = new SKPaint { FilterQuality = SKFilterQuality.High, IsAntialias = true };
            canvas.DrawBitmap(image, dest, paint);
        }
    }
}
