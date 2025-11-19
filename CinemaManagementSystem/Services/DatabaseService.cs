using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CinemaManagementSystem.Services
{
    /// <summary>
    /// Сервис для работы с базой данных
    /// </summary>
    public class DatabaseService
    {
        private readonly string connectionString;

        public DatabaseService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["CinemaDB"].ConnectionString;
        }

        /// <summary>
        /// Получить подключение к БД
        /// </summary>
        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Выполнить запрос без возврата данных
        /// </summary>
        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Выполнить запрос с возвратом одного значения
        /// </summary>
        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Выполнить запрос с возвратом DataTable
        /// </summary>
        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        /// Выполнить хранимую процедуру
        /// </summary>
        public DataTable ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        /// Выполнить хранимую процедуру без возврата данных
        /// </summary>
        public void ExecuteStoredProcedureNonQuery(string procedureName, params SqlParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}