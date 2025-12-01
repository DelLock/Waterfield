using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace Battleship
{
    public class OnlineMenuForm : Form
    {
        private TextBox txtIp;

        public OnlineMenuForm()
        {
            this.Text = "Онлайн-мультиплеер";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Label lblInfo = new Label
            {
                Text = "Введите IP-адрес хоста (если присоединяетесь):",
                Location = new Point(20, 30),
                AutoSize = true
            };

            txtIp = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(340, 23),
                Text = GetLocalIPAddress()
            };

            Button btnHost = new Button
            {
                Text = "Создать игру (хост)",
                Location = new Point(20, 100),
                Size = new Size(340, 35)
            };
            btnHost.Click += (s, e) => StartAsHost();

            Button btnJoin = new Button
            {
                Text = "Присоединиться",
                Location = new Point(20, 150),
                Size = new Size(340, 35)
            };
            btnJoin.Click += (s, e) => StartAsClient();

            Button btnBack = new Button
            {
                Text = "Назад",
                Location = new Point(20, 190),
                Size = new Size(100, 30)
            };
            btnBack.Click += (s, e) => this.Close();

            Controls.Add(lblInfo);
            Controls.Add(txtIp);
            Controls.Add(btnHost);
            Controls.Add(btnJoin);
            Controls.Add(btnBack);
        }

        private void StartAsHost()
        {
            string localIp = GetLocalIPAddress();
            LobbyForm lobby = new LobbyForm(localIp);
            lobby.FormClosed += (s, e) =>
            {
                var network = lobby.GetNetwork();
                if (network != null)
                {
                    // Запуск игры как хост
                    Form1 gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: null, isHost: true, network: network);
                    gameForm.FormClosed += (s2, e2) => this.Show();
                    this.Hide();
                    gameForm.Show();
                }
                else
                {
                    this.Show(); // Отмена
                }
            };
            this.Hide();
            lobby.Show();
        }

        private void StartAsClient()
        {
            try
            {
                var network = new NetworkManager(txtIp.Text, isHost: false);
                Form1 gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: null, isHost: false, network: network);
                gameForm.FormClosed += (s, e) => this.Show();
                this.Hide();
                gameForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться: {ex.Message}");
            }
        }

        public static string GetLocalIPAddress()
        {
            try
            {
                using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
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
    }
}