using System;

public class Board
{
    public bool[,] Grid { get; private set; }      // Где стоят корабли
    public bool[,] Shots { get; private set; }     // Где стреляли
    public int TotalShipCells { get; private set; }
    public int HitCells { get; set; }              // ✅ Изменили на public set

    public Board()
    {
        Grid = new bool[10, 10];
        Shots = new bool[10, 10];
        TotalShipCells = 0;
        HitCells = 0;
    }

    public void PlaceShipsRandomly()
    {
        ClearBoard();
        int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        Random rand = new Random();

        foreach (int size in shipSizes)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < 100)
            {
                int x = rand.Next(10);
                int y = rand.Next(10);
                bool isHorizontal = rand.Next(2) == 0;

                if (CanPlaceShip(x, y, size, isHorizontal))
                {
                    PlaceShip(x, y, size, isHorizontal);
                    TotalShipCells += size;
                    placed = true;
                }
                attempts++;
            }

            if (!placed)
            {
                ClearBoard();
                TotalShipCells = 0;
                PlaceShipsRandomly();
                return;
            }
        }
    }

    private void ClearBoard()
    {
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
                Grid[i, j] = Shots[i, j] = false;
        TotalShipCells = HitCells = 0;
    }

    private bool CanPlaceShip(int x, int y, int size, bool isHorizontal)
    {
        if (isHorizontal)
        {
            if (y + size > 10) return false;

            for (int i = 0; i < size; i++)
            {
                int checkY = y + i;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int checkX = x + dx;
                        int checkYWithOffset = checkY + dy;

                        if (checkX >= 0 && checkX < 10 && checkYWithOffset >= 0 && checkYWithOffset < 10)
                        {
                            if (Grid[checkX, checkYWithOffset])
                                return false;
                        }
                    }
                }
            }
        }
        else
        {
            if (x + size > 10) return false;

            for (int i = 0; i < size; i++)
            {
                int checkX = x + i;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int checkXWithOffset = checkX + dx;
                        int checkYWithOffset = y + dy;

                        if (checkXWithOffset >= 0 && checkXWithOffset < 10 && checkYWithOffset >= 0 && checkYWithOffset < 10)
                        {
                            if (Grid[checkXWithOffset, checkYWithOffset])
                                return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void PlaceShip(int x, int y, int size, bool isHorizontal)
    {
        for (int i = 0; i < size; i++)
        {
            int shipX = x + (!isHorizontal ? i : 0);
            int shipY = y + (isHorizontal ? i : 0);
            Grid[shipX, shipY] = true;
        }
    }

    public bool FireAt(int x, int y)
    {
        if (x < 0 || x >= 10 || y < 0 || y >= 10) return false;

        if (Shots[x, y]) return false;
        Shots[x, y] = true;
        if (Grid[x, y])
        {
            HitCells++;
            return true;
        }
        return false;
    }

    public bool IsAllShipsSunk() => HitCells >= TotalShipCells;
}