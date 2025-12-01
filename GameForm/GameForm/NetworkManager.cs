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

        public event Action<string> OnMessageReceived;

        public NetworkManager(string ip, bool isHost)
        {
            try
            {
                if (isHost)
                {
                    // Хост создает сервер
                    _server = new TcpListener(IPAddress.Any, 5000);
                    _server.Start();
                    Console.WriteLine("Хост: Ожидаем подключения клиента...");

                    // Асинхронно ждем подключения
                    _client = _server.AcceptTcpClient();
                    Console.WriteLine("Хост: Клиент подключен!");
                }
                else
                {
                    // Клиент подключается к хосту
                    Console.WriteLine($"Клиент: Подключаемся к {ip}:5000");
                    _client = new TcpClient();
                    _client.Connect(ip, 5000);
                    Console.WriteLine("Клиент: Подключение установлено!");
                }

                _stream = _client.GetStream();
                _isConnected = true;
                StartListening();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                Disconnect();
            }
        }

        private void StartListening()
        {
            _listenThread = new Thread(() =>
            {
                byte[] buffer = new byte[1024];
                while (_isConnected && _client != null && _client.Connected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("Соединение разорвано");
                            break;
                        }

                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Получено сообщение: {message}");
                        OnMessageReceived?.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка чтения: {ex.Message}");
                        break;
                    }
                }
                Disconnect();
            });
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        public void SendMessage(string message)
        {
            if (!_isConnected || _client == null || !_client.Connected)
            {
                Console.WriteLine($"Не удалось отправить сообщение: {message} - нет соединения");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                Console.WriteLine($"Отправлено сообщение: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки: {ex.Message}");
                Disconnect();
            }
        }

        public void SendMove(int x, int y)
        {
            SendMessage($"MOVE:{x},{y}");
        }

        public void SendResult(bool isHit)
        {
            SendMessage($"RESULT:{isHit}");
        }

        public void Disconnect()
        {
            try
            {
                _isConnected = false;
                _stream?.Close();
                _client?.Close();
                _server?.Stop();
                Console.WriteLine("Соединение разорвано");
            }
            catch { }
        }
    }
}
