using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace Fluxify.Networking
{
    // Bu sınıf, oyun sunucusundaki her bir oyuncuyu temsil eder ve onların ağ bağlantılarını yönetir.
    class Client
    {
        public static int dataBufferSize = 2048; // Veri boyutu sınırı

        public int id; // Oyuncu ID'si
        public Player player; // Oyuncu nesnesi
        public TCP tcp; // TCP bağlantısı
        public UDP udp; // UDP bağlantısı

        // Yeni bir oyuncu oluşturur ve ona ait TCP ve UDP bağlantılarını başlatır.
        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        // Bu sınıf içindeki TCP ve UDP alt sınıfları, ağ bağlantılarını yönetir.
        public class TCP
        {
            public TcpClient socket; // TCP soketi
            private readonly int id; // Oyuncu ID'si
            private NetworkStream stream; // Ağ akışı
            private Packet receivedData; // Alınan veri paketi
            private byte[] receiveBuffer; // Alınan veriyi depolayan buffer

            // Yeni bağlanan oyuncunun TCP ile ilgili bilgilerini başlatır.
            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Sunucuya hoş geldin!");
            }

            // Veriyi TCP aracılığıyla oyuncuya gönderir.
            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"TCP ile veri gönderme hatası - Oyuncu {id}: {_ex}");
                }
            }

            // Ağ akışından gelen veriyi okur.
            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"TCP ile veri alma hatası - Oyuncu {id}: {_ex}");
                    Server.clients[id].Disconnect();
                }
            }

            // Gelen veriyi işlemek üzere hazırlar.
            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            // TCP bağlantısını kapatır ve temizler.
            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint; // UDP bağlantı noktası
            private int id; // Oyuncu ID'si

            // Yeni bağlanan oyuncunun UDP ile ilgili bilgilerini başlatır.
            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            // Veriyi UDP aracılığıyla oyuncuya gönderir.
            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            // Gelen veriyi işlemek üzere hazırlar.
            public void HandleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }

            // UDP bağlantısını temizler.
            public void Disconnect()
            {
                endPoint = null;
            }
        }

        // Oyuncuyu oyun dünyasına ekler ve diğer oyunculara yeni oyuncuyu bildirir.
        public void SendIntoGame(string _playerName)
        {
            player = new Player(id, _playerName, new Vector3(0, 0, 0));

            // Yeni oyuncuya diğer oyuncuları gönder
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    if (_client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, _client.player);
                    }
                }
            }

            // Yeni oyuncuyu diğer oyunculara (kendisi dahil) gönder
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(_client.id, player);
                }
            }
        }

        // Oyuncuyu bağlantıdan çıkarır ve tüm ağ trafiğini durdurur.
        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} bağlantısı kesildi.");

            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
