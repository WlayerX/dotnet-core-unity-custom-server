using System;
using System.Collections.Generic;
using System.Text;

namespace Fluxify.Networking
{
    class ServerSend
    {
        /// <summary>
        /// Bir paketi TCP üzerinden belirli bir istemciye gönderir.
        /// </summary>
        /// <param name="_toClient">Paketi gönderilecek istemci.</param>
        /// <param name="_packet">İstemciye gönderilecek paket.</param>
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        /// <summary>
        /// Bir paketi UDP üzerinden belirli bir istemciye gönderir.
        /// </summary>
        /// <param name="_toClient">Paketi gönderilecek istemci.</param>
        /// <param name="_packet">İstemciye gönderilecek paket.</param>
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        /// <summary>
        /// Bir paketi TCP üzerinden tüm istemcilere gönderir.
        /// </summary>
        /// <param name="_packet">Gönderilecek paket.</param>
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        /// <summary>
        /// Bir paketi TCP üzerinden belirli bir istemci hariç tüm istemcilere gönderir.
        /// </summary>
        /// <param name="_exceptClient">Veri gönderilmeyecek istemci.</param>
        /// <param name="_packet">Gönderilecek paket.</param>
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        /// <summary>
        /// Bir paketi UDP üzerinden tüm istemcilere gönderir.
        /// </summary>
        /// <param name="_packet">Gönderilecek paket.</param>
        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        /// <summary>
        /// Bir paketi UDP üzerinden belirli bir istemci hariç tüm istemcilere gönderir.
        /// </summary>
        /// <param name="_exceptClient">Veri gönderilmeyecek istemci.</param>
        /// <param name="_packet">Gönderilecek paket.</param>
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #region Paketler
        /// <summary>
        /// Verilen istemciye hoş geldin mesajını gönderir.
        /// </summary>
        /// <param name="_toClient">Paketi alacak istemci.</param>
        /// <param name="_msg">Gönderilecek mesaj.</param>
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        /// <summary>
        /// Bir istemcinin bir oyuncuyu oluşturmasını ister.
        /// </summary>
        /// <param name="_toClient">Oyuncuyu oluşturacak istemci.</param>
        /// <param name="_player">Oluşturulacak oyuncu.</param>
        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPData(_toClient, _packet);
            }
        }

        /// <summary>
        /// Bir oyuncunun güncellenmiş pozisyonunu tüm istemcilere gönderir.
        /// </summary>
        /// <param name="_player">Pozisyonu güncellenecek oyuncu.</param>
        public static void PlayerPosition(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);

                SendUDPDataToAll(_packet);
            }
        }

        /// <summary>
        /// Bir oyuncunun güncellenmiş rotasyonunu kendisine haricindeki tüm istemcilere gönderir.
        /// Bu, yerel oyuncunun rotasyonunu üzerine yazmamak için yapılır.
        /// </summary>
        /// <param name="_player">Rotasyonu güncellenecek oyuncu.</param>
        public static void PlayerRotation(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);

                SendUDPDataToAll(_player.id, _packet);
            }
        }
        #endregion
    }
}
