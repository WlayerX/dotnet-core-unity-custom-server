using System;
using System.Threading;

namespace Fluxify.Networking
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Fluxify Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(50, 26950);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Ana thread başlatıldı. Saniyede {Constants.TICKS_PER_SEC} tik çalışıyor.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    // Eğer bir sonraki döngü için zaman geçmişse, yani başka bir tik çalıştırma zamanı gelmişse
                    GameLogic.Update(); // Oyun mantığını çalıştır

                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK); // Bir sonraki tikin ne zaman çalıştırılması gerektiğini hesapla

                    if (_nextLoop > DateTime.Now)
                    {
                        // Eğer bir sonraki tikin çalıştırılma zamanı gelecekteyse, yani sunucu geride çalışmıyorsa
                        Thread.Sleep(_nextLoop - DateTime.Now); // Thread'i uyut ve tekrar ihtiyaç duyulana kadar bekle.
                    }
                }
            }
        }
    }
}
