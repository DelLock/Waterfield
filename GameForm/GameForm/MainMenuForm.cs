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
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Button btnSolo = new Button
            {
                Text = "Играть против компьютера",
                Location = new Point(80, 80),
                Size = new Size(220, 40)
            };
            btnSolo.Click += (s, e) => StartSoloGame();

            Button btnOnline = new Button
            {
                Text = "Онлайн-мультиплеер",
                Location = new Point(80, 140),
                Size = new Size(220, 40)
            };
            btnOnline.Click += (s, e) => ShowOnlineMenu();

            this.Controls.Add(btnSolo);
            this.Controls.Add(btnOnline);
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