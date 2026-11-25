using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using FlappyBird.Rendering;

namespace FlappyBird.Scene
{
    /// <summary>İki nokta arasında sarkan, yanıp sönen yılbaşı ışıkları.</summary>
    public class LightString
    {
        private readonly PointF _start;
        private readonly PointF _end;
        private readonly float _sag;
        private readonly int _bulbCount;
        private readonly float _phase;

        public LightString(PointF start, PointF end, float sag, int bulbCount, float phase)
        {
            _start = start;
            _end = end;
            _sag = sag;
            _bulbCount = bulbCount;
            _phase = phase;
        }

        private PointF PointAt(float t)
        {
            // İkinci dereceden Bezier: orta kontrol noktası aşağı sarkar
            float cx = (_start.X + _end.X) / 2;
            float cy = (_start.Y + _end.Y) / 2 + _sag * 2;
            float u = 1 - t;
            return new PointF(
                u * u * _start.X + 2 * u * t * cx + t * t * _end.X,
                u * u * _start.Y + 2 * u * t * cy + t * t * _end.Y);
        }

        public void Draw(Graphics g, float time)
        {
            var wire = new PointF[21];
            for (int i = 0; i < wire.Length; i++) wire[i] = PointAt(i / 20f);
            using (var pen = new Pen(Color.FromArgb(30, 34, 30), 1.6f))
                g.DrawLines(pen, wire);

            for (int i = 0; i < _bulbCount; i++)
            {
                PointF p = PointAt((i + 0.5f) / _bulbCount);
                Color c = Palette.Bulbs[i % Palette.Bulbs.Length];
                float twinkle = 0.65f + 0.35f * (float)Math.Sin(time * 3 + i * 1.7 + _phase);

                Shapes.FillGlow(g, new PointF(p.X, p.Y + 4), 11, c, (int)(150 * twinkle));
                using (var socket = new SolidBrush(Color.FromArgb(40, 44, 40)))
                    g.FillRectangle(socket, p.X - 1.6f, p.Y - 1, 3.2f, 3);
                Shapes.FillEllipse(g, c, p.X, p.Y + 5, 2.8f, 4f);
                Shapes.FillEllipse(g, Color.FromArgb((int)(220 * twinkle), Color.White), p.X - 0.8f, p.Y + 3.6f, 0.9f, 1.4f);
            }
        }
    }
}
