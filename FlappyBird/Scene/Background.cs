using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using FlappyBird.Rendering;

namespace FlappyBird.Scene
{
    /// <summary>
    /// Gece gökyüzü, ay, bulutlar, ev, ağaçlar, çit ve karlı zemin.
    /// Hem menüde hem oyun sırasında aynı arka plan kullanılır.
    /// </summary>
    public class Background
    {
        private struct Star
        {
            public PointF Position;
            public float Size;
            public float Phase;
        }

        private readonly float _width;
        private readonly float _height;
        private readonly List<Star> _stars = new List<Star>();

        private static readonly PointF[] SnowflakeGlyphs =
        {
            new PointF(227, 16), new PointF(181, 118), new PointF(217, 90), new PointF(58, 203),
            new PointF(356, 219), new PointF(428, 272), new PointF(28, 350), new PointF(48, 507),
            new PointF(370, 470), new PointF(90, 596), new PointF(224, 642), new PointF(140, 40)
        };

        public Background(float width, float height)
        {
            _width = width;
            _height = height;

            var rnd = new Random(42);
            for (int i = 0; i < 80; i++)
            {
                _stars.Add(new Star
                {
                    Position = new PointF((float)rnd.NextDouble() * width, (float)rnd.NextDouble() * height * 0.8f),
                    Size = 0.6f + (float)rnd.NextDouble() * 1.4f,
                    Phase = (float)rnd.NextDouble() * 6.28f
                });
            }
        }

        public void DrawSky(Graphics g, float time)
        {
            var rect = new RectangleF(0, 0, _width, _height);
            using (var brush = new LinearGradientBrush(rect, Palette.SkyTop, Palette.SkyBottom, LinearGradientMode.Vertical))
            {
                brush.InterpolationColors = new ColorBlend
                {
                    Colors = new[] { Palette.SkyTop, Palette.SkyMid, Palette.SkyBottom },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                g.FillRectangle(brush, rect);
            }

            foreach (Star s in _stars)
            {
                int a = (int)(140 + 110 * Math.Sin(time * 2 + s.Phase));
                Shapes.FillEllipse(g, Color.FromArgb(Math.Max(30, a), Color.White), s.Position.X, s.Position.Y, s.Size, s.Size);
            }
            foreach (PointF p in SnowflakeGlyphs)
                Shapes.DrawSnowflakeGlyph(g, p, 5, Color.FromArgb(210, 230, 240, 255));

            DrawMoon(g, new PointF(322, 108), 56);

            DrawCloud(g, -10, 128, 1.0f);
            DrawCloud(g, -30, 252, 1.1f);
            DrawCloud(g, 318, 148, 0.9f);
            DrawCloud(g, -30, 470, 1.0f);
            DrawCloud(g, 360, 500, 1.15f);
        }

        public void DrawLandscape(Graphics g, float time)
        {
            DrawFarTrees(g);
            DrawPine(g, 175, 668, 50, 82);
            DrawPine(g, 212, 690, 40, 62);
            DrawHouse(g, time);
            DrawPine(g, 30, 530, 112, 180);
            DrawChristmasTree(g, 252, 660, 74, 94, time);
            DrawGround(g);
            DrawFence(g);
        }

        // ---------------------------------------------------------------- Gökyüzü

        private static void DrawMoon(Graphics g, PointF c, float r)
        {
            Shapes.FillGlow(g, c, r * 2.0f, Palette.MoonLight, 70);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(c.X - r, c.Y - r, r * 2, r * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterPoint = new PointF(c.X - r * 0.25f, c.Y - r * 0.25f);
                    brush.CenterColor = Palette.MoonLight;
                    brush.SurroundColors = new[] { Palette.MoonDark };
                    g.FillPath(brush, path);
                }
            }

            Color crater = Color.FromArgb(70, 200, 170, 100);
            Shapes.FillEllipse(g, crater, c.X - 22, c.Y - 18, 10, 8);
            Shapes.FillEllipse(g, crater, c.X + 18, c.Y - 26, 7, 5);
            Shapes.FillEllipse(g, crater, c.X + 6, c.Y + 4, 12, 9);
            Shapes.FillEllipse(g, crater, c.X - 30, c.Y + 18, 6, 5);

            DrawSleigh(g, c.X - 30, c.Y + 2);
        }

