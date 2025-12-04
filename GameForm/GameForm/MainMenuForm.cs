using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship
{
    public class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            this.Text = "Морской бой — Главное меню";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 248, 255);
            this.Name = "MainMenuForm";

            Panel backgroundPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            Label titleLabel = new Label
            {
                Text = "🌊 МОРСКОЙ БОЙ",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Size = new Size(450, 60),
                Location = new Point(25, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            RoundedButton btnSolo = new RoundedButton
            {
                Text = "🎮 Играть против компьютера",
                Location = new Point(100, 120),
                Size = new Size(300, 50),
                BackColor = Color.FromArgb(30, 144, 255),
                BorderColor = Color.FromArgb(65, 105, 225),
                BorderRadius = 15,
                BorderSize = 2,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(0, 191, 255)
            };
            btnSolo.Click += (s, e) => StartSoloGame();

            RoundedButton btnOnline = new RoundedButton
            {
                Text = "🌐 Онлайн-мультиплеер",
                Location = new Point(100, 190),
                Size = new Size(300, 50),
                BackColor = Color.FromArgb(46, 139, 87),
                BorderColor = Color.FromArgb(34, 139, 34),
                BorderRadius = 15,
                BorderSize = 2,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(60, 179, 113)
            };
            btnOnline.Click += (s, e) => ShowOnlineMenu();

            RoundedButton btnExit = new RoundedButton
            {
                Text = "🚪 Выход",
                Location = new Point(100, 260),
                Size = new Size(300, 50),
                BackColor = Color.FromArgb(220, 20, 60),
                BorderColor = Color.FromArgb(178, 34, 34),
                BorderRadius = 15,
                BorderSize = 2,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(255, 69, 0)
            };
            btnExit.Click += (s, e) => Application.Exit();

            backgroundPanel.Controls.Add(titleLabel);
            backgroundPanel.Controls.Add(btnSolo);
            backgroundPanel.Controls.Add(btnOnline);
            backgroundPanel.Controls.Add(btnExit);
            this.Controls.Add(backgroundPanel);
        }

        private void StartSoloGame()
        {
            Form1 gameForm = new Form1(isTwoPlayers: false, isOnline: false, ip: null, isHost: false);
            gameForm.FormClosed += (s, e) => this.Show();
            this.Hide();
            gameForm.Show();
        }

        private void ShowOnlineMenu()
        {
            OnlineMenuForm onlineMenu = new OnlineMenuForm();
            onlineMenu.FormClosed += (s, e) => this.Show();
            this.Hide();
            onlineMenu.Show();
        }
    }
}