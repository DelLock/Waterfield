using System;
using System.Windows.Forms;

namespace Battleship
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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