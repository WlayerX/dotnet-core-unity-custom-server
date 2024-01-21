using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Fluxify.Networking
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        /// <summary>Başlangıçta sunucuyu başlatır.</summary>
        /// <param name="_maxPlayers">Aynı anda bağlanabilecek maksimum oyuncu sayısı.</param>
        /// <param name="_port">Sunucunun başlayacağı port.</param>
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Sunucu başlatılıyor...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Sunucu {Port} portunda başlatıldı.");
        }

        /// <summary>Yeni TCP bağlantılarını işler.</summary>
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Gelen bağlantı: {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} bağlanamadı: Sunucu dolu!");
        }

        /// <summary>Gelen UDP verilerini alır.</summary>
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (clients[_clientId].udp.endPoint == null)
                    {
                        // Eğer bu yeni bir bağlantıysa
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        // İstemcinin başka biri tarafından taklit edilmediğinden emin olmak için yanlış bir clientID gönderilmediğinden emin olur
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"UDP verisi alınırken hata oluştu: {_ex}");
            }
        }

        /// <summary>Belirtilen uç noktaya bir paket gönderir.</summary>
        /// <param name="_clientEndPoint">Paketin gönderileceği uç nokta.</param>
        /// <param name="_packet">Gönderilecek paket.</param>
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"UDP ile {_clientEndPoint} adresine veri gönderme hatası: {_ex}");
            }
        }

        /// <summary>Gerekli tüm sunucu verilerini başlatır.</summary>
        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            };
            Console.WriteLine("Paketler başlatıldı.");
        }
    }
}