        /// <summary>Ayın önünden geçen kızak ve geyik silüetleri.</summary>
        private static void DrawSleigh(Graphics g, float x, float y)
        {
            using (var brush = new SolidBrush(Palette.Silhouette))
            using (var pen = new Pen(Palette.Silhouette, 1.4f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                // Kızak
                using (var sleigh = new GraphicsPath())
                {
                    sleigh.AddLine(x - 14, y - 2, x + 4, y - 2);
                    sleigh.AddLine(x + 4, y - 2, x + 2, y + 6);
                    sleigh.AddLine(x + 2, y + 6, x - 12, y + 6);
                    sleigh.CloseFigure();
                    g.FillPath(brush, sleigh);
                }
                g.DrawLine(pen, x - 16, y + 9, x + 6, y + 9);
                g.DrawArc(pen, x + 2, y + 4, 6, 5, 270, 180);
                Shapes.FillEllipse(g, Palette.Silhouette, x - 6, y - 6, 4, 5); // Noel Baba
                Shapes.FillEllipse(g, Palette.Silhouette, x - 11, y - 4, 4, 4); // hediye çuvalı

                // Üç geyik, kızaktan sağa ve yukarı doğru
                for (int i = 0; i < 3; i++)
                {
                    float rx = x + 18 + i * 15;
                    float ry = y - 6 - i * 5;
                    g.DrawLine(pen, x + 4, y + 1, rx - 4, ry + 1);
                    DrawReindeer(g, brush, pen, rx, ry);
                }
            }
        }

        private static void DrawReindeer(Graphics g, Brush brush, Pen pen, float x, float y)
        {
            g.FillEllipse(brush, x - 5, y - 2.5f, 10, 5);         // gövde
            g.DrawLine(pen, x + 4, y - 1, x + 6, y - 5);          // boyun
            g.FillEllipse(brush, x + 5, y - 7, 4, 3);             // kafa
            g.DrawLine(pen, x + 6, y - 7, x + 5, y - 10);         // boynuz
            g.DrawLine(pen, x + 7, y - 7, x + 9, y - 10);
            g.DrawLine(pen, x - 4, y + 1, x - 7, y + 5);          // bacaklar (koşar halde)
            g.DrawLine(pen, x - 2, y + 2, x - 1, y + 6);
            g.DrawLine(pen, x + 3, y + 1, x + 6, y + 4);
            g.DrawLine(pen, x + 2, y + 2, x + 1, y + 6);
        }

        private static void DrawCloud(Graphics g, float x, float y, float scale)
        {
            GraphicsState s = g.Save();
            g.TranslateTransform(x, y);
            g.ScaleTransform(scale, scale);
            using (var path = new GraphicsPath())
            {
                path.FillMode = FillMode.Winding;
                path.AddEllipse(0, 14, 50, 30);
                path.AddEllipse(26, 0, 46, 40);
                path.AddEllipse(58, 12, 40, 30);
                path.AddEllipse(80, 22, 34, 22);
                path.AddRectangle(new RectangleF(16, 28, 90, 16));
                using (var b = new LinearGradientBrush(new PointF(0, 0), new PointF(0, 44), Palette.CloudLight, Palette.Cloud))
                    g.FillPath(b, path);
            }
            g.Restore(s);
        }

        // ---------------------------------------------------------------- Zemin

        private void DrawFarTrees(Graphics g)
        {
            using (var b = new SolidBrush(Palette.FarTrees))
            {
                var rnd = new Random(5);
                for (float x = -10; x < _width; x += 22)
                {
                    float h = 40 + (float)rnd.NextDouble() * 30;
                    g.FillPolygon(b, new[]
                    {
                        new PointF(x, 742), new PointF(x + 13, 742 - h), new PointF(x + 26, 742)
                    });
                }
            }
        }

        private static void DrawPine(Graphics g, float cx, float top, float width, float height)
        {
            int tiers = 4;
            for (int i = 0; i < tiers; i++)
            {
                float t0 = top + height * i / (tiers + 0.6f);
                float tierH = height / (tiers - 0.6f);
                float w = width * (0.45f + 0.55f * (i + 1) / tiers);
                var pts = new[]
                {
                    new PointF(cx, t0),
                    new PointF(cx + w / 2, t0 + tierH),
                    new PointF(cx - w / 2, t0 + tierH)
                };
                using (var b = new LinearGradientBrush(new PointF(cx - w / 2, 0), new PointF(cx + w / 2, 0), Palette.Pine, Palette.PineDark))
                    g.FillPolygon(b, pts);

                // Dalların üstündeki kar
                using (var snow = new SolidBrush(Palette.Snow))
                using (var path = new GraphicsPath())
                {
                    path.AddLine(cx, t0, cx + w * 0.3f, t0 + tierH * 0.6f);
                    path.AddBezier(cx + w * 0.3f, t0 + tierH * 0.6f, cx + w * 0.1f, t0 + tierH * 0.5f,
                        cx - w * 0.1f, t0 + tierH * 0.75f, cx - w * 0.36f, t0 + tierH * 0.72f);
                    path.CloseFigure();
                    g.FillPath(snow, path);
                }
            }
            using (var trunk = new SolidBrush(Palette.Wood))
                g.FillRectangle(trunk, cx - width * 0.06f, top + height - 4, width * 0.12f, 14);
        }

        private static void DrawChristmasTree(Graphics g, float cx, float top, float width, float height, float time)
        {
            DrawPine(g, cx, top + 6, width, height);

            // Işıklar
            var rnd = new Random(3);
            for (int i = 0; i < 22; i++)
            {
                float t = (float)rnd.NextDouble();
                float y = top + 18 + t * (height - 22);
                float half = width * 0.45f * (y - top) / height;
                float x = cx + ((float)rnd.NextDouble() * 2 - 1) * half;
                Color c = Palette.Bulbs[i % Palette.Bulbs.Length];
                float tw = 0.55f + 0.45f * (float)Math.Sin(time * 4 + i);
                Shapes.FillGlow(g, new PointF(x, y), 6, c, (int)(170 * tw));
                Shapes.FillEllipse(g, c, x, y, 1.6f, 1.6f);
            }

            // Tepe yıldızı
            var starC = new PointF(cx, top + 2);
            Shapes.FillGlow(g, starC, 20, Palette.Gold, 140);
            using (var star = Shapes.Star(starC, 9, 4))
            using (var b = new SolidBrush(Palette.Gold))
                g.FillPath(b, star);
        }

        private void DrawHouse(Graphics g, float time)
        {
            // Baca dumanı
            for (int i = 0; i < 4; i++)
            {
                float k = (time * 0.35f + i * 0.25f) % 1f;
                float sx = 404 + (float)Math.Sin(k * 5) * 5 - k * 6;
                float sy = 620 - k * 60;
                Shapes.FillEllipse(g, Color.FromArgb((int)(90 * (1 - k)), 210, 215, 235), sx, sy, 6 + k * 10, 5 + k * 8);
            }

            using (var wall = new SolidBrush(Palette.Wall))
            using (var wallDark = new SolidBrush(Palette.WallDark))
            using (var snow = new SolidBrush(Palette.Snow))
            using (var shade = new SolidBrush(Palette.SnowShade))
            {
                // Baca
                g.FillRectangle(wallDark, 396, 622, 15, 34);
                g.FillRectangle(snow, 393, 617, 21, 7);

                // Sol kanat
                g.FillRectangle(wallDark, 286, 700, 46, 60);
                g.FillPolygon(shade, new[] { new PointF(278, 708), new PointF(334, 680), new PointF(338, 690), new PointF(284, 716) });
                g.FillPolygon(snow, new[] { new PointF(276, 704), new PointF(334, 676), new PointF(338, 686), new PointF(282, 712) });

                // Ana bina
                g.FillRectangle(wall, 320, 668, 112, 94);
                g.FillPolygon(wall, new[] { new PointF(320, 670), new PointF(376, 636), new PointF(432, 670) });

                // Karlı çatı
                g.FillPolygon(shade, new[]
                {
                    new PointF(304, 678), new PointF(376, 634), new PointF(450, 678),
                    new PointF(442, 684), new PointF(376, 644), new PointF(312, 684)
                });
                g.FillPolygon(snow, new[]
                {
                    new PointF(302, 674), new PointF(376, 628), new PointF(452, 674),
                    new PointF(444, 680), new PointF(376, 638), new PointF(310, 680)
                });
            }

            DrawWindow(g, 338, 684, 22, 26);
            DrawWindow(g, 392, 684, 22, 26);
            DrawWindow(g, 338, 724, 22, 26);
            DrawWindow(g, 392, 724, 22, 26);
            DrawWindow(g, 299, 722, 18, 22);
        }

        private static void DrawWindow(Graphics g, float x, float y, float w, float h)
        {
            Shapes.FillGlow(g, new PointF(x + w / 2, y + h / 2), w * 1.2f, Palette.Window, 90);
            using (var b = new SolidBrush(Palette.Window))
            using (var frame = new Pen(Palette.WallDark, 2))
            {
                g.FillRectangle(b, x, y, w, h);
                g.DrawRectangle(frame, x, y, w, h);
                g.DrawLine(frame, x + w / 2, y, x + w / 2, y + h);
                g.DrawLine(frame, x, y + h / 2, x + w, y + h / 2);
            }
        }

        private void DrawGround(Graphics g)
        {
            using (var path = new GraphicsPath())
            {
                path.AddLine(0, _height, 0, 752);
                path.AddBezier(0, 752, 110, 738, 220, 762, 330, 748);
                path.AddBezier(330, 748, 380, 742, 420, 748, _width, 746);
                path.AddLine(_width, 746, _width, _height);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(new PointF(0, 740), new PointF(0, _height), Palette.Snow, Palette.SnowShade))
                    g.FillPath(b, path);
            }
        }

        private void DrawFence(Graphics g)
        {
            using (var wood = new SolidBrush(Palette.Wood))
            using (var dark = new Pen(Color.FromArgb(70, 44, 34), 1.2f))
            {
                g.FillRectangle(wood, 0, 758, _width, 5);
                g.FillRectangle(wood, 0, 772, _width, 5);
                for (float x = 6; x < _width; x += 26)
                {
                    using (var post = Shapes.RoundedRect(new RectangleF(x, 750, 9, 40), 3))
                    {
                        g.FillPath(wood, post);
                        g.DrawPath(dark, post);
                    }
                    Shapes.FillEllipse(g, Palette.Snow, x + 4.5f, 750, 6, 3.2f);
                }
            }
            Shapes.FillSnowCap(g, -4, 758, _width + 8, 5, 21);
            using (var b = new SolidBrush(Palette.Snow))
                g.FillRectangle(b, 0, 788, _width, _height - 788);
        }
    }
}
