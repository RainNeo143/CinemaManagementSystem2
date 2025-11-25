using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CinemaManagementSystem.Models;

namespace CinemaManagementSystem.Services
{
    /// <summary>
    /// Сервис для работы с бронированиями
    /// </summary>
    public class BookingService
    {
        private readonly DatabaseService dbService;

        public BookingService()
        {
            dbService = new DatabaseService();
        }

        /// <summary>
        /// Получить список всех фильмов в репертуаре
        /// </summary>
        public DataTable GetFilmRepertoire(DateTime? fromDate = null)
        {
            string query = @"
                SELECT DISTINCT
                    f.Код_фильма,
                    f.Наименование AS Фильм,
                    j.Наименование AS Жанр,
                    f.Длительность,
                    f.Возрастные_ограничения,
                    f.Описание
                FROM Фильмы f
                LEFT JOIN Жанры j ON f.Код_жанра = j.Код_жанра
                JOIN Сеанс s ON f.Код_фильма = s.Код_фильма
                WHERE s.Дата >= @ДатаС
                ORDER BY f.Наименование";

            var parameter = new SqlParameter("@ДатаС", fromDate ?? DateTime.Today);
            return dbService.ExecuteQuery(query, parameter);
        }

        /// <summary>
        /// Получить сеансы для конкретного фильма
        /// </summary>
        public DataTable GetSessionsForFilm(int filmId, DateTime? date = null)
        {
            string query = @"
                SELECT 
                    s.Код_сеанса,
                    s.Дата,
                    s.Время_начала,
                    s.Время_окончания,
                    s.Цена_билета,
                    s.Номер_зала,
                    z.Наименование AS Зал,
                    z.Количество_мест,
                    z.Количество_мест - ISNULL(занятые.Занято, 0) AS Свободных_мест
                FROM Сеанс s
                JOIN Залы z ON s.Номер_зала = z.Номер_зала
                LEFT JOIN (
                    SELECT Код_сеанса, COUNT(*) AS Занято
                    FROM Бронирования
                    WHERE Статус != N'Отменено'
                    GROUP BY Код_сеанса
                ) занятые ON s.Код_сеанса = занятые.Код_сеанса
                WHERE s.Код_фильма = @Код_фильма
                  AND s.Дата >= @Дата
                ORDER BY s.Дата, s.Время_начала";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Код_фильма", filmId),
                new SqlParameter("@Дата", date ?? DateTime.Today)
            };

            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Получить свободные места для сеанса
        /// </summary>
        public DataTable GetAvailableSeats(int sessionId)
        {
            var parameter = new SqlParameter("@Код_сеанса", sessionId);
            return dbService.ExecuteStoredProcedure("ПолучитьСвободныеМеста", parameter);
        }

        /// <summary>
        /// Забронировать билет
        /// </summary>
        public int BookTicket(int userId, int sessionId, int row, int seatNumber)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Код_пользователя", userId),
                    new SqlParameter("@Код_сеанса", sessionId),
                    new SqlParameter("@Ряд", row),
                    new SqlParameter("@Номер_места", seatNumber)
                };

                DataTable result = dbService.ExecuteStoredProcedure("БронированиеБилета", parameters);

                if (result.Rows.Count > 0)
                {
                    return Convert.ToInt32(result.Rows[0]["Код_бронирования"]);
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка бронирования: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить историю бронирований пользователя
        /// </summary>
        public DataTable GetUserBookings(int userId)
        {
            string query = @"
        SELECT 
            b.Код_бронирования,
            f.Наименование AS Фильм,
            s.Дата,
            s.Время_начала,
            z.Наименование AS Зал,
            b.Ряд,
            b.Номер_места,
            b.Сумма,
            ISNULL(b.Статус, 'Неизвестно') AS Статус,  -- Добавлен ISNULL
            b.Дата_бронирования
        FROM Бронирования b
        INNER JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса
        INNER JOIN Фильмы f ON s.Код_фильма = f.Код_фильма
        INNER JOIN Залы z ON s.Номер_зала = z.Номер_зала
        WHERE b.Код_пользователя = @Код_пользователя
        ORDER BY s.Дата DESC, s.Время_начала DESC";

            var parameter = new SqlParameter("@Код_пользователя", userId);
            return dbService.ExecuteQuery(query, parameter);
        }

        /// <summary>
        /// Отменить бронирование
        /// </summary>
        public bool CancelBooking(int bookingId, int userId)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Код_бронирования", bookingId),
                    new SqlParameter("@Код_пользователя", userId)
                };

                DataTable result = dbService.ExecuteStoredProcedure("ОтменитьБронирование", parameters);
                return result.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка отмены: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить статистику для администратора
        /// </summary>
        public DataTable GetStatistics(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            string query = @"
                SELECT 
                    f.Наименование AS Фильм,
                    COUNT(b.Код_бронирования) AS Количество_бронирований,
                    SUM(b.Сумма) AS Общая_сумма,
                    AVG(CAST(b.Сумма AS FLOAT)) AS Средняя_цена,
                    s.Дата
                FROM Фильмы f
                JOIN Сеанс s ON f.Код_фильма = s.Код_фильма
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса AND b.Статус != N'Отменено'
                WHERE (@ДатаС IS NULL OR s.Дата >= @ДатаС)
                  AND (@ДатаПо IS NULL OR s.Дата <= @ДатаПо)
                GROUP BY f.Наименование, s.Дата
                ORDER BY s.Дата DESC, Количество_бронирований DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ДатаС", dateFrom ?? (object)DBNull.Value),
                new SqlParameter("@ДатаПо", dateTo ?? (object)DBNull.Value)
            };

            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Получить популярные фильмы
        /// </summary>
        public DataTable GetPopularFilms(int topCount = 10)
        {
            string query = @"
                SELECT TOP (@TopCount)
                    f.Наименование AS Фильм,
                    j.Наименование AS Жанр,
                    COUNT(b.Код_бронирования) AS Количество_билетов,
                    SUM(b.Сумма) AS Выручка
                FROM Фильмы f
                LEFT JOIN Жанры j ON f.Код_жанра = j.Код_жанра
                LEFT JOIN Сеанс s ON f.Код_фильма = s.Код_фильма
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                GROUP BY f.Наименование, j.Наименование
                ORDER BY Количество_билетов DESC, Выручка DESC";

            var parameter = new SqlParameter("@TopCount", topCount);
            return dbService.ExecuteQuery(query, parameter);
        }
    }
}