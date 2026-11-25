using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using FlappyBird.Rendering;

namespace FlappyBird.UI
{
    /// <summary>Menü ekranındaki başlık yazısı ve "Tap to Play" butonu.</summary>
    public class StartScreen
    {
        public const string ButtonText = "Tap to Play";

        /// <summary>Butonun altında gösterilen kontrol ipucu.</summary>
        public const string HintText = "Space / Tıkla : Zıpla";

        private readonly float _centerX;

        /// <summary>Butonun sanal ekrandaki alanı (tıklama kontrolü için).</summary>
        public RectangleF ButtonBounds { get; private set; }

        /// <summary>Fare butonun üzerindeyse true.</summary>
        public bool IsButtonHovered { get; set; }

        public StartScreen(float screenWidth)
        {
            _centerX = screenWidth / 2;
            ButtonBounds = new RectangleF(_centerX - 118, 522, 236, 62);
        }

        public bool HitTest(PointF virtualPoint)
        {
            return ButtonBounds.Contains(virtualPoint);
        }

        public void Draw(Graphics g, float time)
        {
            RectangleF flappy = DrawWord(g, "Flappy", 82, 344, Palette.Cream, Palette.CreamShade, 1);
            RectangleF bird = DrawWord(g, "Bird", 86, 428, Palette.Red, Palette.RedDark, 2);

            DrawSparkles(g, flappy, time);
            DrawSantaHat(g, flappy.Left + 10, flappy.Top - 6);
            DrawHolly(g, new PointF(bird.Left - 38, bird.Top + 40), false);
            DrawHolly(g, new PointF(bird.Right + 40, bird.Top + 6), true);

            DrawButton(g, time);
            DrawHint(g, time);
        }

        /// <summary>Butonun altında hafifçe yanıp sönen kontrol ipucu.</summary>
        private void DrawHint(Graphics g, float time)
        {
            int alpha = (int)(200 + 55 * Math.Sin(time * 2.5));
            var pill = new RectangleF(_centerX - 100, ButtonBounds.Bottom + 22, 200, 32);

            using (var path = Shapes.RoundedRect(pill, 16))
            using (var bg = new SolidBrush(Color.FromArgb(150, 10, 16, 50)))
            using (var border = new Pen(Color.FromArgb(120, Palette.Snow), 1.5f))
            {
                g.FillPath(bg, path);
                g.DrawPath(border, path);
            }

            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var text = new GraphicsPath())
            {
                text.AddString(HintText, FontManager.Ui, (int)FontStyle.Bold, 16, pill, format);
                using (var b = new SolidBrush(Color.FromArgb(alpha, Palette.Cream)))
                    g.FillPath(b, text);
            }
        }

        // ------------------------------------------------------------- Başlık

        /// <summary>
        /// Kelimeyi koyu kontur + gradyan dolgu + üstte kar ile çizer.
        /// Kar, harf silüetinin kendisinden aşağı kaydırılmış halinin çıkarılmasıyla elde edilir;
        /// böylece her harfin yalnızca üst kenarları beyaz olur.
        /// </summary>
        private RectangleF DrawWord(Graphics g, string text, float size, float centerY, Color top, Color bottom, int seed)
        {
            using (var path = new GraphicsPath())
            {
                path.AddString(text, FontManager.Title, (int)FontStyle.Regular, size, PointF.Empty, StringFormat.GenericTypographic);
                RectangleF b = path.GetBounds();
                using (var m = new Matrix())
                {
                    m.Translate(_centerX - b.Width / 2 - b.X, centerY - b.Height / 2 - b.Y);
                    path.Transform(m);
                }
                b = path.GetBounds();

                // Gölge
                using (var shadow = (GraphicsPath)path.Clone())
                using (var m = new Matrix())
                using (var shadowPen = new Pen(Color.FromArgb(120, 5, 8, 30), 14) { LineJoin = LineJoin.Round })
                {
                    m.Translate(0, 6);
                    shadow.Transform(m);
                    g.DrawPath(shadowPen, shadow);
                }

                // Kalın koyu kontur
                using (var outline = new Pen(Palette.Outline, 12) { LineJoin = LineJoin.Round })
                    g.DrawPath(outline, path);

                // Dolgu
                using (var fill = new LinearGradientBrush(new PointF(0, b.Top), new PointF(0, b.Bottom + 1), top, bottom))
                    g.FillPath(fill, path);

                // İç parlama çizgisi
                using (var shine = new Pen(Color.FromArgb(60, Color.White), 2))
                    g.DrawPath(shine, path);

                DrawSnowOnLetters(g, path);
                return b;
            }
        }

