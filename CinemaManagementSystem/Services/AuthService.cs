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
                        Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : "",
                        Balance = row["Баланс"] != DBNull.Value ? Convert.ToDecimal(row["Баланс"]) : 20000.00m
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

        /// <summary>
        /// Получение баланса пользователя
        /// </summary>
        public decimal GetUserBalance(int userId)
        {
            try
            {
                var parameter = new SqlParameter("@Код_пользователя", userId);
                DataTable result = dbService.ExecuteStoredProcedure("ПолучитьБаланс", parameter);

                if (result.Rows.Count > 0 && result.Rows[0]["Баланс"] != DBNull.Value)
                {
                    return Convert.ToDecimal(result.Rows[0]["Баланс"]);
                }

                return 0;
            }
            catch
            {
                // Если процедура не существует, используем прямой запрос
                string query = "SELECT Баланс FROM Пользователи WHERE Код_пользователя = @Код_пользователя";
                var parameter = new SqlParameter("@Код_пользователя", userId);
                object result = dbService.ExecuteScalar(query, parameter);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 20000.00m;
            }
        }

        /// <summary>
        /// Пополнение баланса
        /// </summary>
        public decimal TopUpBalance(int userId, decimal amount)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Код_пользователя", userId),
                    new SqlParameter("@Сумма", amount)
                };

                DataTable result = dbService.ExecuteStoredProcedure("ПополнитьБаланс", parameters);

                if (result.Rows.Count > 0 && result.Rows[0]["Баланс"] != DBNull.Value)
                {
                    return Convert.ToDecimal(result.Rows[0]["Баланс"]);
                }

                return GetUserBalance(userId);
            }
            catch
            {
                // Если процедура не существует, используем прямой запрос
                string query = "UPDATE Пользователи SET Баланс = Баланс + @Сумма WHERE Код_пользователя = @Код_пользователя; SELECT Баланс FROM Пользователи WHERE Код_пользователя = @Код_пользователя";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Код_пользователя", userId),
                    new SqlParameter("@Сумма", amount)
                };
                object result = dbService.ExecuteScalar(query, parameters);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }
    }
}