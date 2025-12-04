using Battleship;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class GameLobbyClient
{
    public event Action<List<GameLobby>> OnLobbiesUpdated;
    private UdpClient _udpClient;
    private Thread _discoveryThread;
    private bool _isRunning = false;
    private Dictionary<string, DateTime> _lobbyLastSeen = new Dictionary<string, DateTime>();

    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        try
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
            _udpClient.Client.ReceiveTimeout = 1000;

            _discoveryThread = new Thread(DiscoveryWorker);
            _discoveryThread.IsBackground = true;
            _discoveryThread.Start();
        }
        catch
        {
            _isRunning = false;
        }
    }

    private void DiscoveryWorker()
    {
        while (_isRunning)
        {
            try
            {
                DiscoverLobbies();
                Thread.Sleep(2000);
            }
            catch
            {
                Thread.Sleep(1000);
            }
        }
    }

    public void DiscoverLobbiesAsync()
    {
        ThreadPool.QueueUserWorkItem(state =>
        {
            try
            {
                var lobbies = DiscoverLobbies();
                OnLobbiesUpdated?.Invoke(lobbies);
            }
            catch { }
        });
    }

    public List<GameLobby> DiscoverLobbies()
    {
        var lobbies = new List<GameLobby>();

        try
        {
            byte[] broadcastData = Encoding.UTF8.GetBytes("BATTLESHIP_DISCOVER");
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 5002);

            if (_udpClient != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        _udpClient.Send(broadcastData, broadcastData.Length, broadcastEndPoint);
                    }
                    catch { }
                }

                byte[] buffer = new byte[1024];
                DateTime timeout = DateTime.Now.AddSeconds(1);

                while (DateTime.Now < timeout)
                {
                    try
                    {
                        if (_udpClient.Available > 0)
                        {
                            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                            buffer = _udpClient.Receive(ref sender);
                            string response = Encoding.UTF8.GetString(buffer).Trim();

                            if (response.StartsWith("BATTLESHIP_HOST:"))
                            {
                                string[] parts = response.Substring(16).Split('|');
                                if (parts.Length >= 3)
                                {
                                    var lobby = new GameLobby
                                    {
                                        HostName = parts[0],
                                        HostIP = parts[1],
                                        Status = parts[2],
                                        Players = 1,
                                        MaxPlayers = 2
                                    };

                                    var existing = lobbies.Find(l => l.HostIP == lobby.HostIP);
                                    if (existing == null)
                                    {
                                        lobbies.Add(lobby);
                                    }

                                    _lobbyLastSeen[lobby.HostIP] = DateTime.Now;
                                }
                            }
                        }
                    }
                    catch { }

                    Thread.Sleep(10);
                }

                CleanExpiredLobbies(lobbies);
            }
        }
        catch
        {
            return new List<GameLobby>();
        }

        return lobbies;
    }

    private void CleanExpiredLobbies(List<GameLobby> lobbies)
    {
        var expiredIPs = new List<string>();
        foreach (var kvp in _lobbyLastSeen)
        {
            if ((DateTime.Now - kvp.Value).TotalSeconds > 10)
            {
                expiredIPs.Add(kvp.Key);
            }
        }

        foreach (var ip in expiredIPs)
        {
            _lobbyLastSeen.Remove(ip);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        try
        {
            _udpClient?.Close();
            _discoveryThread?.Join(1000);
        }
        catch { }
    }
}