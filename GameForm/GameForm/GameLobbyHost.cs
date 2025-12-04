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

        public void StartAdvertising(string hostName, string hostIp)
        {
            if (_isAdvertising) return;

            _hostName = hostName;
            _hostIp = hostIp;
            _isAdvertising = true;

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
                _udpServer.Client.ReceiveTimeout = 1000;

                while (_isAdvertising)
                {
                    try
                    {
                        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] requestData = _udpServer.Receive(ref clientEndPoint);
                        string request = Encoding.UTF8.GetString(requestData);

                        if (request == "BATTLESHIP_DISCOVER")
                        {
                            string response = $"BATTLESHIP_HOST:{_hostName}|{_hostIp}|Ожидание игрока";
                            byte[] responseData = Encoding.UTF8.GetBytes(response);
                            _udpServer.Send(responseData, responseData.Length, clientEndPoint);
                        }
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        Thread.Sleep(100);
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                try { _udpServer?.Close(); } catch { }
            }
        }
    }
}