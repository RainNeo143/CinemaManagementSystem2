using System;
using System.Windows.Forms;
using CinemaManagementSystem.Forms;
using CinemaManagementSystem.Models;

namespace CinemaManagementSystem
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Показываем форму входа
            LoginForm loginForm = new LoginForm();

            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                User currentUser = loginForm.CurrentUser;

                if (currentUser != null)
                {
                    // В зависимости от роли открываем соответствующую форму
                    if (currentUser.IsAdmin)
                    {
                        Application.Run(new AdminForm(currentUser));
                    }
                    else if (currentUser.IsUser)
                    {
                        Application.Run(new MainUserForm(currentUser));
                    }
                }
            }
        }
    }
}