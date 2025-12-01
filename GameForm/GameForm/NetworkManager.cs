
﻿using System;
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

        public NetworkManager(string ip, bool isHost)
        {
            try
            {
                if (isHost)
                {
                    Console.WriteLine("Хост: Пытаюсь запустить сервер...");

                    // ✅ Пробуем создать сервер
                    _server = new TcpListener(IPAddress.Any, 5000);
                    _server.Start();

                    Console.WriteLine("Хост: Сервер запущен, жду подключения...");

                    // ✅ Ждем подключение клиента (без таймаута, чтобы не выбрасывать исключение)
                    _client = _server.AcceptTcpClient();

                    Console.WriteLine("Хост: Клиент подключен успешно!");
                }
                else
                {
                    Console.WriteLine($"Клиент: Пытаюсь подключиться к {ip}:5000...");

                    // ✅ Пробуем подключиться к хосту
                    _client = new TcpClient();
                    _client.Connect(ip, 5000);

                    Console.WriteLine("Клиент: Подключение установлено успешно!");
                }

                _stream = _client.GetStream();
                _isConnected = true;

                Console.WriteLine($"Сетевое соединение установлено: isHost={isHost}");

                StartListening();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в NetworkManager: {ex.Message}");
                _isConnected = false;

                // ✅ НЕ выбрасываем исключение наружу, просто отмечаем что не подключились
                Disconnect();
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
                                    Console.WriteLine($"Получено сообщение: {message}");
                                    OnMessageReceived?.Invoke(message);
                                }
                            }
                            else
                            {
                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка чтения из потока: {ex.Message}");
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

        public bool SendMove(int x, int y)
        {
            return SendMessage($"MOVE:{x},{y}");
        }

        public bool SendResult(bool isHit)
        {
            return SendMessage($"RESULT:{isHit}");
        }

        private bool SendMessage(string message)
        {
            if (!_isConnected || _client == null || !_client.Connected)
            {
                Console.WriteLine($"Нет соединения для отправки: {message}");
                return false;
            }

            try
            {
                if (_stream == null || !_stream.CanWrite)
                {
                    Console.WriteLine($"Поток не доступен для записи: {message}");
                    return false;
                }

                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                Console.WriteLine($"Сообщение отправлено: {message}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки: {ex.Message}");
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

                Console.WriteLine("Соединение закрыто");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при разрыве соединения: {ex.Message}");
            }
        }

        public bool IsConnected => _isConnected && _client != null && _client.Connected;
    }
}