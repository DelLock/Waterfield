using System;

public class Board
{
    public bool[,] Grid { get; private set; }      // Где стоят корабли
    public bool[,] Shots { get; private set; }     // Где стреляли
    public int TotalShipCells { get; private set; }
    public int HitCells { get; private set; }

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
            while (!placed)
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
        if (isHorizontal && y + size > 10) return false;
        if (!isHorizontal && x + size > 10) return false;

        for (int dx = -1; dx <= (isHorizontal ? size : 1) + 1; dx++)
        {
            for (int dy = -1; dy <= (!isHorizontal ? size : 1) + 1; dy++)
            {
                int checkX = x + (!isHorizontal ? dx : 0);
                int checkY = y + (isHorizontal ? dy : 0);

                if (checkX >= 0 && checkX < 10 && checkY >= 0 && checkY < 10)
                {
                    if (Grid[checkX, checkY])
                        return false;
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
        if (Shots[x, y]) return false;
        Shots[x, y] = true;
        if (Grid[x, y]) HitCells++;
        return Grid[x, y];
    }

    public bool IsAllShipsSunk() => HitCells >= TotalShipCells;
}