using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Battleship
{
    public partial class Form1 : Form
    {
        private enum GameState { PlayerTurn, EnemyTurn, GameOver }
        private Board myBoard = new Board();
        private Board enemyBoard = new Board();
        private Button[,] myButtons = new Button[10, 10];
        private Button[,] enemyButtons = new Button[10, 10];
        private GameState state = GameState.PlayerTurn;
        private NetworkManager _network;
        private bool _isOnline;
        private bool _isHost;
        private bool _isMyTurn = true;
        private object lastMoveTag;

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
            _isMyTurn = _isHost;
            SetupGamePanels();
            statusLabel.Text = _isMyTurn ? "Ваш ход!" : "Ход противника...";
            _network.OnMessageReceived += ProcessNetworkMessage;
        }

        private void ProcessNetworkMessage(string message)
        {
            if (IsDisposed || Disposing) return;

            if (message.StartsWith("MOVE:"))
            {
                string[] coords = message.Substring(5).Split(',');
                int x = int.Parse(coords[0]);
                int y = int.Parse(coords[1]);

                bool hit = myBoard.FireAt(x, y);
                _network?.SendResult(hit);
                UpdateUI();

                if (myBoard.IsAllShipsSunk())
                {
                    EndGame(false);
                    return;
                }

                _isMyTurn = !hit; // ← ИСПРАВЛЕНО: было _isMyToTurn
                if (!IsDisposed)
                    statusLabel.Text = _isMyTurn ? "Ваш ход!" : "Ход противника...";
            }
            else if (message.StartsWith("RESULT:"))
            {
                bool hit = bool.Parse(message.Substring(7));
                if (lastMoveTag != null)
                {
                    string[] parts = lastMoveTag.ToString().Split(',');
                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);

                    if (hit)
                        enemyBoard.Grid[x, y] = true;
                }

                _isMyTurn = hit;
                UpdateUI();
                if (!IsDisposed)
                    statusLabel.Text = _isMyTurn ? "Ваш ход!" : "Ход противника...";
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
            myBoard.PlaceShipsRandomly();
            enemyBoard.PlaceShipsRandomly();
            state = GameState.PlayerTurn;
            _isMyTurn = true;
            statusLabel.Text = "Ваш ход!";
            SetupGamePanels();
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
                    Button myBtn = myButtons[i, j];
                    if (myBoard.Shots[i, j])
                        myBtn.BackColor = myBoard.Grid[i, j] ? Color.Red : Color.LightBlue;
                    else
                        myBtn.BackColor = myBoard.Grid[i, j] ? Color.DarkGray : SystemColors.Control;

                    Button enemyBtn = enemyButtons[i, j];
                    if (enemyBoard.Shots[i, j])
                        enemyBtn.BackColor = enemyBoard.Grid[i, j] ? Color.Red : Color.LightBlue;
                    else
                        enemyBtn.BackColor = SystemColors.Control;
                }
            }
        }

        private void MyCellClick(object sender, EventArgs e) { }

        private void EnemyCellClick(object sender, EventArgs e)
        {
            if (_isOnline && !_isMyTurn) return;
            if (state == GameState.GameOver) return;

            Button btn = (Button)sender;
            Point coords = (Point)btn.Tag;
            int x = coords.Y;
            int y = coords.X;

            if (enemyBoard.Shots[x, y]) return;

            if (_isOnline)
            {
                lastMoveTag = $"{x},{y}";
                _network?.SendMove(x, y);
                enemyBoard.Shots[x, y] = true;
                _isMyTurn = false;
                statusLabel.Text = "Ход противника...";
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
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _network?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}