using System;
using System.Collections.Generic;
using System.Drawing;
using FlappyBird.Rendering;

namespace FlappyBird.Scene
{
    /// <summary>Tek bir kar tanesinin konumu ve hızı.</summary>
    public class Snowflake
    {
        public float X;
        public float Y;
        public float Speed;
        public float Size;
        public float Drift;
    }

    /// <summary>Ekranda sürekli yağan kar.</summary>
    public class Snowfall
    {
        private readonly List<Snowflake> _flakes = new List<Snowflake>();
        private readonly Random _rnd = new Random(7);
        private readonly float _width;
        private readonly float _height;

        public Snowfall(int count, float width, float height)
        {
            _width = width;
            _height = height;
            for (int i = 0; i < count; i++)
                _flakes.Add(CreateFlake((float)_rnd.NextDouble() * height));
        }

        private Snowflake CreateFlake(float y)
        {
            float size = 1.2f + (float)_rnd.NextDouble() * 2.6f;
            return new Snowflake
            {
                X = (float)_rnd.NextDouble() * _width,
                Y = y,
                Size = size,
                Speed = 18 + size * 14,
                Drift = (float)_rnd.NextDouble() * 6.28f
            };
        }

        public void Update(float dt, float time)
        {
            for (int i = 0; i < _flakes.Count; i++)
            {
                Snowflake f = _flakes[i];
                f.Y += f.Speed * dt;
                f.X += (float)Math.Sin(time * 0.9 + f.Drift) * 12 * dt;
                if (f.Y > _height + 5)
                    _flakes[i] = CreateFlake(-5);
            }
        }

        public void Draw(Graphics g)
        {
            using (var brush = new SolidBrush(Color.FromArgb(220, Palette.Snow)))
            {
                foreach (Snowflake f in _flakes)
                    g.FillEllipse(brush, f.X - f.Size, f.Y - f.Size, f.Size * 2, f.Size * 2);
            }
        }
    }
}
