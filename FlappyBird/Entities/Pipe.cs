using System.Drawing;
using System.Drawing.Drawing2D;
using FlappyBird.Rendering;

namespace FlappyBird.Entities
{
    /// <summary>
    /// Karlı yeşil boru. Ağız kısmı (lip) borunun üstünde ya da altında olabilir.
    /// Oyun aşamasında X değeri her karede azaltılarak boru sola kaydırılacak.
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

        /// <summary>Çarpışma kontrolü için borunun kapladığı alan.</summary>
        public RectangleF Bounds
        {
            get { return new RectangleF(X - LipOverhang, Top, Width + LipOverhang * 2, Bottom - Top); }
        }

        public void Draw(Graphics g)
        {
            float lipY = OpensUpward ? Top : Bottom - LipHeight;
            var body = new RectangleF(X, Top, Width, Bottom - Top);
            var lip = new RectangleF(X - LipOverhang, lipY, Width + LipOverhang * 2, LipHeight);

            using (var outline = new Pen(Palette.PipeOutline, 2.5f))
            {
                FillTube(g, body);
                g.DrawRectangle(outline, body.X, body.Y, body.Width, body.Height);

                FillTube(g, lip);
                using (var lipPath = Shapes.RoundedRect(lip, 4))
                    g.DrawPath(outline, lipPath);
            }

            // Kar her zaman ağzın üst yüzeyinde birikir
            Shapes.FillSnowCap(g, lip.X - 3, lip.Y, lip.Width + 6, 12, (int)X + (int)Top);
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
