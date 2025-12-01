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

                // ✅ Отладочная информация
                Console.WriteLine($"Форма создана: isHost={_isHost}, network.IsConnected={_network.IsConnected}");

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
            // Размещаем корабли
            myBoard.PlaceShipsRandomly();
            SetupGamePanels();
            UpdateUI();

            // ✅ ВАЖНО: Проверяем соединение перед настройкой обработчика
            if (!_network.IsConnected)
            {
                MessageBox.Show("Сетевое соединение потеряно. Игра будет закрыта.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // Настраиваем обработчик сетевых сообщений
            _network.OnMessageReceived += ProcessNetworkMessage;
            Console.WriteLine("Обработчик сетевых сообщений настроен");

            // ✅ ПРОСТАЯ ЛОГИКА: Хост начинает первым
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

            Console.WriteLine($"ProcessNetworkMessage вызван: {message}");

            this.Invoke(new Action(() => {
                try
                {
                    Console.WriteLine($"Обрабатываем сообщение в UI потоке: {message}");

                    if (message.StartsWith("MOVE:"))
                    {
                        // Противник сделал ход
                        string[] coords = message.Substring(5).Split(',');
                        int x = int.Parse(coords[0]);
                        int y = int.Parse(coords[1]);

                        Console.WriteLine($"Противник стреляет в {x},{y}");

                        // Проверяем попадание по нашему полю
                        bool hit = myBoard.FireAt(x, y);

                        // Отправляем результат противнику
                        bool sent = _network.SendResult(hit);
                        Console.WriteLine($"Результат отправлен: {hit}, успешно: {sent}");

                        UpdateUI();
                        UpdateShipsInfo();

                        if (myBoard.IsAllShipsSunk())
                        {
                            EndGame(false);
                            return;
                        }

                        // После выстрела противника - наш ход
                        _isMyTurn = true;
                        statusLabel.Text = hit ?
                            $"🔥 Противник попал в ({x},{y})! Ваш ход!" :
                            $"💨 Противник промахнулся в ({x},{y})! Ваш ход!";

                        EnableEnemyButtons(true);
                    }
                    else if (message.StartsWith("RESULT:"))
                    {
                        // Получили результат нашего выстрела
                        bool hit = bool.Parse(message.Substring(7));
                        Console.WriteLine($"Получен результат нашего выстрела: {hit}");

                        if (lastMoveTag != null)
                        {
                            string[] parts = lastMoveTag.ToString().Split(',');
                            int x = int.Parse(parts[0]);
                            int y = int.Parse(parts[1]);

                            Console.WriteLine($"Координаты выстрела: {x},{y}, результат: {hit}");

                            // ✅ Запоминаем результат выстрела
                            enemyBoard.Shots[x, y] = true;
                            enemyHitMap[x, y] = hit;

                            UpdateUI();
                            UpdateShipsInfo();

                            // ✅ Проверяем победу
                            if (_isOnline)
                            {
                                int totalHits = 0;
                                for (int i = 0; i < 10; i++)
                                    for (int j = 0; j < 10; j++)
                                        if (enemyHitMap[i, j]) totalHits++;

                                Console.WriteLine($"Всего попаданий: {totalHits}");

                                if (totalHits >= 20) // Всего 20 клеток кораблей
                                {
                                    EndGame(true);
                                    return;
                                }
                            }
                        }

                        if (hit)
                        {
                            // Попали - продолжаем ход
                            _isMyTurn = true;
                            statusLabel.Text = "🎯 Попадание! Стреляйте снова!";
                            EnableEnemyButtons(true);
                        }
                        else
                        {
                            // Промахнулись - ход противника
                            _isMyTurn = false;
                            statusLabel.Text = "🌊 Промах. Ход противника...";
                            EnableEnemyButtons(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки сообщения: {ex.Message}");
                    MessageBox.Show($"Ошибка обработки сетевого сообщения: {ex.Message}", "Сетевая ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private void EnableEnemyButtons(bool enable)
        {
            Console.WriteLine($"EnableEnemyButtons: {enable}");

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
                    // Моё поле
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

                    // Поле противника
                    RoundedButton enemyBtn = enemyButtons[i, j];
                    if (enemyBoard.Shots[i, j])
                    {
                        enemyBtn.BackColor = enemyHitMap[i, j] ?
                            Color.FromArgb(255, 100, 100) :
                            Color.FromArgb(180, 220, 255);
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
            int enemyShipsLeft = enemyBoard.TotalShipCells - enemyBoard.HitCells;

            lblShipsInfo.Text = $"Ваши корабли: {myShipsLeft}/{myBoard.TotalShipCells} | " +
                              $"Корабли противника: {enemyShipsLeft}/{enemyBoard.TotalShipCells}";
        }

        private void MyCellClick(object sender, EventArgs e) { }

        private void EnemyCellClick(object sender, EventArgs e)
        {
            Console.WriteLine($"EnemyCellClick вызван, isMyTurn={_isMyTurn}");

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

            Console.WriteLine($"Кликнули по клетке: x={x}, y={y}");

            if (enemyBoard.Shots[x, y])
            {
                statusLabel.Text = "⚠️ Сюда уже стреляли!";
                return;
            }

            // ✅ Сохраняем координаты выстрела
            lastMoveTag = $"{x},{y}";
            Console.WriteLine($"Сохранили lastMoveTag: {lastMoveTag}");

            if (_isOnline)
            {
                // ✅ Проверяем соединение перед отправкой
                if (!_network.IsConnected)
                {
                    statusLabel.Text = "❌ Соединение потеряно!";
                    MessageBox.Show("Сетевое соединение потеряно. Игра будет закрыта.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                Console.WriteLine($"Отправляю MOVE: x={x}, y={y}");

                // Отправляем ход противнику
                bool sent = _network.SendMove(x, y);
                Console.WriteLine($"SendMove результат: {sent}");

                if (sent)
                {
                    // ✅ НЕ отмечаем выстрел визуально пока не получим результат!
                    // Просто блокируем кнопки
                    _isMyTurn = false;
                    EnableEnemyButtons(false);
                    statusLabel.Text = $"🎯 Выстрел в ({x},{y})... Ожидание результата";

                    // ✅ Временно показываем "выстрел" серым цветом
                    RoundedButton clickedBtn = enemyButtons[x, y];
                    clickedBtn.BackColor = Color.FromArgb(200, 200, 220);
                }
                else
                {
                    statusLabel.Text = "❌ Ошибка отправки хода! Проверьте соединение.";
                    Console.WriteLine("ОШИБКА: SendMove вернул false");
                }
            }
            else
            {
                // Офлайн игра против компьютера
                bool hit = enemyBoard.FireAt(x, y);
                enemyHitMap[x, y] = hit;
                enemyBoard.Shots[x, y] = true;

                UpdateUI();
                UpdateShipsInfo();

                if (enemyBoard.IsAllShipsSunk())
                {
                    EndGame(true);
                    return;
                }

                if (!hit)
                {
                    statusLabel.Text = "💨 Промах! Ход компьютера...";
                    EnableEnemyButtons(false);
                    ComputerTurn();
                }
                else
                {
                    statusLabel.Text = "🎯 Попадание! Стреляйте снова!";
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
            UpdateShipsInfo();

            if (myBoard.IsAllShipsSunk())
            {
                EndGame(false);
                return;
            }

            if (!hit)
            {
                state = GameState.PlayerTurn;
                statusLabel.Text = "💨 Компьютер промахнулся! Ваш ход!";
                EnableEnemyButtons(true);
            }
            else
            {
                statusLabel.Text = $"🔥 Компьютер попал в ({x},{y})! Его ход продолжается...";
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
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _network?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}