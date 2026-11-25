using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace FlappyBird.Rendering
{
    /// <summary>
    /// Başlık yazı tipini (Lilita One) dosyadan yükler. Dosya bulunamazsa
    /// Windows'ta hazır gelen kalın bir yazı tipine geri döner.
    /// </summary>
    public static class FontManager
    {
        private static PrivateFontCollection _collection;
        private static FontFamily _title;

        public static FontFamily Title
        {
            get
            {
                if (_title == null) _title = LoadTitleFont();
                return _title;
            }
        }

        private static FontFamily LoadTitleFont()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", "LilitaOne-Regular.ttf");
            try
            {
                if (File.Exists(path))
                {
                    _collection = new PrivateFontCollection();
                    _collection.AddFontFile(path);
                    return _collection.Families[0];
                }
            }
            catch (Exception)
            {
                // Yazı tipi yüklenemezse sessizce yedek yazı tipine geç
            }

            foreach (string name in new[] { "Lilita One", "Segoe UI Black", "Arial Black", "Arial" })
            {
                try { return new FontFamily(name); }
                catch (ArgumentException) { }
            }
            return FontFamily.GenericSansSerif;
        }
    }
}
