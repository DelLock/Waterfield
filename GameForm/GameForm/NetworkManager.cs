using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Battleship
{
    public class NetworkManager
    {
        private TcpClient _client;
        private TcpListener _server;
        private NetworkStream _stream;
        private Thread _listenThread;

        public event Action<string> OnMessageReceived;

        public NetworkManager(string ip, bool isHost)
        {
            if (isHost)
            {
                _server = new TcpListener(IPAddress.Any, 5000);
                _server.Start();
                _client = _server.AcceptTcpClient();
            }
            else
            {
                _client = new TcpClient();
                _client.Connect(ip, 5000);
            }
            _stream = _client.GetStream();
            StartListening();
        }

        private void StartListening()
        {
            _listenThread = new Thread(() =>
            {
                byte[] buffer = new byte[1024];
                while (_client?.Connected == true)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnMessageReceived?.Invoke(message);
                    }
                    catch { break; }
                }
                Disconnect();
            });
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        public void SendMove(int x, int y)
        {
            if (!_client?.Connected == true) return;
            string message = $"MOVE:{x},{y}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        public void SendResult(bool isHit)
        {
            if (!_client?.Connected == true) return;
            string message = $"RESULT:{isHit}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        // ✅ ТОЛЬКО ОДИН Disconnect
        public void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
                _server?.Stop();
            }
            catch { }
            // Очищаем ссылки
            _stream = null;
            _client = null;
            _server = null;
        }
    }
}