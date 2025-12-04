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
                Text = "Доступные комнаты:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 80),
                AutoSize = true
            };

            lstLobbies = new ListBox
            {
                Location = new Point(20, 110),
                Size = new Size(560, 150),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstLobbies.DoubleClick += (s, e) =>
            {
                if (!_isConnecting && lstLobbies.SelectedItem != null)
                {
                    string selected = lstLobbies.SelectedItem.ToString();
                    int start = selected.IndexOf('(') + 1;
                    int end = selected.IndexOf(')');
                    if (start > 0 && end > start)
                    {
                        string ipInfo = selected.Substring(start, end - start);
                        string[] parts = ipInfo.Split(' ');
                        if (parts.Length > 0)
                        {
                            txtIp.Text = parts[0];
                            StartAsClient();
                        }
                    }
                }
            };

            btnRefresh = new RoundedButton
            {
                Text = "🔄 Обновить список",
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
                if (!_isConnecting) RefreshLobbies();
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
                BackColor = Color.White
            };

            lblStatus = new Label
            {
                Text = "Готов к подключению...",
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
                Text = "🔗 Присоединиться",
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
                if (!_isConnecting) StartAsClient();
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
            RefreshLobbies();
        }

        private void RefreshLobbies()
        {
            if (_isConnecting) return;

            lstLobbies.Items.Clear();
            lstLobbies.Items.Add("Поиск доступных комнат...");

            try
            {
                var lobbies = _lobbyClient.DiscoverLobbies();
                UpdateLobbiesList(lobbies);
            }
            catch
            {
                lstLobbies.Items.Clear();
                lstLobbies.Items.Add("Не удалось найти комнаты");
            }
        }

        private void UpdateLobbiesList(List<GameLobby> lobbies)
        {
            if (lstLobbies.InvokeRequired)
            {
                lstLobbies.Invoke(new Action(() => UpdateLobbiesList(lobbies)));
                return;
            }

            if (_isConnecting) return;

            lstLobbies.Items.Clear();

            if (lobbies.Count == 0)
            {
                lstLobbies.Items.Add("Нет доступных комнат");
            }
            else
            {
                foreach (var lobby in lobbies)
                {
                    lstLobbies.Items.Add(lobby.ToString());
                }
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
                        if (lobby.DialogResult != DialogResult.OK)
                        {
                            _isConnecting = false;
                            this.Enabled = true;
                            lblStatus.Text = "Создание игры отменено";
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
                                RefreshLobbies();
                            };
                            this.Hide();
                            gameForm.Show();
                        }
                        else
                        {
                            _isConnecting = false;
                            lblStatus.Text = "❌ Ошибка: Не удалось установить соединение";
                            this.Enabled = true;
                        }
                    }
                    else
                    {
                        _isConnecting = false;
                        this.Enabled = true;
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
            }
        }

        private void StartAsClient()
        {
            if (_isConnecting) return;
            _isConnecting = true;

            try
            {
                if (string.IsNullOrWhiteSpace(txtIp.Text))
                {
                    _isConnecting = false;
                    MessageBox.Show("Введите IP-адрес хоста", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                lblStatus.Text = "Подключение к хосту...";
                this.Enabled = false;
                Application.DoEvents();

                NetworkManager network = null;
                bool connected = false;

                var connectTask = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        network = new NetworkManager(txtIp.Text, isHost: false);
                        connected = network.IsConnected;
                    }
                    catch
                    {
                        connected = false;
                    }
                });

                if (connectTask.Wait(TimeSpan.FromSeconds(10)))
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
                            RefreshLobbies();
                        };
                        this.Hide();
                        gameForm.Show();
                    }
                    else
                    {
                        _isConnecting = false;
                        lblStatus.Text = "❌ Не удалось подключиться";
                        this.Enabled = true;
                        MessageBox.Show("Не удалось установить соединение с хостом. Проверьте IP-адрес и убедитесь, что хост запущен.",
                            "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    _isConnecting = false;
                    lblStatus.Text = "❌ Таймаут подключения";
                    this.Enabled = true;
                    MessageBox.Show("Таймаут подключения. Хост не отвечает.",
                        "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _isConnecting = false;
                lblStatus.Text = $"❌ Ошибка: {ex.Message}";
                MessageBox.Show($"Не удалось подключиться: {ex.Message}\n\nПроверьте:\n1. Правильность IP-адреса\n2. Запущен ли хост\n3. Брандмауэр не блокирует порт 5000",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = true;
            }
        }

        public static string GetLocalIPAddress()
        {
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address.ToString() ?? "127.0.0.1";
                }
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
                return;
            }

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

        public void StartDiscovery()
        {
            if (_isRunning) return;
            _isRunning = true;

            try
            {
                _udpClient = new UdpClient();
                _udpClient.EnableBroadcast = true;
                _udpClient.Client.ReceiveTimeout = 2000;

                _discoveryThread = new Thread(() =>
                {
                    while (_isRunning)
                    {
                        try
                        {
                            DiscoverLobbies();
                            Thread.Sleep(5000);
                        }
                        catch
                        {
                            Thread.Sleep(1000);
                        }
                    }
                });
                _discoveryThread.IsBackground = true;
                _discoveryThread.Start();
            }
            catch
            {
                _isRunning = false;
            }
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
                    _udpClient.Send(broadcastData, broadcastData.Length, broadcastEndPoint);

                    byte[] buffer = new byte[1024];
                    DateTime timeout = DateTime.Now.AddSeconds(2);

                    while (DateTime.Now < timeout)
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

                                    if (!lobbies.Exists(l => l.HostIP == lobby.HostIP))
                                    {
                                        lobbies.Add(lobby);
                                    }
                                }
                            }
                        }
                        Thread.Sleep(50);
                    }
                }
            }
            catch
            {
                return new List<GameLobby>();
            }

            OnLobbiesUpdated?.Invoke(lobbies);
            return lobbies;
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