using System;
using System.IO;

namespace FlappyBird.Core
{
    /// <summary>
    /// En yüksek skoru bir metin dosyasında saklar.
    /// Dosya, programın .exe dosyasıyla aynı klasördeki "highscore.txt" dosyasıdır.
    /// Dosya yoksa, okunamıyorsa ya da içeriği bozuksa rekor 0 kabul edilir ve oyun çalışmaya devam eder.
    /// </summary>
    public class HighScore
    {
        private const string FileName = "highscore.txt";

        /// <summary>Şimdiye kadarki en yüksek skor.</summary>
        public int Best { get; private set; }

        /// <summary>Rekorun saklandığı dosyanın tam yolu.</summary>
        public string FilePath { get; private set; }

        /// <summary>Son dosya işleminde hata olduysa açıklaması, yoksa null.</summary>
        public string LastError { get; private set; }

        public HighScore()
        {
            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
            Load();
        }

        /// <summary>Rekoru dosyadan okur.</summary>
        public void Load()
        {
            Best = 0;
            LastError = null;

            try
            {
                if (!File.Exists(FilePath))
                    return; // İlk çalıştırma: henüz rekor yok

                string text = File.ReadAllText(FilePath).Trim();
                int value;
                if (int.TryParse(text, out value) && value >= 0)
                    Best = value;
                else
                    LastError = "Rekor dosyasının içeriği geçersiz, 0 kabul edildi.";
            }
            catch (IOException ex)
            {
                LastError = "Rekor dosyası okunamadı: " + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                LastError = "Rekor dosyasına erişim izni yok: " + ex.Message;
            }
        }

        /// <summary>
        /// Oyun bitince çağrılır. Skor rekoru geçtiyse rekoru günceller, dosyaya yazar ve true döndürür.
        /// </summary>
        public bool Submit(int score)
        {
            if (score <= Best)
                return false;

            Best = score;
            Save();
            return true;
        }

        private void Save()
        {
            try
            {
                File.WriteAllText(FilePath, Best.ToString());
                LastError = null;
            }
            catch (IOException ex)
            {
                LastError = "Rekor kaydedilemedi: " + ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                LastError = "Rekor dosyasına yazma izni yok: " + ex.Message;
            }
        }
    }
}
