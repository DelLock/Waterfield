using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Battleship
{
    public class GameLobbyHost
    {
        private Thread _advertisementThread;
        private bool _isAdvertising = false;
        private string _hostName;
        private string _hostIp;
        private UdpClient _udpServer;
        private DateTime _lastAdvertisementTime;

        public void StartAdvertising(string hostName, string hostIp)
        {
            if (_isAdvertising) return;

            _hostName = hostName;
            _hostIp = hostIp;
            _isAdvertising = true;
            _lastAdvertisementTime = DateTime.Now;

            _advertisementThread = new Thread(AdvertisementWorker);
            _advertisementThread.IsBackground = true;
            _advertisementThread.Start();
        }

        public void StopAdvertising()
        {
            _isAdvertising = false;
            try
            {
                _udpServer?.Close();
                _advertisementThread?.Join(1000);
            }
            catch { }
        }

        private void AdvertisementWorker()
        {
            try
            {
                _udpServer = new UdpClient(5002);
                _udpServer.Client.ReceiveTimeout = 100;
                _udpServer.EnableBroadcast = true;

                byte[] responseData = Encoding.UTF8.GetBytes($"BATTLESHIP_HOST:{_hostName}|{_hostIp}|Ожидание игрока");

                IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 5002);

                while (_isAdvertising)
                {
                    try
                    {
                        // Отправляем широковещательное сообщение каждые 2 секунды
                        if ((DateTime.Now - _lastAdvertisementTime).TotalSeconds > 2)
                        {
                            _udpServer.Send(responseData, responseData.Length, broadcastEndPoint);
                            _lastAdvertisementTime = DateTime.Now;
                        }

                        // Проверяем входящие запросы
                        if (_udpServer.Available > 0)
                        {
                            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                            byte[] requestData = _udpServer.Receive(ref clientEndPoint);
                            string request = Encoding.UTF8.GetString(requestData);

                            if (request == "BATTLESHIP_DISCOVER")
                            {
                                _udpServer.Send(responseData, responseData.Length, clientEndPoint);
                                _lastAdvertisementTime = DateTime.Now;
                            }
                        }
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        Thread.Sleep(50);
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки
            }
            finally
            {
                try { _udpServer?.Close(); } catch { }
            }
        }
    }
}