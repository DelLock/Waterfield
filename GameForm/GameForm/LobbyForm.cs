﻿using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Battleship
{
    public partial class LobbyForm : Form
    {
        private NetworkManager _network;
        private bool _isReady = false;

        public LobbyForm(string localIp)
        {
            this.Text = "Комната ожидания";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Label lblInfo = new Label
            {
                Text = "Ожидание второго игрока...",
                Location = new Point(20, 30),
                Size = new Size(360, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblIp = new Label
            {
                Text = $"Ваш IP для подключения:\n{localIp}",
                Location = new Point(20, 70),
                Size = new Size(360, 40),
                Font = new Font("Consolas", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(150, 150),
                Size = new Size(100, 30)
            };
            btnCancel.Click += (s, e) => Close();

            Controls.Add(lblInfo);
            Controls.Add(lblIp);
            Controls.Add(btnCancel);

            // Запуск сервера в фоне
            Task.Run(() => StartHostServer());
        }

        private void StartHostServer()
        {
            try
            {
                // ✅ Создаем NetworkManager для хоста
                _network = new NetworkManager("0.0.0.0", isHost: true);

                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show("Игрок присоединился! Игра начинается...", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _isReady = true;
                        this.DialogResult = DialogResult.OK;
                        Close();
                    }));
                }
                else
                {
                    MessageBox.Show("Игрок присоединился! Игра начинается...", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _isReady = true;
                    this.DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _isReady = false;
                        Close();
                    }));
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _isReady = false;
                    Close();
                }
            }
        }

        public NetworkManager GetNetwork() => _isReady ? _network : null;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_isReady)
            {
                _network?.Disconnect();
            }
            base.OnFormClosing(e);
        }
    }
}
