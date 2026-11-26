using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using FlappyBird.Entities;
using FlappyBird.Rendering;
using FlappyBird.Scene;
using FlappyBird.UI;

namespace FlappyBird.Core
{
    /// <summary>
    /// Oyunun tüm nesnelerini ve durumunu yöneten sınıf.
    /// Form yalnızca olayları bu sınıfa iletir; çizim ve mantık burada.
    /// Tüm koordinatlar 450x800'lük sanal bir ekrana göredir, form boyutu değişince ölçeklenir.
    /// </summary>
    public class Game
    {
        public const float Width = 450f;
        public const float Height = 800f;

        /// <summary>Kuşun çarptığında oyunun bittiği zemin çizgisi (çitin üstü).</summary>
        public const float GroundY = 752f;

        // Oyun ayarları
        private const int PipeCount = 3;
        private const float PipeSpacing = 275f;      // iki boru arası yatay mesafe
        private const float GapSize = 215f;           // borular arasındaki boşluk
        private const float StartSpeed = 150f;        // başlangıç kayma hızı (piksel/sn)
        private const float SpeedPerPoint = 5f;       // her skorda hız artışı
        private const float MaxSpeed = 280f;
        private static readonly PointF BirdPlayStart = new PointF(130, 360);

        private readonly Background _background;
        private readonly Snowfall _snowfall;
        private readonly StartScreen _startScreen;
        private readonly Bird _bird;
        private readonly List<Pipe> _menuPipes = new List<Pipe>();
        private readonly List<LightString> _lights = new List<LightString>();
        private readonly List<PipePair> _pipes = new List<PipePair>();
        private readonly Random _random = new Random();
        private readonly HighScore _highScore = new HighScore();

        private float _time;

        public GameState State { get; private set; }
        public int Score { get; private set; }

        /// <summary>Son oyunda rekor kırıldıysa true (Game Over panelinde gösterilir).</summary>
        public bool IsNewRecord { get; private set; }

        /// <summary>Boruların o anki kayma hızı; skor arttıkça yükselir.</summary>
        public float Speed { get; private set; }

        public Game()
        {
            _background = new Background(Width, Height);
            _snowfall = new Snowfall(90, Width, Height);
            _startScreen = new StartScreen(Width);
            _bird = new Bird(214, 196);

            // Menüdeki dört dekoratif boru (görseldeki yerleşim)
            _menuPipes.Add(new Pipe(40, 74, -10, 168, false));
            _menuPipes.Add(new Pipe(392, 74, -10, 222, false));
            _menuPipes.Add(new Pipe(16, 96, 650, Height + 10, true));
            _menuPipes.Add(new Pipe(386, 74, 668, Height + 10, true));

            _lights.Add(new LightString(new PointF(30, 92), new PointF(124, 22), 8, 7, 0));
            _lights.Add(new LightString(new PointF(386, 116), new PointF(456, 62), 6, 5, 1));
            _lights.Add(new LightString(new PointF(4, 700), new PointF(118, 760), 10, 7, 2));
            _lights.Add(new LightString(new PointF(380, 760), new PointF(456, 700), 8, 5, 3));

            State = GameState.Menu;
        }

        // ------------------------------------------------------------- Oyun döngüsü

        public void Update(float dt)
        {
            _time += dt;
            _snowfall.Update(dt, _time);

            switch (State)
            {
                case GameState.Menu:
                    _bird.Hover(_time);
                    break;

                case GameState.Playing:
                    _bird.Update(dt, _time);
                    UpdatePipes(dt);
                    CheckCollisions();
                    break;

                case GameState.GameOver:
                    // Kuş yere değene kadar düşmeye devam eder
                    if (_bird.Y + Bird.HitRadiusY < GroundY)
                    {
                        _bird.Update(dt, _time);
                        if (_bird.Y + Bird.HitRadiusY >= GroundY) _bird.LandOn(GroundY);
                    }
                    break;
            }
        }

        private void CreatePipes()
        {
            _pipes.Clear();
            for (int i = 0; i < PipeCount; i++)
                _pipes.Add(new PipePair(Width + 60 + i * PipeSpacing, RandomGapCenter(), GapSize, Height));
        }

        /// <summary>Boşluğun merkezi için ekranın üstüne/zemine fazla yaklaşmayan rastgele bir değer.</summary>
        private float RandomGapCenter()
        {
            float min = 90 + GapSize / 2;
            float max = GroundY - 70 - GapSize / 2;
            return min + (float)_random.NextDouble() * (max - min);
        }

        private void UpdatePipes(float dt)
        {
            float rightMost = 0;
            foreach (PipePair p in _pipes)
            {
                p.Move(Speed * dt);
                if (p.X > rightMost) rightMost = p.X;

                if (p.TryPass(_bird.X))
                {
                    Score++;
                    Speed = Math.Min(MaxSpeed, Speed + SpeedPerPoint);
                }
            }

            // Ekrandan çıkan boruyu en sağdaki borunun arkasına taşı
            foreach (PipePair p in _pipes)
            {
                if (p.IsOffScreen)
                {
                    p.Reposition(rightMost + PipeSpacing, RandomGapCenter(), GapSize);
                    rightMost = p.X;
                }
            }
        }

