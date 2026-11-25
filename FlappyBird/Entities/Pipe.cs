using System.Drawing;
using System.Drawing.Drawing2D;
using FlappyBird.Rendering;
using FlappyBird.Scene;

namespace FlappyBird.Entities
{
    /// <summary>
    /// Karlı yeşil boru. Ağız kısmı (lip) borunun üstünde ya da altında olabilir.
    /// Oyunda PipePair sınıfı X değerini her karede azaltarak boruyu sola kaydırır.
    /// </summary>
    public class Pipe
    {
        public const float LipHeight = 26f;
        public const float LipOverhang = 7f;

        public float X { get; set; }
        public float Width { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }

        /// <summary>true: boru aşağıdan yukarı uzanır, ağzı üsttedir.</summary>
        public bool OpensUpward { get; set; }

        public Pipe(float x, float width, float top, float bottom, bool opensUpward)
        {
            X = x;
            Width = width;
            Top = top;
            Bottom = bottom;
            OpensUpward = opensUpward;
        }

        /// <summary>Borunun gövdesi (ağız hariç).</summary>
        public RectangleF BodyBounds
        {
            get { return new RectangleF(X, Top, Width, Bottom - Top); }
        }

        /// <summary>Borunun daha geniş olan ağız kısmı.</summary>
        public RectangleF LipBounds
        {
            get
            {
                float lipY = OpensUpward ? Top : Bottom - LipHeight;
                return new RectangleF(X - LipOverhang, lipY, Width + LipOverhang * 2, LipHeight);
            }
        }

        /// <summary>Verilen dikdörtgen (kuş) boruya değiyor mu?</summary>
        public bool Collides(RectangleF other)
        {
            return BodyBounds.IntersectsWith(other) || LipBounds.IntersectsWith(other);
        }

        /// <summary>Boruyu, gövdesine sarılı yanıp sönen ışıklarla birlikte çizer.</summary>
        public void Draw(Graphics g, float time)
        {
            Draw(g);
            DrawLights(g, time);
        }

        private void DrawLights(Graphics g, float time)
        {
            // Işık dizisi ağızdan biraz uzakta, gövdeye çapraz sarkar
            float y = OpensUpward ? Top + LipHeight + 34 : Bottom - LipHeight - 30;
            if (y < 10 || y > 790) return; // boru çok kısaysa ışık ekleme

            float phase = OpensUpward ? 0f : 2.5f;
            var lights = new LightString(
                new PointF(X - 4, y + 14),
                new PointF(X + Width + 4, y - 14),
                5, 6, phase);
            lights.Draw(g, time);
        }

        public void Draw(Graphics g)
        {
            RectangleF body = BodyBounds;
            RectangleF lip = LipBounds;

            using (var outline = new Pen(Palette.PipeOutline, 2.5f))
            {
                FillTube(g, body);
                g.DrawRectangle(outline, body.X, body.Y, body.Width, body.Height);

                FillTube(g, lip);
                using (var lipPath = Shapes.RoundedRect(lip, 4))
                    g.DrawPath(outline, lipPath);
            }

            // Kar her zaman ağzın üst yüzeyinde birikir
            Shapes.FillSnowCap(g, lip.X - 3, lip.Y, lip.Width + 6, 12, (int)Width + (int)Top);
        }

        private static void FillTube(Graphics g, RectangleF r)
        {
            using (var brush = new LinearGradientBrush(new PointF(r.Left, 0), new PointF(r.Right, 0),
                       Palette.PipeDark, Palette.PipeDark))
            {
                var blend = new ColorBlend
                {
                    Colors = new[] { Palette.PipeDark, Palette.PipeLight, Palette.PipeMid, Palette.PipeDark },
                    Positions = new[] { 0f, 0.28f, 0.6f, 1f }
                };
                brush.InterpolationColors = blend;
                g.FillRectangle(brush, r);
            }
        }
    }
}
