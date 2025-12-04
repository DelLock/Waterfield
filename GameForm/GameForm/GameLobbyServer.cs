using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Battleship
{
    public class GameLobby
    {
        public string HostName { get; set; }
        public string HostIP { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{HostName} ({Players}/{MaxPlayers}) - {Status}";
        }
    }

    public static class GameLobbyServer
    {
        private static List<GameLobby> _activeLobbies = new List<GameLobby>();
        private static TcpListener _listener;
        private static Thread _serverThread;
        private static bool _isRunning = false;

        public static event Action<List<GameLobby>> OnLobbiesUpdated;

        public static void StartServer()
        {
            if (_isRunning) return;

            _isRunning = true;
            _serverThread = new Thread(RunServer);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        public static void StopServer()
        {
            _isRunning = false;
            _listener?.Stop();
        }

        private static void RunServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, 5001);
                _listener.Start();

                while (_isRunning)
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        Thread clientThread = new Thread(() => HandleClient(client));
                        clientThread.IsBackground = true;
                        clientThread.Start();
                    }
                    Thread.Sleep(100);
                }
            }
            catch { }
        }

        private static void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (message.StartsWith("REGISTER:"))
                {
                    string[] parts = message.Substring(9).Split('|');
                    var lobby = new GameLobby
                    {
                        HostName = parts[0],
                        HostIP = parts[1],
                        Players = 1,
                        MaxPlayers = 2,
                        Status = "Ожидание игрока"
                    };

                    _activeLobbies.Add(lobby);
                    NotifyLobbiesUpdated();
                }
                else if (message.StartsWith("UNREGISTER:"))
                {
                    string hostIP = message.Substring(11);
                    _activeLobbies.RemoveAll(l => l.HostIP == hostIP);
                    NotifyLobbiesUpdated();
                }
                else if (message == "GET_LOBBIES")
                {
                    string lobbiesData = SerializeLobbies();
                    byte[] data = Encoding.UTF8.GetBytes(lobbiesData);
                    stream.Write(data, 0, data.Length);
                }

                client.Close();
            }
            catch { }
        }

        private static string SerializeLobbies()
        {
            var result = new StringBuilder();
            foreach (var lobby in _activeLobbies)
            {
                result.AppendLine($"{lobby.HostName}|{lobby.HostIP}|{lobby.Players}|{lobby.MaxPlayers}|{lobby.Status}");
            }
            return result.ToString();
        }

        private static void NotifyLobbiesUpdated()
        {
            OnLobbiesUpdated?.Invoke(new List<GameLobby>(_activeLobbies));
        }

        public static void RegisterLobby(string hostName, string hostIP)
        {
            var lobby = new GameLobby
            {
                HostName = hostName,
                HostIP = hostIP,
                Players = 1,
                MaxPlayers = 2,
                Status = "Ожидание игрока"
            };

            _activeLobbies.Add(lobby);
            NotifyLobbiesUpdated();
            SendToServer($"REGISTER:{hostName}|{hostIP}");
        }

        public static void UnregisterLobby(string hostIP)
        {
            _activeLobbies.RemoveAll(l => l.HostIP == hostIP);
            NotifyLobbiesUpdated();
            SendToServer($"UNREGISTER:{hostIP}");
        }

        public static List<GameLobby> GetLobbies()
        {
            return new List<GameLobby>(_activeLobbies);
        }

        private static void SendToServer(string message)
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 5001))
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    client.GetStream().Write(data, 0, data.Length);
                }
            }
            catch { }
        }
    }
}