        private static void DrawSnowOnLetters(Graphics g, GraphicsPath letters)
        {
            using (var snow = new Region(letters))
            using (var shade = new Region(letters))
            using (var down6 = (GraphicsPath)letters.Clone())
            using (var down9 = (GraphicsPath)letters.Clone())
            using (var m6 = new Matrix())
            using (var m9 = new Matrix())
            {
                m6.Translate(0, 6);
                m9.Translate(0, 9);
                down6.Transform(m6);
                down9.Transform(m9);
                snow.Exclude(down6);
                shade.Exclude(down9);

                using (var shadeBrush = new SolidBrush(Palette.SnowShade))
                using (var snowBrush = new SolidBrush(Palette.Snow))
                {
                    g.FillRegion(shadeBrush, shade);
                    g.FillRegion(snowBrush, snow);
                }
            }
        }

        private static void DrawSparkles(Graphics g, RectangleF title, float time)
        {
            int a = (int)(190 + 60 * Math.Sin(time * 4));
            using (var pen = new Pen(Color.FromArgb(a, Palette.Gold), 5) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                // Sağ üst
                g.DrawLine(pen, title.Right + 22, title.Top - 2, title.Right + 14, title.Top + 14);
                g.DrawLine(pen, title.Right + 26, title.Top + 30, title.Right + 42, title.Top + 26);
                // Sol
                g.DrawLine(pen, title.Left - 34, title.Top + 44, title.Left - 16, title.Top + 48);
                g.DrawLine(pen, title.Left - 28, title.Top + 70, title.Left - 14, title.Top + 62);
            }
        }

        private static void DrawSantaHat(Graphics g, float x, float y)
        {
            GraphicsState s = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(-18);
            g.ScaleTransform(0.72f, 0.72f);
            using (var outline = new Pen(Palette.Outline, 3) { LineJoin = LineJoin.Round })
            {
                using (var cone = new GraphicsPath())
                {
                    cone.AddBezier(-30, 4, -26, -36, 30, -48, 48, -6);
                    cone.AddLine(48, -6, 34, 6);
                    cone.CloseFigure();
                    using (var b = new LinearGradientBrush(new PointF(0, -40), new PointF(0, 6), Palette.Red, Palette.RedDark))
                        g.FillPath(b, cone);
                    g.DrawPath(outline, cone);
                }
                using (var trim = Shapes.RoundedRect(new RectangleF(-36, -2, 80, 18), 9))
                using (var white = new SolidBrush(Palette.Snow))
                {
                    g.FillPath(white, trim);
                    g.DrawPath(outline, trim);
                }
                using (var white = new SolidBrush(Palette.Snow))
                {
                    g.FillEllipse(white, -52, -10, 22, 22);
                    g.DrawEllipse(outline, -52, -10, 22, 22);
                }
            }
            g.Restore(s);
        }

        private static void DrawHolly(Graphics g, PointF c, bool mirror)
        {
            GraphicsState s = g.Save();
            g.TranslateTransform(c.X, c.Y);
            if (mirror) g.ScaleTransform(-1, 1);

            DrawLeaf(g, -14, 0, -150);
            DrawLeaf(g, 14, 4, -30);
            DrawLeaf(g, 0, 16, 80);

            using (var outline = new Pen(Color.FromArgb(90, 10, 10), 1.4f))
            {
                PointF[] berries = { new PointF(-4, -2), new PointF(5, -4), new PointF(1, 6) };
                foreach (PointF p in berries)
                {
                    Shapes.FillEllipse(g, Color.FromArgb(214, 28, 36), p.X, p.Y, 6, 6);
                    g.DrawEllipse(outline, p.X - 6, p.Y - 6, 12, 12);
                    Shapes.FillEllipse(g, Color.FromArgb(200, Color.White), p.X - 2, p.Y - 2.5f, 1.6f, 1.6f);
                }
            }
            g.Restore(s);
        }

