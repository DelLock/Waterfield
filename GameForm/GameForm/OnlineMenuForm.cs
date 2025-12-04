using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Battleship
{
    public class OnlineMenuForm : Form
    {
        private TextBox txtIp;
        private Label lblStatus;
        private ListBox lstLobbies;
        private Button btnRefresh;
        private GameLobbyClient _lobbyClient;
        private bool _isConnecting = false;
        private System.Windows.Forms.Timer _refreshTimer;
        private string _lastSelectedIp;

        public OnlineMenuForm()
        {
            this.Text = "Онлайн-мультиплеер";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 248, 255);

            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            Label titleLabel = new Label
            {
                Text = "🌐 Сетевая игра",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Size = new Size(560, 40),
                Location = new Point(20, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblLobbies = new Label
            {
                Text = "Доступные комнаты (автообновление каждые 3 сек):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 80),
                AutoSize = true
            };

            lstLobbies = new ListBox
            {
                Location = new Point(20, 110),
                Size = new Size(560, 150),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstLobbies.SelectedIndexChanged += (s, e) =>
            {
                if (lstLobbies.SelectedItem is GameLobby selectedLobby)
                {
                    _lastSelectedIp = selectedLobby.HostIP;
                    txtIp.Text = selectedLobby.HostIP;
                }
            };

            lstLobbies.DoubleClick += (s, e) =>
            {
                if (!_isConnecting && lstLobbies.SelectedItem is GameLobby selectedLobby)
                {
                    StartAsClient(selectedLobby.HostIP);
                }
            };

            btnRefresh = new RoundedButton
            {
                Text = "🔄 Обновить сейчас",
                Location = new Point(20, 270),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(30, 144, 255),
                BorderColor = Color.FromArgb(65, 105, 225),
                BorderRadius = 10,
                BorderSize = 2,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(0, 191, 255)
            };
            btnRefresh.Click += (s, e) =>
            {
                if (!_isConnecting) ForceRefreshLobbies();
            };

            Label lblManual = new Label
            {
                Text = "Или введите IP-адрес вручную:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 320),
                AutoSize = true
            };

            txtIp = new TextBox
            {
                Location = new Point(20, 350),
                Size = new Size(560, 30),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Text = "192.168."
            };
            txtIp.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter && !_isConnecting)
                {
                    StartAsClient();
                }
            };

            lblStatus = new Label
            {
                Text = "Поиск доступных игр в сети...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 385),
                Size = new Size(560, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            RoundedButton btnHost = new RoundedButton
            {
                Text = "🏠 Создать игру",
                Location = new Point(20, 410),
                Size = new Size(275, 45),
                BackColor = Color.FromArgb(30, 144, 255),
                BorderColor = Color.FromArgb(65, 105, 225),
                BorderRadius = 15,
                BorderSize = 2,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(0, 191, 255)
            };
            btnHost.Click += (s, e) =>
            {
                if (!_isConnecting) StartAsHost();
            };

            RoundedButton btnJoin = new RoundedButton
            {
                Text = "🔗 Присоединиться к выбранной",
                Location = new Point(305, 410),
                Size = new Size(275, 45),
                BackColor = Color.FromArgb(46, 139, 87),
                BorderColor = Color.FromArgb(34, 139, 34),
                BorderRadius = 15,
                BorderSize = 2,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(60, 179, 113)
            };
            btnJoin.Click += (s, e) =>
            {
                if (!_isConnecting)
                {
                    if (lstLobbies.SelectedItem is GameLobby selectedLobby)
                    {
                        StartAsClient(selectedLobby.HostIP);
                    }
                    else if (!string.IsNullOrWhiteSpace(txtIp.Text))
                    {
                        StartAsClient();
                    }
                    else
                    {
                        MessageBox.Show("Выберите комнату из списка или введите IP-адрес вручную", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            RoundedButton btnBack = new RoundedButton
            {
                Text = "← Назад",
                Location = new Point(20, 465),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(192, 192, 192),
                BorderColor = Color.FromArgb(169, 169, 169),
                BorderRadius = 10,
                BorderSize = 1,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(64, 64, 64),
                HoverColor = Color.FromArgb(211, 211, 211)
            };
            btnBack.Click += (s, e) =>
            {
                if (!_isConnecting) this.Close();
            };

            contentPanel.Controls.Add(titleLabel);
            contentPanel.Controls.Add(lblLobbies);
            contentPanel.Controls.Add(lstLobbies);
            contentPanel.Controls.Add(btnRefresh);
            contentPanel.Controls.Add(lblManual);
            contentPanel.Controls.Add(txtIp);
            contentPanel.Controls.Add(lblStatus);
            contentPanel.Controls.Add(btnHost);
            contentPanel.Controls.Add(btnJoin);
            contentPanel.Controls.Add(btnBack);

            this.Controls.Add(contentPanel);

            txtIp.Text = GetLocalIPAddress();
            _lobbyClient = new GameLobbyClient();
            _lobbyClient.OnLobbiesUpdated += UpdateLobbiesList;
            _lobbyClient.Start();

            SetupAutoRefresh();
            ForceRefreshLobbies();
        }

        private void SetupAutoRefresh()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 3000;
            _refreshTimer.Tick += (s, e) =>
            {
                if (!_isConnecting)
                {
                    RefreshLobbiesSilent();
                }
            };
            _refreshTimer.Start();
        }

        private void ForceRefreshLobbies()
        {
            if (_isConnecting) return;

            lblStatus.Text = "Поиск игр в сети...";
            lstLobbies.Items.Clear();
            lstLobbies.Items.Add("Идет поиск...");

            try
            {
                _lobbyClient.DiscoverLobbiesAsync();
            }
            catch
            {
                UpdateLobbiesList(new List<GameLobby>());
            }
        }

        private void RefreshLobbiesSilent()
        {
            if (_isConnecting) return;

            try
            {
                _lobbyClient.DiscoverLobbiesAsync();
            }
            catch { }
        }

        private void UpdateLobbiesList(List<GameLobby> lobbies)
        {
            if (lstLobbies.InvokeRequired)
            {
                lstLobbies.Invoke(new Action(() => UpdateLobbiesList(lobbies)));
                return;
            }

            if (_isConnecting) return;

            string selectedIp = _lastSelectedIp;

            lstLobbies.Items.Clear();

            if (lobbies.Count == 0)
            {
                lstLobbies.Items.Add("Нет доступных комнат");
                lblStatus.Text = "Поиск завершен. Комнат не найдено.";
            }
            else
            {
                int selectedIndex = -1;
                for (int i = 0; i < lobbies.Count; i++)
                {
                    lstLobbies.Items.Add(lobbies[i]);
                    if (lobbies[i].HostIP == selectedIp)
                    {
                        selectedIndex = i;
                    }
                }

                if (selectedIndex >= 0)
                {
                    lstLobbies.SelectedIndex = selectedIndex;
                }

                lblStatus.Text = $"Найдено {lobbies.Count} комнат(а). Дважды кликните для подключения.";
            }
        }

        private void StartAsHost()
        {
            if (_isConnecting) return;
            _isConnecting = true;

            try
            {
                lblStatus.Text = "Создание игры...";
                this.Enabled = false;
                Application.DoEvents();

                string localIp = GetLocalIPAddress();

                using (var lobby = new LobbyForm(localIp))
                {
                    lobby.FormClosed += (s, e) =>
                    {
                        _isConnecting = false;
                        this.Enabled = true;

                        if (lobby.DialogResult == DialogResult.OK)
                        {
                            this.Hide();
                        }
                        else
                        {
                            lblStatus.Text = "Создание игры отменено";
                            ForceRefreshLobbies();
                        }
                    };

                    var result = lobby.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        var network = lobby.GetNetwork();
                        if (network != null && network.IsConnected)
                        {
                            lblStatus.Text = "✅ Игра создана! Запуск...";
                            Application.DoEvents();

                            var gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: null, isHost: true, network: network);
                            gameForm.FormClosed += (s2, e2) =>
                            {
                                _isConnecting = false;
                                this.Show();
                                this.Enabled = true;
                                lblStatus.Text = "Готов к подключению...";
                                ForceRefreshLobbies();
                            };
                            this.Hide();
                            gameForm.Show();
                        }
                        else
                        {
                            _isConnecting = false;
                            lblStatus.Text = "❌ Не удалось установить соединение";
                            this.Enabled = true;
                            ForceRefreshLobbies();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _isConnecting = false;
                lblStatus.Text = $"❌ Ошибка: {ex.Message}";
                MessageBox.Show($"Не удалось создать игру: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = true;
                ForceRefreshLobbies();
            }
        }

        private void StartAsClient(string ipAddress = null)
        {
            if (_isConnecting) return;
            _isConnecting = true;

            try
            {
                string targetIp = ipAddress ?? txtIp.Text.Trim();

                if (string.IsNullOrWhiteSpace(targetIp))
                {
                    _isConnecting = false;
                    MessageBox.Show("Введите IP-адрес хоста", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!IsValidIP(targetIp))
                {
                    _isConnecting = false;
                    MessageBox.Show("Введите корректный IP-адрес (например: 192.168.1.100)", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                lblStatus.Text = $"Подключение к {targetIp}...";
                this.Enabled = false;
                Application.DoEvents();

                NetworkManager network = null;
                bool connected = false;

                var connectTask = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        network = new NetworkManager(targetIp, isHost: false);
                        connected = network.IsConnected;
                    }
                    catch (Exception ex)
                    {
                        connected = false;
                    }
                });

                if (connectTask.Wait(TimeSpan.FromSeconds(5)))
                {
                    if (connected && network != null && network.IsConnected)
                    {
                        lblStatus.Text = "✅ Подключено! Запуск игры...";
                        Application.DoEvents();

                        var gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: null, isHost: false, network: network);
                        gameForm.FormClosed += (s, e) =>
                        {
                            _isConnecting = false;
                            this.Show();
                            this.Enabled = true;
                            lblStatus.Text = "Готов к подключению...";
                            ForceRefreshLobbies();
                        };
                        this.Hide();
                        gameForm.Show();
                    }
                    else
                    {
                        _isConnecting = false;
                        lblStatus.Text = "❌ Не удалось подключиться";
                        this.Enabled = true;
                        MessageBox.Show("Не удалось установить соединение с хостом. Возможно:\n1. Игра уже началась или закончилась\n2. Хост отключился\n3. Неправильный IP-адрес\n4. Брандмауэр блокирует подключение",
                            "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ForceRefreshLobbies();
                    }
                }
                else
                {
                    _isConnecting = false;
                    lblStatus.Text = "❌ Таймаут подключения";
                    this.Enabled = true;
                    MessageBox.Show("Хост не отвечает. Проверьте:\n1. Хост запущен и ожидает подключения\n2. Правильность IP-адреса\n3. Оба компьютера в одной сети\n4. Брандмауэр не блокирует порт 5000",
                        "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ForceRefreshLobbies();
                }
            }
            catch (Exception ex)
            {
                _isConnecting = false;
                lblStatus.Text = $"❌ Ошибка: {ex.Message}";
                MessageBox.Show($"Не удалось подключиться: {ex.Message}",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = true;
                ForceRefreshLobbies();
            }
        }

        private bool IsValidIP(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;

            if (ip == "localhost" || ip == "127.0.0.1") return true;

            string[] parts = ip.Split('.');
            if (parts.Length != 4) return false;

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }

            return true;
        }

        public static string GetLocalIPAddress()
        {
            try
            {
                string localIP = "";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        if (localIP.StartsWith("192.168.") || localIP.StartsWith("10.") || localIP.StartsWith("172."))
                        {
                            return localIP;
                        }
                    }
                }

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isConnecting)
            {
                e.Cancel = true;
                MessageBox.Show("Пожалуйста, дождитесь завершения подключения", "Подключение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _refreshTimer?.Stop();
            _lobbyClient?.Stop();
            base.OnFormClosing(e);
        }
    }

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
                return lobbies;
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
                lobbies.RemoveAll(l => l.HostIP == ip);
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
}