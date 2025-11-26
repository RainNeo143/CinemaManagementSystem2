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
            // ИСПРАВЛЕНО: Используем таблицу Сеанс напрямую вместо Репертуар
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
            // ИСПРАВЛЕНО: Используем таблицу Сеанс напрямую
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
        /// Получить свободные места для сеанса с информацией о бронированиях пользователя
        /// </summary>
        public DataTable GetAvailableSeatsWithUserBookings(int sessionId, int userId)
        {
            string query = @"
                SELECT 
                    m.Ряд,
                    m.Номер_места,
                    m.Тип_места,
                    CASE 
                        WHEN b.Код_бронирования IS NOT NULL AND b.Статус != N'Отменено' AND b.Код_пользователя = @Код_пользователя THEN N'МоёБронирование'
                        WHEN b.Код_бронирования IS NOT NULL AND b.Статус != N'Отменено' THEN N'Занято'
                        ELSE N'Свободно'
                    END AS Статус_места,
                    b.Код_бронирования
                FROM Места_в_залах m
                JOIN Сеанс s ON m.Номер_зала = s.Номер_зала
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND m.Ряд = b.Ряд 
                    AND m.Номер_места = b.Номер_места
                    AND b.Статус != N'Отменено'
                WHERE s.Код_сеанса = @Код_сеанса
                ORDER BY m.Ряд, m.Номер_места";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Код_сеанса", sessionId),
                new SqlParameter("@Код_пользователя", userId)
            };

            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Получить свободные места для сеанса (старый метод для совместимости)
        /// </summary>
        public DataTable GetAvailableSeats(int sessionId)
        {
            try
            {
                var parameter = new SqlParameter("@Код_сеанса", sessionId);
                return dbService.ExecuteStoredProcedure("ПолучитьСвободныеМеста", parameter);
            }
            catch
            {
                // Fallback на прямой запрос если процедура не существует
                string query = @"
                    SELECT 
                        m.Ряд,
                        m.Номер_места,
                        m.Тип_места,
                        CASE 
                            WHEN b.Код_бронирования IS NOT NULL AND b.Статус != N'Отменено' THEN N'Занято'
                            ELSE N'Свободно'
                        END AS Статус_места
                    FROM Места_в_залах m
                    JOIN Сеанс s ON m.Номер_зала = s.Номер_зала
                    LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                        AND m.Ряд = b.Ряд 
                        AND m.Номер_места = b.Номер_места
                        AND b.Статус != N'Отменено'
                    WHERE s.Код_сеанса = @Код_сеанса
                    ORDER BY m.Ряд, m.Номер_места";

                var parameter = new SqlParameter("@Код_сеанса", sessionId);
                return dbService.ExecuteQuery(query, parameter);
            }
        }

        /// <summary>
        /// Получить цену билета для сеанса
        /// </summary>
        public decimal GetTicketPrice(int sessionId, string seatType = "Обычное")
        {
            // ИСПРАВЛЕНО: Используем таблицу Сеанс напрямую
            string query = "SELECT Цена_билета FROM Сеанс WHERE Код_сеанса = @Код_сеанса";
            var parameter = new SqlParameter("@Код_сеанса", sessionId);

            object result = dbService.ExecuteScalar(query, parameter);
            decimal basePrice = result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;

            // VIP места на 50% дороже
            if (seatType == "VIP")
                basePrice *= 1.5m;

            return basePrice;
        }

        /// <summary>
        /// Забронировать и оплатить билет
        /// </summary>
        public BookingResult BookAndPayTicket(int userId, int sessionId, int row, int seatNumber)
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
                    return new BookingResult
                    {
                        Success = true,
                        BookingId = Convert.ToInt32(result.Rows[0]["Код_бронирования"]),
                        TicketNumber = result.Rows[0]["Номер_билета"].ToString(),
                        Amount = Convert.ToDecimal(result.Rows[0]["Сумма"])
                    };
                }

                return new BookingResult { Success = false, ErrorMessage = "Не удалось создать бронирование" };
            }
            catch (SqlException ex)
            {
                // Ловим ошибки из RAISERROR
                return new BookingResult { Success = false, ErrorMessage = ex.Message };
            }
            catch (Exception ex)
            {
                return new BookingResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Забронировать билет (старый метод для совместимости)
        /// </summary>
        public int BookTicket(int userId, int sessionId, int row, int seatNumber)
        {
            var result = BookAndPayTicket(userId, sessionId, row, seatNumber);
            return result.Success ? result.BookingId : -1;
        }

        /// <summary>
        /// Получить информацию о билете для печати
        /// </summary>
        public TicketInfo GetTicketInfo(int bookingId)
        {
            try
            {
                var parameter = new SqlParameter("@Код_бронирования", bookingId);
                DataTable result = dbService.ExecuteStoredProcedure("ПолучитьИнфоБилета", parameter);

                if (result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    return new TicketInfo
                    {
                        BookingId = Convert.ToInt32(row["Код_бронирования"]),
                        TicketNumber = row["Номер_билета"]?.ToString() ?? $"TICKET-{bookingId}",
                        FilmTitle = row["Фильм"].ToString(),
                        Genre = row["Жанр"]?.ToString() ?? "",
                        Duration = row["Длительность"] != DBNull.Value ? Convert.ToInt32(row["Длительность"]) : 0,
                        AgeRating = row["Возрастные_ограничения"]?.ToString() ?? "",
                        SessionDate = Convert.ToDateTime(row["Дата"]),
                        StartTime = (TimeSpan)row["Время_начала"],
                        EndTime = (TimeSpan)row["Время_окончания"],
                        HallName = row["Зал"].ToString(),
                        Row = Convert.ToInt32(row["Ряд"]),
                        SeatNumber = Convert.ToInt32(row["Номер_места"]),
                        SeatType = row["Тип_места"]?.ToString() ?? "Обычное",
                        Amount = Convert.ToDecimal(row["Сумма"]),
                        BookingDate = Convert.ToDateTime(row["Дата_бронирования"]),
                        BuyerName = row["Покупатель"]?.ToString() ?? ""
                    };
                }

                return null;
            }
            catch
            {
                // Если процедура не существует, используем прямой запрос
                return GetTicketInfoDirect(bookingId);
            }
        }

        private TicketInfo GetTicketInfoDirect(int bookingId)
        {
            // ИСПРАВЛЕНО: Используем Сеанс вместо Репертуар
            string query = @"
                SELECT 
                    b.Код_бронирования,
                    ISNULL(b.Номер_билета, 'TICKET-' + CAST(b.Код_бронирования AS NVARCHAR)) AS Номер_билета,
                    f.Наименование AS Фильм,
                    j.Наименование AS Жанр,
                    f.Длительность,
                    f.Возрастные_ограничения,
                    s.Дата,
                    s.Время_начала,
                    s.Время_окончания,
                    z.Наименование AS Зал,
                    b.Ряд,
                    b.Номер_места,
                    ISNULL(m.Тип_места, N'Обычное') AS Тип_места,
                    b.Сумма,
                    b.Дата_бронирования,
                    p.ФИО AS Покупатель
                FROM Бронирования b
                JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса
                JOIN Фильмы f ON s.Код_фильма = f.Код_фильма
                LEFT JOIN Жанры j ON f.Код_жанра = j.Код_жанра
                JOIN Залы z ON s.Номер_зала = z.Номер_зала
                LEFT JOIN Места_в_залах m ON s.Номер_зала = m.Номер_зала AND b.Ряд = m.Ряд AND b.Номер_места = m.Номер_места
                JOIN Пользователи p ON b.Код_пользователя = p.Код_пользователя
                WHERE b.Код_бронирования = @Код_бронирования";

            var parameter = new SqlParameter("@Код_бронирования", bookingId);
            DataTable result = dbService.ExecuteQuery(query, parameter);

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                return new TicketInfo
                {
                    BookingId = Convert.ToInt32(row["Код_бронирования"]),
                    TicketNumber = row["Номер_билета"].ToString(),
                    FilmTitle = row["Фильм"].ToString(),
                    Genre = row["Жанр"]?.ToString() ?? "",
                    Duration = row["Длительность"] != DBNull.Value ? Convert.ToInt32(row["Длительность"]) : 0,
                    AgeRating = row["Возрастные_ограничения"]?.ToString() ?? "",
                    SessionDate = Convert.ToDateTime(row["Дата"]),
                    StartTime = (TimeSpan)row["Время_начала"],
                    EndTime = (TimeSpan)row["Время_окончания"],
                    HallName = row["Зал"].ToString(),
                    Row = Convert.ToInt32(row["Ряд"]),
                    SeatNumber = Convert.ToInt32(row["Номер_места"]),
                    SeatType = row["Тип_места"]?.ToString() ?? "Обычное",
                    Amount = Convert.ToDecimal(row["Сумма"]),
                    BookingDate = Convert.ToDateTime(row["Дата_бронирования"]),
                    BuyerName = row["Покупатель"]?.ToString() ?? ""
                };
            }

            return null;
        }

        /// <summary>
        /// Получить историю бронирований пользователя
        /// </summary>
        public DataTable GetUserBookings(int userId)
        {
            // ИСПРАВЛЕНО: Используем Сеанс вместо Репертуар
            string query = @"
                SELECT 
                    b.Код_бронирования,
                    ISNULL(b.Номер_билета, 'TICKET-' + CAST(b.Код_бронирования AS NVARCHAR)) AS Номер_билета,
                    f.Наименование AS Фильм,
                    s.Дата,
                    s.Время_начала,
                    z.Наименование AS Зал,
                    b.Ряд,
                    b.Номер_места,
                    b.Сумма,
                    ISNULL(b.Статус, N'Неизвестно') AS Статус,
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
        /// Отменить бронирование с возвратом денег
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
            // ИСПРАВЛЕНО: Используем Сеанс вместо Репертуар
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

    /// <summary>
    /// Результат бронирования
    /// </summary>
    public class BookingResult
    {
        public bool Success { get; set; }
        public int BookingId { get; set; }
        public string TicketNumber { get; set; }
        public decimal Amount { get; set; }
        public string ErrorMessage { get; set; }
    }
}