        /// <summary>Tırtıklı kenarlı çoban püskülü yaprağı.</summary>
        private static void DrawLeaf(Graphics g, float x, float y, float angle)
        {
            GraphicsState s = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(angle);

            var pts = new PointF[]
            {
                new PointF(0, 0), new PointF(8, -8), new PointF(12, -5), new PointF(18, -11), new PointF(23, -6),
                new PointF(30, -10), new PointF(34, 0),
                new PointF(30, 10), new PointF(23, 6), new PointF(18, 11), new PointF(12, 5), new PointF(8, 8)
            };
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(pts);
                using (var b = new LinearGradientBrush(new PointF(0, -10), new PointF(0, 10),
                           Color.FromArgb(70, 160, 70), Color.FromArgb(22, 92, 40)))
                    g.FillPath(b, path);
                using (var pen = new Pen(Color.FromArgb(14, 50, 24), 1.6f) { LineJoin = LineJoin.Round })
                    g.DrawPath(pen, path);
                using (var vein = new Pen(Color.FromArgb(120, 200, 240, 160), 1.2f))
                    g.DrawLine(vein, 3, 0, 30, 0);
            }
            g.Restore(s);
        }

        // ------------------------------------------------------------- Buton

        private void DrawButton(Graphics g, float time)
        {
            RectangleF r = ButtonBounds;
            float pulse = IsButtonHovered ? 1.06f : 1f + 0.015f * (float)Math.Sin(time * 3);

            GraphicsState s = g.Save();
            float cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
            g.TranslateTransform(cx, cy);
            g.ScaleTransform(pulse, pulse);
            g.TranslateTransform(-cx, -cy);

            Color top = IsButtonHovered ? Color.FromArgb(255, 92, 80) : Palette.Red;
            Color bottom = IsButtonHovered ? Color.FromArgb(206, 36, 36) : Palette.RedDark;

            using (var shadow = Shapes.RoundedRect(new RectangleF(r.X, r.Y + 6, r.Width, r.Height), 20))
            using (var sb = new SolidBrush(Color.FromArgb(130, 8, 6, 30)))
                g.FillPath(sb, shadow);

            using (var body = Shapes.RoundedRect(r, 20))
            {
                using (var b = new LinearGradientBrush(new PointF(0, r.Top), new PointF(0, r.Bottom), top, bottom))
                    g.FillPath(b, body);
                using (var inner = Shapes.RoundedRect(new RectangleF(r.X + 8, r.Y + 10, r.Width - 16, r.Height * 0.38f), 10))
                using (var hb = new SolidBrush(Color.FromArgb(55, Color.White)))
                    g.FillPath(hb, inner);
                using (var pen = new Pen(Palette.Outline, 4))
                    g.DrawPath(pen, body);
            }

            Shapes.FillSnowCap(g, r.X + 6, r.Y + 1, r.Width - 12, 12, 8);

            // Yazı
            using (var path = new GraphicsPath())
            {
                path.AddString(ButtonText, FontManager.Title, (int)FontStyle.Regular, 30, PointF.Empty, StringFormat.GenericTypographic);
                RectangleF tb = path.GetBounds();
                using (var m = new Matrix())
                {
                    m.Translate(cx - tb.Width / 2 - tb.X, cy + 4 - tb.Height / 2 - tb.Y);
                    path.Transform(m);
                }
                using (var pen = new Pen(Color.FromArgb(120, 60, 10, 10), 5) { LineJoin = LineJoin.Round })
                    g.DrawPath(pen, path);
                using (var b = new SolidBrush(Palette.Cream))
                    g.FillPath(b, path);
            }

            // Yan yıldızlar
            foreach (float sx in new[] { r.Left + 26, r.Right - 26 })
            {
                using (var star = Shapes.Star(new PointF(sx, cy + 3), 10, 4.5f))
                using (var b = new SolidBrush(Palette.Gold))
                using (var pen = new Pen(Color.FromArgb(150, 90, 30), 1.4f))
                {
                    g.FillPath(b, star);
                    g.DrawPath(pen, star);
                }
            }

            g.Restore(s);
        }
    }
}
