using System;
using System.Windows.Forms;

namespace Battleship
{
    static class Program
    {
        public static Player CurrentPlayer { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var nicknameForm = new NicknameForm())
            {
                if (nicknameForm.ShowDialog() == DialogResult.OK)
                {
                    CurrentPlayer = new Player(nicknameForm.PlayerNickname);

                    bool menuExists = false;
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form is MainMenuForm)
                        {
                            menuExists = true;
                            form.BringToFront();
                            break;
                        }
                    }

                    if (!menuExists)
                    {
                        Application.Run(new MainMenuForm());
                    }
                }
            }
        }
    }
}