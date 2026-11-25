using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FlappyBird.Core;

namespace FlappyBird
{
    /// <summary>
    /// Ana pencere. Zamanlayıcı ile oyun döngüsünü çalıştırır, klavye/fare olaylarını
    /// Game sınıfına iletir ve sanal ekranı pencere boyutuna ölçekleyerek çizer.
    /// </summary>
    public partial class Form1 : Form
    {
        private readonly Game _game = new Game();
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private readonly Stopwatch _clock = new Stopwatch();
        private double _lastTime;

        public Form1()
        {
            InitializeComponent();

            Text = "Flappy Bird - Christmas Edition";
            // Pencere, ekranın kullanılabilir yüksekliğinin %88'i kadar olsun (9:16 oran korunur)
            Rectangle work = Screen.PrimaryScreen.WorkingArea;
            int height = (int)(work.Height * 0.88);
            int width = (int)(height * Game.Width / Game.Height);
            ClientSize = new Size(width, height - SystemInformation.CaptionHeight);
            MinimumSize = new Size(260, 420);
            FormBorderStyle = FormBorderStyle.Sizable; // kenarından sürükleyerek boyutlandırılabilir
            MaximizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(10, 20, 62);
            KeyPreview = true;

            // Titremeyi önlemek için çift tamponlu çizim
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _timer.Interval = 16; // ~60 FPS
            _timer.Tick += OnTick;
            _clock.Start();
            _timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            double now = _clock.Elapsed.TotalSeconds;
            float dt = (float)Math.Min(now - _lastTime, 0.05);
            _lastTime = now;

            _game.Update(dt);
            Invalidate();
        }

        // ---------------------------------------------------------- Ölçekleme

        private float ViewScale { get { return Math.Min(ClientSize.Width / Game.Width, ClientSize.Height / Game.Height); } }

        private PointF Offset
        {
            get
            {
                float s = ViewScale;
                return new PointF((ClientSize.Width - Game.Width * s) / 2, (ClientSize.Height - Game.Height * s) / 2);
            }
        }

        /// <summary>Pencere koordinatını 450x800'lük sanal ekran koordinatına çevirir.</summary>
        private PointF ToVirtual(Point p)
        {
            PointF o = Offset;
            float s = ViewScale;
            return new PointF((p.X - o.X) / s, (p.Y - o.Y) / s);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            PointF o = Offset;
            float s = ViewScale;

            g.TranslateTransform(o.X, o.Y);
            g.ScaleTransform(s, s);
            g.SetClip(new RectangleF(0, 0, Game.Width, Game.Height));
            _game.Draw(g);
        }

        // ---------------------------------------------------------- Olaylar

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Space:
                case Keys.Enter:
                case Keys.Up:
                    _game.Start();
                    break;
                case Keys.Escape:
                    _game.BackToMenu();
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            PointF p = ToVirtual(e.Location);
            _game.OnMouseMove(p);
            Cursor = _game.IsOverButton(p) ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left)
                _game.OnMouseClick(ToVirtual(e.Location));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
