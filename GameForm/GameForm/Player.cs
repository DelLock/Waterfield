using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship
{
    public class Player
    {
        public string Nickname { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }

        public Player(string nickname)
        {
            Nickname = nickname;
            Wins = 0;
            Losses = 0;
        }

        public double WinRate => Wins + Losses > 0 ? (double)Wins / (Wins + Losses) * 100 : 0;

        public override string ToString()
        {
            return $"{Nickname} (Побед: {Wins}, Поражений: {Losses}, WR: {WinRate:F1}%)";
        }
    }
}