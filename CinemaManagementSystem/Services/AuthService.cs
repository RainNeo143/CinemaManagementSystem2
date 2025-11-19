using System;
using System.Data;
using System.Data.SqlClient;
using CinemaManagementSystem.Models;
using CinemaManagementSystem.Utils;

namespace CinemaManagementSystem.Services
{
    /// <summary>
    /// Сервис аутентификации и авторизации
    /// </summary>
    public class AuthService
    {
        private readonly DatabaseService dbService;

        public AuthService()
        {
            dbService = new DatabaseService();
        }

        /// <summary>
        /// Вход в систему
        /// </summary>
        public User Login(string login, string password)
        {
            try
            {
                string passwordHash = PasswordHelper.HashPassword(password);

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Логин", login),
                    new SqlParameter("@Пароль_хеш", passwordHash)
                };

                DataTable result = dbService.ExecuteStoredProcedure("АвторизацияПользователя", parameters);

                if (result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    return new User
                    {
                        Id = Convert.ToInt32(row["Код_пользователя"]),
                        Login = row["Логин"].ToString(),
                        Role = row["Роль"].ToString(),
                        FullName = row["ФИО"].ToString(),
                        Email = row["Email"].ToString()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка входа: {ex.Message}");
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public bool Register(string login, string password, string fullName, string email, string phone)
        {
            try
            {
                string passwordHash = PasswordHelper.HashPassword(password);

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Логин", login),
                    new SqlParameter("@Пароль_хеш", passwordHash),
                    new SqlParameter("@ФИО", fullName),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Телефон", phone ?? (object)DBNull.Value)
                };

                dbService.ExecuteStoredProcedureNonQuery("РегистрацияПользователя", parameters);
                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка регистрации: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверка существования пользователя
        /// </summary>
        public bool UserExists(string login)
        {
            string query = "SELECT COUNT(*) FROM Пользователи WHERE Логин = @Логин";
            var parameter = new SqlParameter("@Логин", login);

            int count = Convert.ToInt32(dbService.ExecuteScalar(query, parameter));
            return count > 0;
        }
    }
}