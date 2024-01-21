using System;
using System.Collections.Generic;
using System.Text;

namespace Fluxify.Networking
{
    class GameLogic
    {
        /// Oyundan gelen tüm verileri çalıştır.
        public static void Update()
        {
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    _client.player.Update();
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
