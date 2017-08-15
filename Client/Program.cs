using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    static class Program
    {
        public enum ProgramState { Login, MainWindow, UserInfo, Exit };
        public static ProgramState state = ProgramState.Login;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Login());

            while (state != ProgramState.Exit)
            {
                switch (state)
                {
                    case ProgramState.Login:
                        new Login().ShowDialog();
                        break;
                    case ProgramState.MainWindow:
                        new ClientMainForm().ShowDialog();
                        break;
                    case ProgramState.UserInfo:
                        new UserManagement().ShowDialog();
                        break;
                }
            }
        }
    }
}
