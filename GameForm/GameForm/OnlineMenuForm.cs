using System;
using System.Drawing;
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
                Text = "127.0.0.1"
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
            Form1 gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: "0.0.0.0", isHost: true);
            gameForm.FormClosed += (s, e) => this.Show();
            this.Hide();
            gameForm.Show();
        }

        private void StartAsClient()
        {
            Form1 gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: txtIp.Text, isHost: false);
            gameForm.FormClosed += (s, e) => this.Show();
            this.Hide();
            gameForm.Show();
        }
    }
}