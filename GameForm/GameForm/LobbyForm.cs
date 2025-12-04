using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Battleship
{
    public partial class LobbyForm : Form
    {
        private NetworkManager _network;
        private bool _isReady = false;
        private GameLobbyHost _lobbyHost;
        private string _localIp;
        private bool _connectionAttempted = false;

        public LobbyForm(string localIp)
        {
            _localIp = localIp;
            this.Text = "Комната ожидания";
            this.Size = new Size(500, 300);
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
                Text = "Ожидание второго игрока...",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 80),
                Size = new Size(460, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblIp = new Label
            {
                Text = $"Ваш IP для подключения:\n{_localIp}",
                Font = new Font("Consolas", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 120),
                Size = new Size(460, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(220, 240, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            RoundedButton btnCancel = new RoundedButton
            {
                Text = "✖ Отмена",
                Location = new Point(200, 200),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(220, 20, 60),
                BorderColor = Color.FromArgb(178, 34, 34),
                BorderRadius = 10,
                BorderSize = 1,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(255, 69, 0)
            };
            btnCancel.Click += (s, e) => Close();

            contentPanel.Controls.Add(lblTitle);
            contentPanel.Controls.Add(lblInfo);
            contentPanel.Controls.Add(lblIp);
            contentPanel.Controls.Add(btnCancel);

            this.Controls.Add(contentPanel);

            StartAdvertising();
            StartHostServer();
        }

        private void StartAdvertising()
        {
            _lobbyHost = new GameLobbyHost();
            _lobbyHost.StartAdvertising(Program.CurrentPlayer.Nickname, _localIp);
        }

        private void StartHostServer()
        {
            Task.Run(() =>
            {
                try
                {
                    _network = new NetworkManager("0.0.0.0", isHost: true);
                    _connectionAttempted = true;

                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            _isReady = true;
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }));
                }
                catch (Exception ex)
                {
                    _connectionAttempted = true;
                    this.Invoke(new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            MessageBox.Show($"❌ Ошибка создания игры: {ex.Message}\n\nПроверьте:\n1. Порт 5000 не занят\n2. Брандмауэр разрешает подключения",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _isReady = false;
                            this.Close();
                        }
                    }));
                }
            });
        }

        public NetworkManager GetNetwork()
        {
            if (_isReady && _network != null && _network.IsConnected)
                return _network;
            return null;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _lobbyHost?.StopAdvertising();

            if (!_connectionAttempted)
            {
                e.Cancel = true;
                return;
            }

            if (!_isReady && _network != null)
            {
                _network.Disconnect();
            }

            base.OnFormClosing(e);
        }
    }
}