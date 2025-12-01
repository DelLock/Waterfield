using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship
{
    public partial class Form1 : Form
    {
        private enum GameState { Player1Turn, Player2Turn, ComputerTurn, GameOver }
        private Board playerBoard = new Board();
        private Board opponentBoard = new Board();
        private Button[,] playerButtons = new Button[10, 10];
        private Button[,] opponentButtons = new Button[10, 10];
        private GameState state;
        private bool isTwoPlayers = false;

        public Form1()
        {
            InitializeComponent();
            SetupGamePanels();
            NewGame();

            btnNewGame.Click += (s, e) => NewGame();
            chkTwoPlayers.CheckedChanged += (s, e) => isTwoPlayers = chkTwoPlayers.Checked;
        }

        private void SetupGamePanels()
        {
            CreateButtonGrid(pnlPlayer, playerButtons, PlayerCellClick);
            CreateButtonGrid(pnlOpponent, opponentButtons, OpponentCellClick);
        }

        private void CreateButtonGrid(Panel panel, Button[,] buttons, EventHandler clickHandler)
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
                        Tag = new Point(j, i),
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

        private void NewGame()
        {
            playerBoard = new Board();
            opponentBoard = new Board();
            playerBoard.PlaceShipsRandomly();
            opponentBoard.PlaceShipsRandomly();
            state = GameState.Player1Turn;
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Ваше поле
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    Button btn = playerButtons[i, j];
                    if (playerBoard.Shots[i, j])
                        btn.BackColor = playerBoard.Grid[i, j] ? Color.Red : Color.LightBlue;
                    else
                        btn.BackColor = playerBoard.Grid[i, j] ? Color.DarkGray : SystemColors.Control;
                }

            // Поле противника
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    Button btn = opponentButtons[i, j];
                    if (opponentBoard.Shots[i, j])
                        btn.BackColor = opponentBoard.Grid[i, j] ? Color.Red : Color.LightBlue;
                    else
                        btn.BackColor = SystemColors.Control;
                }
        }

        private void PlayerCellClick(object sender, EventArgs e) { /* нельзя стрелять по своему полю */ }

        private void OpponentCellClick(object sender, EventArgs e)
        {
            if (state == GameState.GameOver) return;

            var btn = (Button)sender;
            var coords = (Point)btn.Tag;
            int x = coords.Y; // строка
            int y = coords.X; // столбец

            if (isTwoPlayers && state == GameState.Player2Turn)
            {
                // Второй игрок стреляет по полю первого
                if (playerBoard.Shots[x, y]) return;
                bool hit = playerBoard.FireAt(x, y);
                UpdateUI();
                if (playerBoard.IsAllShipsSunk())
                {
                    MessageBox.Show("Победил Игрок 2!", "Конец игры", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    state = GameState.GameOver;
                }
                else
                {
                    state = hit ? GameState.Player2Turn : GameState.Player1Turn;
                }
                return;
            }

            // Первый игрок / одиночная игра
            if ((!isTwoPlayers && state == GameState.ComputerTurn) || (isTwoPlayers && state != GameState.Player1Turn))
                return;

            if (opponentBoard.Shots[x, y]) return;

            bool hitOpp = opponentBoard.FireAt(x, y);
            UpdateUI();

            if (opponentBoard.IsAllShipsSunk())
            {
                string msg = isTwoPlayers ? "Победил Игрок 1!" : "Вы победили!";
                MessageBox.Show(msg, "Конец игры", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state = GameState.GameOver;
                return;
            }

            if (!hitOpp)
            {
                state = isTwoPlayers ? GameState.Player2Turn : GameState.ComputerTurn;
                if (!isTwoPlayers)
                    ComputerTurn();
            }
        }

        private void ComputerTurn()
        {
            if (state != GameState.ComputerTurn || state == GameState.GameOver) return;

            Random rand = new Random();
            int x, y;
            do
            {
                x = rand.Next(10);
                y = rand.Next(10);
            } while (playerBoard.Shots[x, y]);

            bool hit = playerBoard.FireAt(x, y);
            UpdateUI();

            if (playerBoard.IsAllShipsSunk())
            {
                MessageBox.Show("Победил Компьютер!", "Конец игры", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state = GameState.GameOver;
                return;
            }

            if (!hit)
            {
                state = GameState.Player1Turn;
            }
            else
            {
                // Дополнительный ход при попадании
                System.Threading.Thread.Sleep(400);
                ComputerTurn();
            }
        }
    }
}