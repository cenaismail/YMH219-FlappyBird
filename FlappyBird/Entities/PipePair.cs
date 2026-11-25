using System;
using System.Drawing;

namespace FlappyBird.Entities
{
    /// <summary>
    /// Arasında kuşun geçeceği boşluk bulunan üst + alt boru ikilisi.
    /// Her karede sola kayar; ekranın solundan çıkınca sağa geri taşınır.
    /// </summary>
    public class PipePair
    {
        public const float PipeWidth = 74f;

        private readonly Pipe _top;
        private readonly Pipe _bottom;
        private readonly float _screenHeight;

        /// <summary>Kuş bu boruyu geçti mi? (Skor bir kez artsın diye.)</summary>
        public bool Passed { get; private set; }

        /// <summary>Boşluğun dikey merkezi.</summary>
        public float GapCenter { get; private set; }

        /// <summary>Boşluğun yüksekliği.</summary>
        public float GapSize { get; private set; }

        public float X
        {
            get { return _top.X; }
        }

        /// <summary>Borunun (ağız kısmı dahil) en sağ noktası.</summary>
        public float Right
        {
            get { return X + PipeWidth + Pipe.LipOverhang; }
        }

        public PipePair(float x, float gapCenter, float gapSize, float screenHeight)
        {
            _screenHeight = screenHeight;
            _top = new Pipe(x, PipeWidth, -20, 0, false);
            _bottom = new Pipe(x, PipeWidth, 0, screenHeight + 20, true);
            Reposition(x, gapCenter, gapSize);
        }

        /// <summary>Boruyu yeni bir X konumuna ve yeni bir boşluk yüksekliğine taşır.</summary>
        public void Reposition(float x, float gapCenter, float gapSize)
        {
            GapCenter = gapCenter;
            GapSize = gapSize;
            Passed = false;

            _top.X = x;
            _top.Bottom = gapCenter - gapSize / 2;

            _bottom.X = x;
            _bottom.Top = gapCenter + gapSize / 2;
            _bottom.Bottom = _screenHeight + 20;
        }

        /// <summary>Boruyu sola kaydırır.</summary>
        public void Move(float dx)
        {
            _top.X -= dx;
            _bottom.X -= dx;
        }

        /// <summary>Boru tamamen ekranın solundan çıktı mı?</summary>
        public bool IsOffScreen
        {
            get { return Right < 0; }
        }

        /// <summary>Kuş borunun ortasını geçtiyse true döndürür (yalnızca bir kez).</summary>
        public bool TryPass(float birdX)
        {
            if (!Passed && X + PipeWidth / 2 < birdX)
            {
                Passed = true;
                return true;
            }
            return false;
        }

        public bool Collides(RectangleF birdBounds)
        {
            return _top.Collides(birdBounds) || _bottom.Collides(birdBounds);
        }

        public void Draw(Graphics g, float time)
        {
            _top.Draw(g, time);
            _bottom.Draw(g, time);
        }
    }
}
