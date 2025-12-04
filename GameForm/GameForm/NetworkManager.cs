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
        private bool _isConnected = false;
        private bool _isListening = false;

        public event Action<string> OnMessageReceived;

        public NetworkManager(string ip, bool isHost, TcpClient tcpClient = null)
        {
            try
            {
                if (isHost)
                {
                    if (tcpClient != null)
                    {
                        _client = tcpClient;
                        _isConnected = true;
                        _stream = _client.GetStream();
                    }
                    else
                    {
                        _server = new TcpListener(IPAddress.Any, 5000);
                        _server.Start();
                        _client = _server.AcceptTcpClient();
                        _isConnected = true;
                        _stream = _client.GetStream();
                    }
                }
                else
                {
                    _client = new TcpClient();
                    _client.Connect(ip, 5000);
                    _isConnected = true;
                    _stream = _client.GetStream();
                }

                StartListening();
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Disconnect();
                throw new Exception($"Ошибка подключения: {ex.Message}");
            }
        }

        private void StartListening()
        {
            if (!_isConnected) return;

            _listenThread = new Thread(() =>
            {
                _isListening = true;
                try
                {
                    byte[] buffer = new byte[1024];
                    while (_isConnected && _client != null && _client.Connected && _isListening)
                    {
                        try
                        {
                            if (_stream.DataAvailable)
                            {
                                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                    OnMessageReceived?.Invoke(message);
                                }
                            }
                            else
                            {
                                Thread.Sleep(50);
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    Disconnect();
                }
            });

            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        public bool SendMove(int x, int y) => SendMessageInternal($"MOVE:{x},{y}");
        public bool SendResult(bool isHit) => SendMessageInternal($"RESULT:{isHit}");
        public bool SendMessage(string message) => SendMessageInternal(message);

        private bool SendMessageInternal(string message)
        {
            if (!_isConnected || _client == null || !_client.Connected) return false;
            try
            {
                if (_stream == null || !_stream.CanWrite) return false;
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _isListening = false;
                _isConnected = false;
                _stream?.Close();
                _client?.Close();
                _server?.Stop();
            }
            catch { }
        }

        public bool IsConnected => _isConnected && _client != null && _client.Connected;
    }
}