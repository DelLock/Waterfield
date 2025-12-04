using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship
{
    public partial class Form1 : Form
    {
        private enum GameState { PlayerTurn, EnemyTurn, GameOver }
        private Board myBoard = new Board();
        private Board enemyBoard = new Board();
        private bool[,] enemyHitMap = new bool[10, 10];
        private RoundedButton[,] myButtons = new RoundedButton[10, 10];
        private RoundedButton[,] enemyButtons = new RoundedButton[10, 10];
        private GameState state = GameState.PlayerTurn;
        private NetworkManager _network;
        private bool _isOnline;
        private bool _isHost;
        private bool _isMyTurn = false;
        private object lastMoveTag;
        private bool _opponentDisconnected = false;

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
                    MessageBox.Show("Ошибка: сетевое соединение не установлено.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                Text = _isHost ? "Морской бой — Хост" : "Морской бой — Клиент";
                lblTitle.Text += _isHost ? " (Хост)" : " (Клиент)";
                SetupOnlineGame();
            }
            else
            {
                Text = "Морской бой — Против компьютера";
                lblTitle.Text += " • Одиночная игра";
                NewGame();
            }

            btnNewGame.Click += (s, e) => NewGame();
            UpdateShipsInfo();
        }

        private void SetupOnlineGame()
        {
            myBoard.PlaceShipsRandomly();
            SetupGamePanels();
            UpdateUI();

            if (!_network.IsConnected)
            {
                MessageBox.Show("Не удалось установить сетевое соединение.",
                    "Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            _network.OnMessageReceived += ProcessNetworkMessage;
            _isMyTurn = _isHost;

            if (_isHost)
            {
                statusLabel.Text = $"✅ Вы хост! Вы ходите первым!";
            }
            else
            {
                statusLabel.Text = $"⏳ Вы клиент! Ожидайте ход хоста...";
            }

            EnableEnemyButtons(_isMyTurn);
        }

        private void ProcessNetworkMessage(string message)
        {
            if (IsDisposed || Disposing) return;

            this.Invoke(new Action(() => {
                try
                {
                    if (message.StartsWith("MOVE:"))
                    {
                        string[] coords = message.Substring(5).Split(',');
                        int x = int.Parse(coords[0]);
                        int y = int.Parse(coords[1]);

                        bool hit = myBoard.FireAt(x, y);
                        UpdateUI();
                        UpdateShipsInfo();

                        statusLabel.Text = hit ?
                            $"🔥 Противник попал в ({x},{y})!" :
                            $"💨 Противник промахнулся в ({x},{y})!";

                        _network.SendResult(hit);

                        if (myBoard.IsAllShipsSunk())
                        {
                            EndGame(false);
                            return;
                        }

                        _isMyTurn = true;
                        statusLabel.Text += " Ваш ход!";
                        EnableEnemyButtons(true);
                    }
                    else if (message.StartsWith("RESULT:"))
                    {
                        bool hit = bool.Parse(message.Substring(7));

                        if (lastMoveTag != null)
                        {
                            string[] parts = lastMoveTag.ToString().Split(',');
                            int x = int.Parse(parts[0]);
                            int y = int.Parse(parts[1]);

                            enemyHitMap[x, y] = hit;
                            UpdateUI();
                            UpdateShipsInfo();

                            statusLabel.Text = hit ?
                                $"🎯 Вы попали в ({x},{y})!" :
                                $"🌊 Вы промахнулись в ({x},{y})!";

                            if (_isOnline && CheckOnlineWinCondition())
                            {
                                EndGame(true);
                                return;
                            }
                        }

                        if (hit)
                        {
                            _isMyTurn = true;
                            statusLabel.Text += " Стреляйте снова!";
                            EnableEnemyButtons(true);
                        }
                        else
                        {
                            _isMyTurn = false;
                            statusLabel.Text += " Ход противника...";
                            EnableEnemyButtons(false);
                        }
                    }
                    else if (message == "DISCONNECT")
                    {
                        _opponentDisconnected = true;
                        MessageBox.Show("⚠️ Противник отключился от игры.", "Отключение",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ReturnToMenu();
                    }
                }
                catch { }
            }));
        }

        private bool CheckOnlineWinCondition()
        {
            int totalHits = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (enemyHitMap[i, j]) totalHits++;
                }
            }
            return totalHits >= 20;
        }

        private void EnableEnemyButtons(bool enable)
        {
            foreach (var btn in enemyButtons)
            {
                if (btn != null)
                {
                    btn.Enabled = enable;
                    btn.Cursor = enable ? Cursors.Hand : Cursors.Default;
                }
            }
        }

        private void NewGame()
        {
            if (_isOnline)
            {
                MessageBox.Show("В онлайн-режиме новую игру можно начать только после перезапуска.",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            myBoard = new Board();
            enemyBoard = new Board();
            enemyHitMap = new bool[10, 10];
            myBoard.PlaceShipsRandomly();
            enemyBoard.PlaceShipsRandomly();
            state = GameState.PlayerTurn;
            _isMyTurn = true;
            statusLabel.Text = "🚢 Игра началась! Ваш ход!";
            SetupGamePanels();
            UpdateUI();
            UpdateShipsInfo();
            EnableEnemyButtons(true);
        }

        private void SetupGamePanels()
        {
            CreateButtonGrid(pnlPlayer, myButtons, MyCellClick, true);
            CreateButtonGrid(pnlOpponent, enemyButtons, EnemyCellClick, false);
        }

        private void CreateButtonGrid(Panel panel, RoundedButton[,] buttons, EventHandler clickHandler, bool isMyBoard)
        {
            panel.Controls.Clear();
            int buttonSize = 28;
            int spacing = 30;
            int offsetX = (panel.Width - (10 * spacing + 2)) / 2;
            int offsetY = (panel.Height - (10 * spacing + 2)) / 2;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    RoundedButton btn = new RoundedButton
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(offsetX + j * spacing, offsetY + i * spacing),
                        Tag = isMyBoard ? null : new Point(j, i),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = isMyBoard ? Color.FromArgb(220, 240, 255) : Color.FromArgb(240, 248, 255),
                        BorderRadius = 8,
                        BorderSize = 1,
                        BorderColor = Color.FromArgb(180, 200, 240),
                        HoverColor = Color.FromArgb(200, 220, 255),
                        Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(25, 25, 112),
                        Enabled = !isMyBoard && (_isOnline ? _isMyTurn : true)
                    };
                    btn.FlatAppearance.BorderSize = 0;
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
                    RoundedButton myBtn = myButtons[i, j];
                    if (myBoard.Shots[i, j])
                    {
                        myBtn.BackColor = myBoard.Grid[i, j] ?
                            Color.FromArgb(255, 100, 100) :
                            Color.FromArgb(180, 220, 255);
                    }
                    else
                    {
                        myBtn.BackColor = myBoard.Grid[i, j] ?
                            Color.FromArgb(100, 150, 200) :
                            Color.FromArgb(220, 240, 255);
                    }

                    RoundedButton enemyBtn = enemyButtons[i, j];

                    if (enemyBoard.Shots[i, j])
                    {
                        if (_isOnline)
                        {
                            if (enemyHitMap[i, j])
                            {
                                enemyBtn.BackColor = Color.FromArgb(255, 100, 100);
                            }
                            else
                            {
                                enemyBtn.BackColor = Color.FromArgb(200, 200, 240);
                            }
                        }
                        else
                        {
                            enemyBtn.BackColor = enemyBoard.Grid[i, j] ?
                                Color.FromArgb(255, 100, 100) :
                                Color.FromArgb(180, 220, 255);
                        }
                    }
                    else
                    {
                        enemyBtn.BackColor = Color.FromArgb(240, 248, 255);
                    }
                }
            }
        }

        private void UpdateShipsInfo()
        {
            int myShipsLeft = myBoard.TotalShipCells - myBoard.HitCells;
            int enemyShipsLeft = _isOnline ?
                (20 - CountEnemyHits()) :
                (enemyBoard.TotalShipCells - enemyBoard.HitCells);

            lblShipsInfo.Text = $"Ваши корабли: {myShipsLeft}/{myBoard.TotalShipCells} | " +
                              $"Корабли противника: {enemyShipsLeft}/{(_isOnline ? 20 : enemyBoard.TotalShipCells)}";
        }

        private int CountEnemyHits()
        {
            int hits = 0;
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    if (enemyHitMap[i, j]) hits++;
            return hits;
        }

        private void MyCellClick(object sender, EventArgs e) { }

        private void EnemyCellClick(object sender, EventArgs e)
        {
            if (!_isMyTurn)
            {
                statusLabel.Text = "⏳ Сейчас не ваш ход! Ожидайте...";
                return;
            }

            if (state == GameState.GameOver) return;

            RoundedButton btn = (RoundedButton)sender;
            if (btn.Tag == null) return;

            Point coords = (Point)btn.Tag;
            int x = coords.Y;
            int y = coords.X;

            if (enemyBoard.Shots[x, y])
            {
                statusLabel.Text = "⚠️ Сюда уже стреляли!";
                return;
            }

            lastMoveTag = $"{x},{y}";

            if (_isOnline)
            {
                if (!_network.IsConnected)
                {
                    statusLabel.Text = "❌ Соединение потеряно!";
                    MessageBox.Show("Соединение с противником потеряно.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ReturnToMenu();
                    return;
                }

                enemyBoard.Shots[x, y] = true;
                UpdateUI();
                UpdateShipsInfo();
                statusLabel.Text = $"🎯 Выстрел в ({x},{y})... Ожидание результата";

                bool sent = _network.SendMove(x, y);

                if (sent)
                {
                    _isMyTurn = false;
                    EnableEnemyButtons(false);
                }
                else
                {
                    statusLabel.Text = "❌ Ошибка отправки хода!";
                    enemyBoard.Shots[x, y] = false;
                    UpdateUI();
                    _isMyTurn = true;
                }
            }
            else
            {
                bool hit = enemyBoard.FireAt(x, y);
                enemyHitMap[x, y] = hit;
                enemyBoard.Shots[x, y] = true;
                UpdateUI();
                UpdateShipsInfo();
                statusLabel.Text = hit ?
                    $"🎯 Попадание в ({x},{y})! Стреляйте снова!" :
                    $"💨 Промах в ({x},{y})! Ход компьютера...";

                if (enemyBoard.IsAllShipsSunk())
                {
                    EndGame(true);
                    return;
                }

                if (!hit)
                {
                    EnableEnemyButtons(false);
                    ComputerTurn();
                }
            }
        }

        private void ComputerTurn()
        {
            if (state == GameState.GameOver) return;

            System.Threading.Thread.Sleep(800);

            Random rand = new Random();
            int x, y;
            do
            {
                x = rand.Next(10);
                y = rand.Next(10);
            } while (myBoard.Shots[x, y]);

            bool hit = myBoard.FireAt(x, y);
            UpdateUI();
            UpdateShipsInfo();

            string resultText = hit ?
                $"🔥 Компьютер попал в ({x},{y})!" :
                $"💨 Компьютер промахнулся в ({x},{y})!";

            if (myBoard.IsAllShipsSunk())
            {
                EndGame(false);
                return;
            }

            if (!hit)
            {
                state = GameState.PlayerTurn;
                statusLabel.Text = $"{resultText} Ваш ход!";
                EnableEnemyButtons(true);
            }
            else
            {
                statusLabel.Text = $"{resultText} Его ход продолжается...";
                System.Threading.Thread.Sleep(800);
                ComputerTurn();
            }
        }

        private void EndGame(bool isPlayerWinner)
        {
            if (state == GameState.GameOver) return;
            state = GameState.GameOver;

            string message = _isOnline
                ? (isPlayerWinner ? "🎉 Вы победили!" : "💀 Победил противник!")
                : (isPlayerWinner ? "🎉 Вы победили компьютер!" : "💀 Победил компьютер!");

            MessageBox.Show(message, "Конец игры", MessageBoxButtons.OK,
                isPlayerWinner ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation);

            _network?.Disconnect();
            EnableEnemyButtons(false);
            statusLabel.Text = "🏁 Игра завершена!";

            System.Windows.Forms.Timer returnTimer = new System.Windows.Forms.Timer();
            returnTimer.Interval = 2000;
            returnTimer.Tick += (s, e) => {
                returnTimer.Stop();
                ReturnToMenu();
            };
            returnTimer.Start();
        }

        private void ReturnToMenu()
        {
            if (IsDisposed || Disposing) return;

            _network?.Disconnect();
            Form mainMenu = Application.OpenForms["MainMenuForm"];
            if (mainMenu != null)
            {
                mainMenu.Show();
                this.Close();
            }
            else
            {
                this.Hide();
                new MainMenuForm().Show();
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isOnline && _network != null && _network.IsConnected && !_opponentDisconnected)
            {
                try
                {
                    _network.SendMessage("DISCONNECT");
                }
                catch { }
            }

            _network?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}