using System.Drawing;

namespace FlappyBird.Rendering
{
    /// <summary>Tasarımda kullanılan tüm renkler tek yerde.</summary>
    public static class Palette
    {
        // Gökyüzü
        public static readonly Color SkyTop = Color.FromArgb(10, 20, 62);
        public static readonly Color SkyMid = Color.FromArgb(28, 44, 110);
        public static readonly Color SkyBottom = Color.FromArgb(52, 70, 140);
        public static readonly Color Cloud = Color.FromArgb(58, 72, 140);
        public static readonly Color CloudLight = Color.FromArgb(78, 94, 165);

        // Ay
        public static readonly Color MoonLight = Color.FromArgb(255, 246, 205);
        public static readonly Color MoonDark = Color.FromArgb(238, 214, 140);
        public static readonly Color Silhouette = Color.FromArgb(48, 36, 52);

        // Kar
        public static readonly Color Snow = Color.FromArgb(250, 252, 255);
        public static readonly Color SnowShade = Color.FromArgb(196, 212, 240);

        // Borular
        public static readonly Color PipeDark = Color.FromArgb(52, 120, 30);
        public static readonly Color PipeMid = Color.FromArgb(118, 190, 48);
        public static readonly Color PipeLight = Color.FromArgb(178, 228, 96);
        public static readonly Color PipeOutline = Color.FromArgb(28, 58, 22);

        // Başlık ve buton
        public static readonly Color Outline = Color.FromArgb(40, 26, 30);
        public static readonly Color Cream = Color.FromArgb(255, 246, 226);
        public static readonly Color CreamShade = Color.FromArgb(240, 220, 186);
        public static readonly Color Red = Color.FromArgb(236, 50, 44);
        public static readonly Color RedDark = Color.FromArgb(178, 20, 26);
        public static readonly Color Gold = Color.FromArgb(255, 204, 58);

        // Doğa / ev
        public static readonly Color PineDark = Color.FromArgb(22, 58, 48);
        public static readonly Color Pine = Color.FromArgb(34, 98, 62);
        public static readonly Color FarTrees = Color.FromArgb(30, 42, 92);
        public static readonly Color Wall = Color.FromArgb(86, 56, 74);
        public static readonly Color WallDark = Color.FromArgb(58, 38, 56);
        public static readonly Color Window = Color.FromArgb(255, 206, 110);
        public static readonly Color Wood = Color.FromArgb(104, 68, 50);

        // Ampul renkleri
        public static readonly Color[] Bulbs =
        {
            Color.FromArgb(255, 70, 60),
            Color.FromArgb(255, 214, 90),
            Color.FromArgb(255, 245, 200),
            Color.FromArgb(255, 120, 60)
        };
    }
}
