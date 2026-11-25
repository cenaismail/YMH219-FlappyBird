using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using FlappyBird.Rendering;

namespace FlappyBird.Entities
{
    /// <summary>
    /// Noel Baba şapkalı, atkılı kuş. Menüde yerinde süzülür; oyunda yer çekimiyle
    /// düşer ve Jump() çağrıldığında yukarı doğru hız kazanır.
    /// </summary>
    public class Bird
    {
        // Çizimde kullanılan gövde yarıçapları (ölçeklenmeden önce)
        public const float RadiusX = 34f;
        public const float RadiusY = 30f;

        /// <summary>Kuşun çizim ölçeği. 1 = orijinal boyut.</summary>
        public const float Size = 0.68f;

        /// <summary>Ölçek uygulanmış gerçek yarıçaplar (çarpışma kontrolü bunları kullanır).</summary>
        public const float HitRadiusX = RadiusX * Size;
        public const float HitRadiusY = RadiusY * Size;

        private static readonly Color Outline = Color.FromArgb(58, 36, 22);
        private static readonly Color BodyLight = Color.FromArgb(255, 226, 98);
        private static readonly Color BodyDark = Color.FromArgb(242, 160, 26);
        private static readonly Color Beak = Color.FromArgb(255, 132, 34);
        private static readonly Color BeakDark = Color.FromArgb(226, 92, 20);

        /// <summary>Yer çekimi ivmesi (piksel / saniye²).</summary>
        public const float Gravity = 1150f;

        /// <summary>Zıplayınca kuşa verilen dikey hız (eksi = yukarı).</summary>
        public const float JumpVelocity = -400f;

        /// <summary>Düşüş hızının üst sınırı.</summary>
        public const float MaxFallSpeed = 560f;

        /// <summary>Kuşun burnunun en fazla ne kadar yukarı/aşağı dönebileceği (derece).</summary>
        public const float MaxUpAngle = -18f;
        public const float MaxDownAngle = 35f;

        /// <summary>Açının hedefe ne kadar hızlı yaklaşacağı. Büyüdükçe dönüş sertleşir.</summary>
        public const float RotationSmoothing = 8f;

        public float X { get; set; }
        public float Y { get; set; }

        /// <summary>Dikey hız (piksel / saniye). Pozitif değer aşağı doğrudur.</summary>
        public float Velocity { get; private set; }

        /// <summary>Derece cinsinden eğim (+ aşağı bakar).</summary>
        public float Rotation { get; set; }

        /// <summary>Kanat çırpma animasyonunun açısı.</summary>
        public float WingAngle { get; private set; }

        private readonly float _baseX;
        private readonly float _baseY;

        public Bird(float x, float y)
        {
            X = x;
            Y = y;
            _baseX = x;
            _baseY = y;
        }

        /// <summary>Menü ekranındaki yukarı-aşağı süzülme.</summary>
        public void Hover(float time)
        {
            Y = _baseY + (float)Math.Sin(time * 2.2) * 8;
            Rotation = (float)Math.Cos(time * 2.2) * -4;
            WingAngle = (float)Math.Sin(time * 14) * 28;
        }

        /// <summary>Kuşu başlangıç konumuna ve hızsız hale getirir.</summary>
        public void Reset()
        {
            Reset(_baseX, _baseY);
        }

        /// <summary>Kuşu verilen konuma taşır ve hızını sıfırlar.</summary>
        public void Reset(float x, float y)
        {
            X = x;
            Y = y;
            Velocity = 0;
            Rotation = 0;
        }

        /// <summary>Kuşa yukarı doğru ani bir hız verir.</summary>
        public void Jump()
        {
            Velocity = JumpVelocity;
        }

        /// <summary>
        /// Oyun sırasında her karede çağrılır: yer çekimi hızı artırır, hız da konumu değiştirir.
        /// dt: son kareden bu yana geçen süre (saniye).
        /// </summary>
        public void Update(float dt, float time)
        {
            Velocity = Math.Min(Velocity + Gravity * dt, MaxFallSpeed);
            Y += Velocity * dt;

            // Yükselirken burnu hafifçe yukarı, düşerken hafifçe aşağı baksın.
            // Açı hıza göre hesaplanır ama sınırlıdır (-18° ile +35° arası) ve
            // hedef açıya yavaşça yaklaşılır; böylece ani dönüşler olmaz.
            float targetRotation = Math.Max(MaxUpAngle, Math.Min(MaxDownAngle, Velocity * 0.06f));
            Rotation += (targetRotation - Rotation) * Math.Min(1f, dt * RotationSmoothing);

            // Yükselirken hızlı kanat çırpar, düşerken kanat sabit kalır
            WingAngle = Velocity < 150 ? (float)Math.Sin(time * 22) * 30 : -10;
        }

        /// <summary>Kuşu yere oturtur (Game Over sonrası).</summary>
        public void LandOn(float groundY)
        {
            Y = groundY - HitRadiusY;
            Velocity = 0;
            WingAngle = -10;
        }

        public RectangleF Bounds
        {
            get { return new RectangleF(X - HitRadiusX + 3, Y - HitRadiusY + 3, HitRadiusX * 2 - 6, HitRadiusY * 2 - 6); }
        }

        public void Draw(Graphics g)
        {
            GraphicsState saved = g.Save();
            g.TranslateTransform(X, Y);
            g.RotateTransform(Rotation);
            g.ScaleTransform(Size, Size);

            DrawSpeedLines(g);
            DrawScarfTail(g);
            DrawBody(g);
            DrawWing(g);
            DrawEye(g);
            DrawBeak(g);
            DrawScarfBand(g);
            DrawHat(g);

            g.Restore(saved);
        }