        private void CheckCollisions()
        {
            // Borulara çarpma
            foreach (PipePair p in _pipes)
            {
                if (p.Collides(_bird.Bounds))
                {
                    GameOver();
                    return;
                }
            }

            // Zemine çarpma
            if (_bird.Y + Bird.HitRadiusY >= GroundY)
            {
                _bird.LandOn(GroundY);
                GameOver();
                return;
            }

            // Ekranın üstünden çıkmasın
            if (_bird.Y - Bird.HitRadiusY < 0)
                _bird.Y = Bird.HitRadiusY;
        }

        private void GameOver()
        {
            State = GameState.GameOver;
            IsNewRecord = _highScore.Submit(Score);
        }

        // ------------------------------------------------------------- Çizim

        public void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            _background.DrawSky(g, _time);
            _background.DrawLandscape(g, _time);

            if (State == GameState.Menu)
            {
                foreach (Pipe p in _menuPipes) p.Draw(g);
                foreach (LightString l in _lights) l.Draw(g, _time);
                _startScreen.HighScore = _highScore.Best;
                _startScreen.Draw(g, _time);
            }
            else
            {
                foreach (PipePair p in _pipes) p.Draw(g, _time);
            }

            _bird.Draw(g);
            _snowfall.Draw(g);

            if (State == GameState.Playing || State == GameState.GameOver)
                DrawScore(g);
            if (State == GameState.GameOver)
                DrawGameOver(g);
        }

        private void DrawScore(Graphics g)
        {
            DrawOutlinedText(g, Score.ToString(), 56, new RectangleF(0, 40, Width, 80), Palette.Cream, 8);
        }

        private void DrawGameOver(Graphics g)
        {
            var panel = new RectangleF(55, 270, Width - 110, 240);
            using (var path = Shapes.RoundedRect(panel, 24))
            using (var bg = new SolidBrush(Color.FromArgb(215, 18, 28, 80)))
            using (var pen = new Pen(Palette.Outline, 4))
            {
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
            }
            Shapes.FillSnowCap(g, panel.X + 10, panel.Y + 1, panel.Width - 20, 12, 4);

            DrawOutlinedText(g, "Game Over", 50, new RectangleF(panel.X, panel.Y + 24, panel.Width, 64), Palette.Red, 8);
            DrawOutlinedText(g, "Skor: " + Score, 30, new RectangleF(panel.X, panel.Y + 88, panel.Width, 40), Palette.Cream, 6);
            DrawOutlinedText(g, "Rekor: " + _highScore.Best, 24, new RectangleF(panel.X, panel.Y + 128, panel.Width, 34), Palette.Gold, 5);

            if (IsNewRecord)
            {
                // Yeni rekor yazısı hafifçe büyüyüp küçülür
                float pulse = 1f + 0.06f * (float)Math.Sin(_time * 6);
                GraphicsState s = g.Save();
                g.TranslateTransform(panel.Right - 30, panel.Y + 16);
                g.RotateTransform(12);
                g.ScaleTransform(pulse, pulse);
                DrawOutlinedText(g, "Yeni Rekor!", 22, new RectangleF(-80, -18, 160, 36), Palette.Gold, 6);
                g.Restore(s);
            }

            DrawOutlinedText(g, "Space: Yeniden Basla   Esc: Menu", 17, new RectangleF(panel.X, panel.Y + 182, panel.Width, 30),
                Palette.Cream, 4);
        }

        private static void DrawOutlinedText(Graphics g, string text, float size, RectangleF area, Color fill, float outline)
        {
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var path = new GraphicsPath())
            {
                path.AddString(text, FontManager.Title, 0, size, area, format);
                using (var pen = new Pen(Palette.Outline, outline) { LineJoin = LineJoin.Round })
                    g.DrawPath(pen, path);
                using (var b = new SolidBrush(fill))
                    g.FillPath(b, path);
            }
        }

        // ------------------------------------------------------------- Girdi

        /// <summary>Space / yukarı ok / tıklama: duruma göre başlat, zıpla veya yeniden başlat.</summary>
        public void OnJump()
        {
            switch (State)
            {
                case GameState.Menu:
                    Start();
                    break;
                case GameState.Playing:
                    _bird.Jump();
                    break;
                case GameState.GameOver:
                    Start();
                    break;
            }
        }

        /// <summary>Yeni oyun: skor sıfırlanır, kuş başa döner ve ilk zıplamayı yapar.</summary>
        public void Start()
        {
            Score = 0;
            Speed = StartSpeed;
            CreatePipes();
            _bird.Reset(BirdPlayStart.X, BirdPlayStart.Y);
            _bird.Jump();
            State = GameState.Playing;
        }

        public void BackToMenu()
        {
            _bird.Reset();
            State = GameState.Menu;
        }

        public void OnMouseMove(PointF p)
        {
            _startScreen.IsButtonHovered = State == GameState.Menu && _startScreen.HitTest(p);
        }

        public void OnMouseClick(PointF p)
        {
            if (State == GameState.Menu)
            {
                if (_startScreen.HitTest(p)) Start();
            }
            else
            {
                OnJump();
            }
        }

        public bool IsOverButton(PointF p)
        {
            return State == GameState.Menu && _startScreen.HitTest(p);
        }
    }
}
