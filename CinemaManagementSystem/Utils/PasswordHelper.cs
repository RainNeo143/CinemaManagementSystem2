using System;
using System.Security.Cryptography;
using System.Text;

namespace CinemaManagementSystem.Utils
{
    /// <summary>
    /// Утилита для хеширования паролей
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Хеширование пароля с использованием SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Проверка пароля
        /// </summary>
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            string inputHash = HashPassword(inputPassword);
            return inputHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}