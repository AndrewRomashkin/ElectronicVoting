using System;
using System.Windows.Forms;
using Common.DataStructures;

namespace Client
{
    static class Program
    {
        public enum ProgramState { Login, MainWindow, UserInfo, Exit };
        public static ProgramState State = ProgramState.Login;
        public static UserInfo UserInfo = null;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (State != ProgramState.Exit)
            {
                Form formToShow = null;
                switch (State)
                {
                    case ProgramState.Login:
                        formToShow = new Login();
                        break;
                    case ProgramState.MainWindow:
                        formToShow = new ClientMainForm();
                        break;
                    case ProgramState.UserInfo:
                        formToShow = new UserManagement();
                        break;
                }
                State = ProgramState.Exit;
                formToShow.ShowDialog();
            }
        }
    }
}
