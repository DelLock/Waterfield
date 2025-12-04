using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

namespace Battleship
{
    public partial class LobbyForm : Form
    {
        private NetworkManager _network;
        private bool _isReady = false;
        private GameLobbyHost _lobbyHost;
        private string _localIp;
        private bool _connectionAttempted = false;
        private Label _lblConnectionStatus;
        private TcpListener _tcpListener;
        private Thread _serverThread;
        private bool _isServerRunning = false;
        private System.Windows.Forms.Timer _statusTimer;

        public LobbyForm(string localIp)
        {
            _localIp = localIp;
            this.Text = "Комната ожидания";
            this.Size = new Size(500, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 248, 255);

            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            Label lblTitle = new Label
            {
                Text = "🕐 Ожидание подключения",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 30),
                Size = new Size(460, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblInfo = new Label
            {
                Text = "Ожидание второго игрока...\nДругие игроки увидят эту комнату в списке",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 80),
                Size = new Size(460, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblIp = new Label
            {
                Text = $"Ваш IP для подключения:\n{_localIp}",
                Font = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 140),
                Size = new Size(460, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(220, 240, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblConnectionStatus = new Label
            {
                Text = "Запуск сервера...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 210),
                Size = new Size(460, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblInstructions = new Label
            {
                Text = "Сообщите ваш IP другому игроку\nили подождите автоматического подключения",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(20, 270),
                Size = new Size(460, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            RoundedButton btnCancel = new RoundedButton
            {
                Text = "✖ Отмена",
                Location = new Point(200, 310),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(220, 20, 60),
                BorderColor = Color.FromArgb(178, 34, 34),
                BorderRadius = 10,
                BorderSize = 1,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(255, 69, 0)
            };
            btnCancel.Click += (s, e) => CancelLobby();

            contentPanel.Controls.Add(lblTitle);
            contentPanel.Controls.Add(lblInfo);
            contentPanel.Controls.Add(lblIp);
            contentPanel.Controls.Add(_lblConnectionStatus);
            contentPanel.Controls.Add(lblInstructions);
            contentPanel.Controls.Add(btnCancel);

            this.Controls.Add(contentPanel);

            StartAdvertising();
            StartHostServer();
            StartStatusTimer();
        }

        private void StartAdvertising()
        {
            try
            {
                _lobbyHost = new GameLobbyHost();
                _lobbyHost.StartAdvertising(Program.CurrentPlayer.Nickname, _localIp);
                UpdateStatus("✅ Комната видна в сети\nIP: " + _localIp);
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Ошибка рекламы: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            if (_lblConnectionStatus.InvokeRequired)
            {
                _lblConnectionStatus.Invoke(new Action(() => UpdateStatus(message)));
            }
            else
            {
                _lblConnectionStatus.Text = message;
            }
        }

        private void StartStatusTimer()
        {
            _statusTimer = new System.Windows.Forms.Timer();
            _statusTimer.Interval = 1000;
            int counter = 0;

            _statusTimer.Tick += (s, e) =>
            {
                counter++;
                string dots = new string('.', (counter % 4));

                if (!_isReady)
                {
                    UpdateStatus($"✅ Комната активна {dots}\nIP: {_localIp}\nПорт: 5000");
                }
            };

            _statusTimer.Start();
        }

        private void StartHostServer()
        {
            _isServerRunning = true;

            _serverThread = new Thread(() =>
            {
                try
                {
                    UpdateStatus("⏳ Запуск игрового сервера...");

                    _tcpListener = new TcpListener(IPAddress.Any, 5000);
                    _tcpListener.Start();

                    UpdateStatus($"✅ Сервер запущен!\nIP: {_localIp}\nПорт: 5000\nОжидание подключения...");
                    _connectionAttempted = true;

                    while (_isServerRunning && !_isReady)
                    {
                        try
                        {
                            if (_tcpListener.Pending())
                            {
                                TcpClient client = _tcpListener.AcceptTcpClient();

                                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                                UpdateStatus($"✅ Подключение от {clientIP}\nСоздание игровой сессии...");

                                _network = new NetworkManager("0.0.0.0", isHost: true, tcpClient: client);

                                if (_network != null && _network.IsConnected)
                                {
                                    _isReady = true;
                                    this.Invoke(new Action(() =>
                                    {
                                        if (!this.IsDisposed)
                                        {
                                            this.DialogResult = DialogResult.OK;
                                            this.Close();
                                        }
                                    }));
                                    break;
                                }
                                else
                                {
                                    UpdateStatus("❌ Ошибка создания сессии\nПовторное ожидание...");
                                }
                            }
                            Thread.Sleep(100);
                        }
                        catch (SocketException)
                        {
                            Thread.Sleep(100);
                        }
                        catch (Exception ex)
                        {
                            UpdateStatus($"❌ Ошибка: {ex.Message}");
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _connectionAttempted = true;
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            MessageBox.Show($"❌ Ошибка создания сервера: {ex.Message}\n\nВозможные причины:\n1. Порт 5000 уже используется\n2. Отсутствуют права администратора\n3. Брандмауэр блокирует порт\n\nПопробуйте перезапустить игру",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.DialogResult = DialogResult.Cancel;
                            this.Close();
                        }
                    }));
                }
            });

            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        private void CancelLobby()
        {
            if (MessageBox.Show("Вы действительно хотите отменить создание игры?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _connectionAttempted = true;
                _isServerRunning = false;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        public NetworkManager GetNetwork()
        {
            return _network;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isServerRunning = false;
            _statusTimer?.Stop();

            if (!_connectionAttempted && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                CancelLobby();
                return;
            }

            _lobbyHost?.StopAdvertising();

            try
            {
                _tcpListener?.Stop();
                _serverThread?.Join(1000);
            }
            catch { }

            if (!_isReady && _network != null)
            {
                _network.Disconnect();
            }

            base.OnFormClosing(e);
        }
    }
}