        private static void DrawSpeedLines(Graphics g)
        {
            using (var pen = new Pen(Color.FromArgb(170, 220, 230, 255), 2.4f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, -88, -8, -56, -10);
                g.DrawLine(pen, -96, 4, -60, 2);
                g.DrawLine(pen, -84, 16, -58, 14);
            }
        }

        private static void DrawBody(Graphics g)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(-RadiusX, -RadiusY, RadiusX * 2, RadiusY * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterPoint = new PointF(-6, -12);
                    brush.CenterColor = BodyLight;
                    brush.SurroundColors = new[] { BodyDark };
                    g.FillPath(brush, path);
                }
                using (var pen = new Pen(Outline, 2.6f))
                    g.DrawPath(pen, path);
            }
            // Parlama
            Shapes.FillEllipse(g, Color.FromArgb(110, Color.White), -14, -18, 9, 5);
        }

        private void DrawWing(Graphics g)
        {
            GraphicsState s = g.Save();
            g.TranslateTransform(-14, 6);
            g.RotateTransform(WingAngle);
            using (var brush = new SolidBrush(Color.FromArgb(255, 238, 150)))
            using (var pen = new Pen(Outline, 2.2f))
            {
                g.FillEllipse(brush, -15, -8, 26, 15);
                g.DrawEllipse(pen, -15, -8, 26, 15);
            }
            g.Restore(s);
        }

        private static void DrawEye(Graphics g)
        {
            using (var white = new SolidBrush(Color.White))
            using (var pen = new Pen(Outline, 2.4f))
            {
                g.FillEllipse(white, 2, -24, 26, 28);
                g.DrawEllipse(pen, 2, -24, 26, 28);
            }
            Shapes.FillEllipse(g, Color.FromArgb(24, 20, 24), 19, -9, 6, 7.5f);
            Shapes.FillEllipse(g, Color.White, 21, -12, 2, 2.2f);
        }

        private static void DrawBeak(Graphics g)
        {
            using (var pen = new Pen(Outline, 2.2f))
            {
                var lower = new RectangleF(16, 10, 28, 12);
                using (var p = Shapes.RoundedRect(lower, 6))
                using (var b = new SolidBrush(BeakDark))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }
                var upper = new RectangleF(18, 1, 34, 13);
                using (var p = Shapes.RoundedRect(upper, 6.5f))
                using (var b = new SolidBrush(Beak))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }
            }
        }

        private static void FillStriped(Graphics g, GraphicsPath path)
        {
            Region old = g.Clip;
            using (var white = new SolidBrush(Color.White))
                g.FillPath(white, path);
            g.SetClip(path, CombineMode.Intersect);
            using (var red = new Pen(Palette.Red, 4.5f))
            {
                for (int i = -120; i < 80; i += 11)
                    g.DrawLine(red, i, -60, i + 50, 60);
            }
            g.Clip = old;
            using (var pen = new Pen(Outline, 2f))
                g.DrawPath(pen, path);
        }

        private static void DrawScarfBand(Graphics g)
        {
            GraphicsState s = g.Save();
            g.TranslateTransform(-6, 22);
            g.RotateTransform(-14);
            using (var band = Shapes.RoundedRect(new RectangleF(-24, -6, 44, 13), 6.5f))
                FillStriped(g, band);
            g.Restore(s);
        }

        private static void DrawScarfTail(Graphics g)
        {
            using (var tail = new GraphicsPath())
            {
                tail.AddBezier(-24, 14, -38, 6, -48, 26, -66, 16);
                tail.AddLine(-66, 16, -62, 30);
                tail.AddBezier(-62, 30, -46, 38, -36, 22, -20, 28);
                tail.CloseFigure();
                FillStriped(g, tail);
            }
        }

        private static void DrawHat(Graphics g)
        {
            GraphicsState s = g.Save();
            g.TranslateTransform(-4, -26);
            g.RotateTransform(-14);

            using (var cone = new GraphicsPath())
            {
                cone.AddBezier(-26, 2, -24, -24, 8, -34, 22, 0);
                cone.AddLine(22, 0, -26, 2);
                cone.CloseFigure();
                using (var b = new LinearGradientBrush(new PointF(0, -30), new PointF(0, 4), Palette.Red, Palette.RedDark))
                    g.FillPath(b, cone);
                using (var pen = new Pen(Outline, 2.2f))
                    g.DrawPath(pen, cone);
            }

            // Sarkan uç
            using (var tip = new GraphicsPath())
            {
                tip.AddBezier(-14, -18, -30, -26, -42, -14, -46, 2);
                tip.AddLine(-46, 2, -38, 4);
                tip.AddBezier(-38, 4, -34, -10, -24, -14, -10, -8);
                tip.CloseFigure();
                using (var b = new SolidBrush(Palette.RedDark))
                    g.FillPath(b, tip);
                using (var pen = new Pen(Outline, 2.2f))
                    g.DrawPath(pen, tip);
            }

            // Beyaz kürk bant ve ponpon
            var trim = new RectangleF(-31, -4, 58, 13);
            using (var p = Shapes.RoundedRect(trim, 6.5f))
            using (var b = new SolidBrush(Palette.Snow))
            using (var pen = new Pen(Outline, 2.2f))
            {
                g.FillPath(b, p);
                g.DrawPath(pen, p);
                g.FillEllipse(b, -50, -2, 15, 15);
                g.DrawEllipse(pen, -50, -2, 15, 15);
            }
            Shapes.FillEllipse(g, Palette.SnowShade, -21, 6, 8, 2);
            g.Restore(s);
        }
    }
}
