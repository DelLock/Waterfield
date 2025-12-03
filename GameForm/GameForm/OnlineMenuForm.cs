﻿using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace Battleship
{
    public class OnlineMenuForm : Form
    {
        private TextBox txtIp;
        private Label lblStatus;

        public OnlineMenuForm()
        {
            this.Text = "Онлайн-мультиплеер";
            this.Size = new Size(500, 400);
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
                Size = new Size(450, 40),
                Location = new Point(25, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblInfo = new Label
            {
                Text = "Введите IP-адрес хоста (если присоединяетесь):",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(50, 80),
                AutoSize = true
            };

            txtIp = new TextBox
            {
                Location = new Point(50, 110),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            lblStatus = new Label
            {
                Text = "Готов к подключению...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(25, 25, 112),
                Location = new Point(50, 145),
                Size = new Size(400, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            RoundedButton btnHost = new RoundedButton
            {
                Text = "🏠 Создать игру (Хост)",
                Location = new Point(50, 180),
                Size = new Size(400, 45),
                BackColor = Color.FromArgb(30, 144, 255),
                BorderColor = Color.FromArgb(65, 105, 225),
                BorderRadius = 15,
                BorderSize = 2,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(0, 191, 255)
            };
            btnHost.Click += (s, e) => StartAsHost();

            RoundedButton btnJoin = new RoundedButton
            {
                Text = "🔗 Присоединиться к игре",
                Location = new Point(50, 240),
                Size = new Size(400, 45),
                BackColor = Color.FromArgb(46, 139, 87),
                BorderColor = Color.FromArgb(34, 139, 34),
                BorderRadius = 15,
                BorderSize = 2,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                HoverColor = Color.FromArgb(60, 179, 113)
            };
            btnJoin.Click += (s, e) => StartAsClient();

            RoundedButton btnBack = new RoundedButton
            {
                Text = "← Назад",
                Location = new Point(50, 310),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(192, 192, 192),
                BorderColor = Color.FromArgb(169, 169, 169),
                BorderRadius = 10,
                BorderSize = 1,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(64, 64, 64),
                HoverColor = Color.FromArgb(211, 211, 211)
            };
            btnBack.Click += (s, e) => this.Close();

            contentPanel.Controls.Add(titleLabel);
            contentPanel.Controls.Add(lblInfo);
            contentPanel.Controls.Add(txtIp);
            contentPanel.Controls.Add(lblStatus);
            contentPanel.Controls.Add(btnHost);
            contentPanel.Controls.Add(btnJoin);
            contentPanel.Controls.Add(btnBack);

            this.Controls.Add(contentPanel);

            // Заполняем IP автоматически
            txtIp.Text = GetLocalIPAddress();
        }

        private void StartAsHost()
        {
            try
            {
                lblStatus.Text = "Создание игры...";
                this.Enabled = false;
                Application.DoEvents();

                string localIp = GetLocalIPAddress();
                LobbyForm lobby = new LobbyForm(localIp);

                DialogResult result = lobby.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var network = lobby.GetNetwork();
                    if (network != null && network.IsConnected)
                    {
                        lblStatus.Text = "✅ Игра создана! Запуск...";
                        Application.DoEvents();

                        Form1 gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: null, isHost: true, network: network);
                        gameForm.FormClosed += (s2, e2) =>
                        {
                            this.Show();
                            this.Enabled = true;
                        };
                        this.Hide();
                        gameForm.Show();
                    }
                    else
                    {
                        lblStatus.Text = "❌ Ошибка: Не удалось установить соединение";
                        this.Enabled = true;
                        MessageBox.Show("Не удалось установить сетевое соединение.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    lblStatus.Text = "Создание игры отменено";
                    this.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Ошибка: {ex.Message}";
                MessageBox.Show($"Не удалось создать игру: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Enabled = true;
            }
        }

        private void StartAsClient()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtIp.Text))
                {
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

                        Form1 gameForm = new Form1(isTwoPlayers: false, isOnline: true, ip: null, isHost: false, network: network);
                        gameForm.FormClosed += (s, e) =>
                        {
                            this.Show();
                            this.Enabled = true;
                        };
                        this.Hide();
                        gameForm.Show();
                    }
                    else
                    {
                        lblStatus.Text = "❌ Не удалось подключиться";
                        this.Enabled = true;
                        MessageBox.Show("Не удалось установить соединение с хостом. Проверьте IP-адрес и убедитесь, что хост запущен.",
                            "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    lblStatus.Text = "❌ Таймаут подключения";
                    this.Enabled = true;
                    MessageBox.Show("Таймаут подключения. Хост не отвечает.",
                        "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
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
                using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Dgram, 0))
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