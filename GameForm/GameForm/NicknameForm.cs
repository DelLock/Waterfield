using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship
{
    public class NicknameForm : Form
    {
        private TextBox txtNickname;
        public string PlayerNickname { get; private set; }

        public NicknameForm()
        {
            this.Text = "Введите ваш ник";
            this.Size = new Size(400, 200);
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
                Text = "🎮 Введите ваш никнейм",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblInfo = new Label
            {
                Text = "Ник будет виден другим игрокам:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(20, 60),
                AutoSize = true
            };

            txtNickname = new TextBox
            {
                Location = new Point(20, 90),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11),
                Text = $"Игрок_{new Random().Next(1000, 9999)}"
            };

            RoundedButton btnOK = new RoundedButton
            {
                Text = "✅ Начать игру",
                Location = new Point(20, 130),
                Size = new Size(340, 35),
                BackColor = Color.FromArgb(30, 144, 255),
                BorderColor = Color.FromArgb(65, 105, 225),
                BorderRadius = 10,
                BorderSize = 2,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(0, 191, 255)
            };
            btnOK.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtNickname.Text))
                {
                    PlayerNickname = txtNickname.Text.Trim();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Введите никнейм!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            contentPanel.Controls.Add(lblTitle);
            contentPanel.Controls.Add(lblInfo);
            contentPanel.Controls.Add(txtNickname);
            contentPanel.Controls.Add(btnOK);

            this.Controls.Add(contentPanel);
            this.AcceptButton = btnOK;
        }
    }
}