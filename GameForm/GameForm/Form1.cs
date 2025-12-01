﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship
{
    public partial class Form1 : Form
    {
        private enum GameState { PlayerTurn, EnemyTurn, GameOver }
        private Board myBoard = new Board();
        private Board enemyBoard = new Board();
        private bool[,] enemyHitMap = new bool[10, 10]; // true = попал
        private Button[,] myButtons = new Button[10, 10];
        private Button[,] enemyButtons = new Button[10, 10];
        private GameState state = GameState.PlayerTurn;
        private NetworkManager _network;
        private bool _isOnline;
        private bool _isHost;
        private bool _isMyTurn = true;
        private object lastMoveTag;
        private bool _waitingForResult = false; // ✅ Новая переменная для отслеживания ожидания результата

        public Form1(bool isTwoPlayers, bool isOnline, string ip, bool isHost, NetworkManager network = null)
        {
            InitializeComponent();
            _isOnline = isOnline;
            _isHost = isHost;
            _network = network;

            if (_isOnline)
            {
                if (_network == null)
                {
                    MessageBox.Show("Ошибка: сетевое соединение не установлено.");
                    Close();
                    return;
                }

                Text = _isHost ? "Морской бой — Хост" : "Морской бой — Клиент";
                SetupGameAfterConnection();
            }
            else
            {
                Text = "Морской бой — Против компьютера";
                NewGame();
            }

            btnNewGame.Click += (s, e) => NewGame();
        }

        private void SetupGameAfterConnection()
        {
            myBoard.PlaceShipsRandomly();
            _isMyTurn = _isHost; // Хост ходит первым
            SetupGamePanels();
            UpdateUI();

            // ✅ Отправляем сообщение о готовности
            if (_isHost)
            {
                // Хост отправляет "READY" клиенту
                _network.SendMessage("READY");
                statusLabel.Text = "Ваш ход!";
            }
            else
            {
                // Клиент отправляет "READY" хосту
                _network.SendMessage("READY");
                statusLabel.Text = "Ход противника...";
            }

            _network.OnMessageReceived += ProcessNetworkMessage;
        }

        private void ProcessNetworkMessage(string message)
        {
            if (IsDisposed || Disposing) return;

            // ✅ Отладочная информация в статусбаре
            this.Invoke(new Action(() => {
                statusLabel.Text = $"Получено: {message}";
            }));

            if (message.StartsWith("MOVE:"))
            {
                this.Invoke(new Action(() => {
                    try
                    {
                        string[] coords = message.Substring(5).Split(',');
                        int x = int.Parse(coords[0]);
                        int y = int.Parse(coords[1]);

                        // Противник стреляет по нашему полю
                        bool hit = myBoard.FireAt(x, y);

                        // Отправляем результат
                        _network.SendResult(hit);
                        UpdateUI();

                        if (myBoard.IsAllShipsSunk())
                        {
                            EndGame(false);
                            return;
                        }

                        // ✅ ПРОТИВНИК СТРЕЛЯЛ - ТЕПЕРЬ НАШ ХОД
                        _isMyTurn = true;
                        _waitingForResult = false;
                        statusLabel.Text = "Ваш ход!";
                        UpdateUI();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка MOVE: {ex.Message}");
                    }
                }));
            }
            else if (message.StartsWith("RESULT:"))
            {
                this.Invoke(new Action(() => {
                    try
                    {
                        bool hit = bool.Parse(message.Substring(7));

                        if (lastMoveTag != null)
                        {
                            string[] parts = lastMoveTag.ToString().Split(',');
                            int x = int.Parse(parts[0]);
                            int y = int.Parse(parts[1]);

                            enemyBoard.Shots[x, y] = true;
                            enemyHitMap[x, y] = hit;
                        }

                        _waitingForResult = false;

                        // ✅ ЕСЛИ ПОПАЛИ - продолжаем ход, если промах - передаем ход
                        if (hit)
                        {
                            _isMyTurn = true;
                            statusLabel.Text = "Попадание! Ваш ход!";
                        }
                        else
                        {
                            _isMyTurn = false;
                            statusLabel.Text = "Промах. Ход противника...";
                        }

                        UpdateUI();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка RESULT: {ex.Message}");
                    }
                }));
            }
            else if (message == "READY")
            {
                this.Invoke(new Action(() => {
                    // ✅ Получили подтверждение готовности от противника
                    if (_isHost)
                    {
                        // Хост уже ждет хода клиента
                        statusLabel.Text = "Клиент готов. Ваш ход!";
                    }
                    else
                    {
                        // Клиент ждет хоста
                        statusLabel.Text = "Хост готов. Ход противника...";
                    }
                }));
            }
        }

        private void NewGame()
        {
            if (_isOnline)
            {
                MessageBox.Show("В онлайн-режиме новую игру можно начать только после перезапуска.");
                return;
            }

            myBoard = new Board();
            enemyBoard = new Board();
            enemyHitMap = new bool[10, 10];
            myBoard.PlaceShipsRandomly();
            enemyBoard.PlaceShipsRandomly();
            state = GameState.PlayerTurn;
            _isMyTurn = true;
            _waitingForResult = false;
            statusLabel.Text = "Ваш ход!";
            SetupGamePanels();
            UpdateUI();
        }

        private void SetupGamePanels()
        {
            CreateButtonGrid(pnlPlayer, myButtons, MyCellClick, true);
            CreateButtonGrid(pnlOpponent, enemyButtons, EnemyCellClick, false);
        }

        private void CreateButtonGrid(Panel panel, Button[,] buttons, EventHandler clickHandler, bool isMyBoard)
        {
            panel.Controls.Clear();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(28, 28),
                        Location = new Point(j * 30 + 1, i * 30 + 1),
                        Tag = isMyBoard ? null : new Point(j, i),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = SystemColors.Control
                    };
                    btn.FlatAppearance.BorderColor = Color.Gray;
                    btn.Click += clickHandler;
                    buttons[i, j] = btn;
                    panel.Controls.Add(btn);
                }
            }
        }

        private void UpdateUI()
        {
            if (IsDisposed) return;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    // Моё поле
                    Button myBtn = myButtons[i, j];
                    if (myBoard.Shots[i, j])
                        myBtn.BackColor = myBoard.Grid[i, j] ? Color.Red : Color.LightBlue;
                    else
                        myBtn.BackColor = myBoard.Grid[i, j] ? Color.DarkGray : SystemColors.Control;

                    // Поле противника
                    Button enemyBtn = enemyButtons[i, j];
                    if (enemyBoard.Shots[i, j])
                        enemyBtn.BackColor = enemyHitMap[i, j] ? Color.Red : Color.LightBlue;
                    else
                        enemyBtn.BackColor = SystemColors.Control;
                }
            }
        }

        private void MyCellClick(object sender, EventArgs e) { /* Нельзя стрелять по своему полю */ }

        private void EnemyCellClick(object sender, EventArgs e)
        {
            if (_isOnline && (!_isMyTurn || _waitingForResult))
            {
                MessageBox.Show("Сейчас не ваш ход!", "Ожидание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (state == GameState.GameOver) return;

            Button btn = (Button)sender;
            if (btn.Tag == null) return;

            Point coords = (Point)btn.Tag;
            int x = coords.Y;
            int y = coords.X;

            if (enemyBoard.Shots[x, y])
            {
                MessageBox.Show("Сюда уже стреляли!", "Повтор", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isOnline)
            {
                lastMoveTag = $"{x},{y}";
                _network.SendMove(x, y);

                // ✅ Отмечаем, что мы выстрелили и ждем результата
                enemyBoard.Shots[x, y] = true;
                _waitingForResult = true;
                _isMyTurn = false; // Передаем ход противнику

                statusLabel.Text = "Ожидание результата...";
                UpdateUI();
            }
            else
            {
                bool hit = enemyBoard.FireAt(x, y);
                UpdateUI();

                if (enemyBoard.IsAllShipsSunk())
                {
                    EndGame(true);
                    return;
                }

                if (!hit)
                {
                    ComputerTurn();
                }
            }
        }

        private void ComputerTurn()
        {
            if (state == GameState.GameOver) return;

            Random rand = new Random();
            int x, y;
            do
            {
                x = rand.Next(10);
                y = rand.Next(10);
            } while (myBoard.Shots[x, y]);

            bool hit = myBoard.FireAt(x, y);
            UpdateUI();

            if (myBoard.IsAllShipsSunk())
            {
                EndGame(false);
                return;
            }

            if (!hit)
            {
                state = GameState.PlayerTurn;
                statusLabel.Text = "Ваш ход!";
            }
            else
            {
                System.Threading.Thread.Sleep(300);
                ComputerTurn();
            }
        }

        private void EndGame(bool isPlayerWinner)
        {
            if (state == GameState.GameOver) return;
            state = GameState.GameOver;

            string message = _isOnline
                ? (isPlayerWinner ? "Вы победили!" : "Победил противник!")
                : (isPlayerWinner ? "Вы победили!" : "Победил компьютер!");

            MessageBox.Show(message, "Конец игры", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _network?.Disconnect();

            foreach (var btn in enemyButtons)
            {
                btn.Enabled = false;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _network?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}
