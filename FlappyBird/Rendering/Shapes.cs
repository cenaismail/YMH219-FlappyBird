using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlappyBird.Rendering
{
    /// <summary>Birden çok sınıfın kullandığı çizim yardımcıları.</summary>
    public static class Shapes
    {
        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Bir yüzeyin üstüne oturan, aşağı doğru yuvarlak damlaları olan kar örtüsü.
        /// Parçalar Region ile birleştirilir; böylece çakışan yerlerde boşluk kalmaz.
        /// </summary>
        public static Region SnowCap(float x, float y, float width, float thickness, int seed)
        {
            var rnd = new Random(seed);
            Region region;
            using (var top = RoundedRect(new RectangleF(x, y - thickness * 0.55f, width, thickness), thickness / 2))
                region = new Region(top);

            int drips = Math.Max(2, (int)(width / 22));
            for (int i = 0; i < drips; i++)
            {
                float cx = x + width * (i + 0.5f) / drips + (float)(rnd.NextDouble() - 0.5) * 6;
                float r = thickness * (0.35f + (float)rnd.NextDouble() * 0.35f);
                using (var drip = new GraphicsPath())
                {
                    drip.AddEllipse(cx - r, y + thickness * 0.2f - r * 0.4f, r * 2, r * 2);
                    region.Union(drip);
                }
            }
            return region;
        }

        public static void FillSnowCap(Graphics g, float x, float y, float width, float thickness, int seed)
        {
            using (var shade = SnowCap(x, y + 2, width, thickness, seed))
            using (var cap = SnowCap(x, y, width, thickness, seed))
            using (var shadeBrush = new SolidBrush(Palette.SnowShade))
            using (var snowBrush = new SolidBrush(Palette.Snow))
            {
                g.FillRegion(shadeBrush, shade);
                g.FillRegion(snowBrush, cap);
            }
        }

        public static GraphicsPath Star(PointF center, float outer, float inner, int points = 5)
        {
            var pts = new PointF[points * 2];
            for (int i = 0; i < pts.Length; i++)
            {
                double a = -Math.PI / 2 + i * Math.PI / points;
                float r = i % 2 == 0 ? outer : inner;
                pts[i] = new PointF(center.X + (float)Math.Cos(a) * r, center.Y + (float)Math.Sin(a) * r);
            }
            var path = new GraphicsPath();
            path.AddPolygon(pts);
            return path;
        }

        /// <summary>Altı kollu kar tanesi işareti.</summary>
        public static void DrawSnowflakeGlyph(Graphics g, PointF c, float r, Color color)
        {
            using (var pen = new Pen(color, Math.Max(1f, r / 4)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                for (int i = 0; i < 3; i++)
                {
                    double a = i * Math.PI / 3;
                    float dx = (float)Math.Cos(a) * r, dy = (float)Math.Sin(a) * r;
                    g.DrawLine(pen, c.X - dx, c.Y - dy, c.X + dx, c.Y + dy);
                }
            }
        }

        /// <summary>Etrafına yumuşak ışık saçan daire (ampul, pencere, ay).</summary>
        public static void FillGlow(Graphics g, PointF c, float radius, Color color, int alpha)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(c.X - radius, c.Y - radius, radius * 2, radius * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(alpha, color);
                    brush.SurroundColors = new[] { Color.FromArgb(0, color) };
                    g.FillPath(brush, path);
                }
            }
        }

        public static void FillEllipse(Graphics g, Color color, float cx, float cy, float rx, float ry)
        {
            using (var b = new SolidBrush(color))
                g.FillEllipse(b, cx - rx, cy - ry, rx * 2, ry * 2);
        }
    }
}
