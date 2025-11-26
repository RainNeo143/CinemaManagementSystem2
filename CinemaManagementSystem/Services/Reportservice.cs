using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace CinemaManagementSystem.Services
{
    /// <summary>
    /// Сервис генерации PDF отчётов для администратора
    /// </summary>
    public class ReportService
    {
        private readonly BookingService bookingService;
        private DataTable currentReportData;
        private string currentReportTitle;
        private string currentReportSubtitle;
        private int currentPageIndex;
        private int rowsPerPage = 25;

        public ReportService()
        {
            bookingService = new BookingService();
        }

        #region Получение данных для отчётов

        /// <summary>
        /// Отчёт: Продажи за день
        /// </summary>
        public DataTable GetDailySalesReport(DateTime date)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    f.Наименование AS [Фильм],
                    CONVERT(varchar(5), s.Время_начала, 108) AS [Время],
                    z.Наименование AS [Зал],
                    COUNT(b.Код_бронирования) AS [Продано],
                    ISNULL(SUM(b.Сумма), 0) AS [Выручка]
                FROM Сеанс s
                JOIN Фильмы f ON s.Код_фильма = f.Код_фильма
                JOIN Залы z ON s.Номер_зала = z.Номер_зала
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                WHERE s.Дата = @Дата
                GROUP BY f.Наименование, s.Время_начала, z.Наименование
                ORDER BY s.Время_начала";

            var parameter = new System.Data.SqlClient.SqlParameter("@Дата", date.Date);
            return dbService.ExecuteQuery(query, parameter);
        }

        /// <summary>
        /// Отчёт: Продажи за период
        /// </summary>
        public DataTable GetSalesReport(DateTime dateFrom, DateTime dateTo)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    CONVERT(varchar, s.Дата, 104) AS [Дата],
                    f.Наименование AS [Фильм],
                    COUNT(b.Код_бронирования) AS [Билетов],
                    ISNULL(SUM(b.Сумма), 0) AS [Выручка],
                    z.Наименование AS [Зал]
                FROM Сеанс s
                JOIN Фильмы f ON s.Код_фильма = f.Код_фильма
                JOIN Залы z ON s.Номер_зала = z.Номер_зала
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо
                GROUP BY s.Дата, f.Наименование, z.Наименование
                HAVING COUNT(b.Код_бронирования) > 0
                ORDER BY s.Дата DESC, Выручка DESC";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: ТОП фильмов по выручке
        /// </summary>
        public DataTable GetTopFilmsByRevenueReport(DateTime dateFrom, DateTime dateTo, int topCount = 10)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT TOP (@TopCount)
                    f.Наименование AS [Фильм],
                    ISNULL(j.Наименование, 'Не указан') AS [Жанр],
                    COUNT(b.Код_бронирования) AS [Билетов],
                    ISNULL(SUM(b.Сумма), 0) AS [Выручка],
                    COUNT(DISTINCT s.Код_сеанса) AS [Сеансов]
                FROM Фильмы f
                LEFT JOIN Жанры j ON f.Код_жанра = j.Код_жанра
                LEFT JOIN Сеанс s ON f.Код_фильма = s.Код_фильма
                    AND s.Дата BETWEEN @ДатаС AND @ДатаПо
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                GROUP BY f.Наименование, j.Наименование
                HAVING COUNT(b.Код_бронирования) > 0
                ORDER BY [Выручка] DESC";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@TopCount", topCount),
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: Загруженность залов
        /// </summary>
        public DataTable GetHallOccupancyReport(DateTime dateFrom, DateTime dateTo)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    z.Наименование AS [Зал],
                    z.Количество_мест AS [Мест],
                    COUNT(DISTINCT s.Код_сеанса) AS [Сеансов],
                    COUNT(b.Код_бронирования) AS [Продано],
                    ISNULL(SUM(b.Сумма), 0) AS [Выручка],
                    CASE 
                        WHEN COUNT(DISTINCT s.Код_сеанса) * z.Количество_мест > 0 
                        THEN CAST(ROUND(
                            CAST(COUNT(b.Код_бронирования) AS FLOAT) / 
                            (COUNT(DISTINCT s.Код_сеанса) * z.Количество_мест) * 100, 1
                        ) AS DECIMAL(5,1))
                        ELSE 0 
                    END AS [Заполн. %]
                FROM Залы z
                LEFT JOIN Сеанс s ON z.Номер_зала = s.Номер_зала
                    AND s.Дата BETWEEN @ДатаС AND @ДатаПо
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                GROUP BY z.Номер_зала, z.Наименование, z.Количество_мест
                ORDER BY [Выручка] DESC";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: Статистика по жанрам
        /// </summary>
        public DataTable GetGenreStatisticsReport(DateTime dateFrom, DateTime dateTo)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    ISNULL(j.Наименование, 'Без жанра') AS [Жанр],
                    COUNT(DISTINCT f.Код_фильма) AS [Фильмов],
                    COUNT(DISTINCT s.Код_сеанса) AS [Сеансов],
                    COUNT(b.Код_бронирования) AS [Билетов],
                    ISNULL(SUM(b.Сумма), 0) AS [Выручка]
                FROM Фильмы f
                LEFT JOIN Жанры j ON f.Код_жанра = j.Код_жанра
                LEFT JOIN Сеанс s ON f.Код_фильма = s.Код_фильма
                    AND s.Дата BETWEEN @ДатаС AND @ДатаПо
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                GROUP BY j.Наименование
                ORDER BY [Выручка] DESC";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: Активность пользователей (ТОП покупателей)
        /// </summary>
        public DataTable GetUserActivityReport(DateTime dateFrom, DateTime dateTo, int topCount = 20)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT TOP (@TopCount)
                    p.ФИО AS [Пользователь],
                    p.Email AS [Email],
                    COUNT(b.Код_бронирования) AS [Заказов],
                    SUM(CASE WHEN b.Статус = N'Забронировано' THEN 1 ELSE 0 END) AS [Активных],
                    SUM(CASE WHEN b.Статус = N'Отменено' THEN 1 ELSE 0 END) AS [Отмен.],
                    ISNULL(SUM(CASE WHEN b.Статус != N'Отменено' THEN b.Сумма ELSE 0 END), 0) AS [Сумма]
                FROM Пользователи p
                JOIN Бронирования b ON p.Код_пользователя = b.Код_пользователя
                JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса
                WHERE p.Роль = N'Пользователь'
                    AND s.Дата BETWEEN @ДатаС AND @ДатаПо
                GROUP BY p.Код_пользователя, p.ФИО, p.Email
                HAVING COUNT(b.Код_бронирования) > 0
                ORDER BY [Сумма] DESC";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@TopCount", topCount),
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: Отменённые бронирования
        /// </summary>
        public DataTable GetCancelledBookingsReport(DateTime dateFrom, DateTime dateTo)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    CONVERT(varchar, b.Дата_бронирования, 104) AS [Дата отмены],
                    p.ФИО AS [Пользователь],
                    f.Наименование AS [Фильм],
                    CONVERT(varchar, s.Дата, 104) AS [Дата сеанса],
                    b.Сумма AS [Возврат]
                FROM Бронирования b
                JOIN Пользователи p ON b.Код_пользователя = p.Код_пользователя
                JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса
                JOIN Фильмы f ON s.Код_фильма = f.Код_фильма
                WHERE b.Статус = N'Отменено'
                    AND b.Дата_бронирования BETWEEN @ДатаС AND @ДатаПо
                ORDER BY b.Дата_бронирования DESC";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: Сводка за период
        /// </summary>
        public DataTable GetPeriodSummaryReport(DateTime dateFrom, DateTime dateTo)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    'Всего сеансов' AS [Показатель],
                    CAST((SELECT COUNT(*) FROM Сеанс WHERE Дата BETWEEN @ДатаС AND @ДатаПо) AS NVARCHAR) AS [Значение]
                UNION ALL
                SELECT 
                    'Продано билетов',
                    CAST((SELECT COUNT(*) FROM Бронирования b 
                     JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса 
                     WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо AND b.Статус != N'Отменено') AS NVARCHAR)
                UNION ALL
                SELECT 
                    'Общая выручка (₸)',
                    CAST((SELECT ISNULL(SUM(b.Сумма), 0) FROM Бронирования b 
                     JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса 
                     WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо AND b.Статус != N'Отменено') AS NVARCHAR)
                UNION ALL
                SELECT 
                    'Отменено бронирований',
                    CAST((SELECT COUNT(*) FROM Бронирования b 
                     JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса 
                     WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо AND b.Статус = N'Отменено') AS NVARCHAR)
                UNION ALL
                SELECT 
                    'Уникальных покупателей',
                    CAST((SELECT COUNT(DISTINCT b.Код_пользователя) FROM Бронирования b 
                     JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса 
                     WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо AND b.Статус != N'Отменено') AS NVARCHAR)
                UNION ALL
                SELECT 
                    'Средний чек (₸)',
                    CAST(ISNULL((SELECT AVG(b.Сумма) FROM Бронирования b 
                     JOIN Сеанс s ON b.Код_сеанса = s.Код_сеанса 
                     WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо AND b.Статус != N'Отменено'), 0) AS NVARCHAR)";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: Расписание сеансов на день
        /// </summary>
        public DataTable GetScheduleReport(DateTime date)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    CONVERT(varchar(5), s.Время_начала, 108) AS [Начало],
                    CONVERT(varchar(5), s.Время_окончания, 108) AS [Конец],
                    f.Наименование AS [Фильм],
                    z.Наименование AS [Зал],
                    s.Цена_билета AS [Цена],
                    z.Количество_мест - ISNULL(занятые.Занято, 0) AS [Свободно]
                FROM Сеанс s
                JOIN Фильмы f ON s.Код_фильма = f.Код_фильма
                JOIN Залы z ON s.Номер_зала = z.Номер_зала
                LEFT JOIN (
                    SELECT Код_сеанса, COUNT(*) AS Занято
                    FROM Бронирования
                    WHERE Статус != N'Отменено'
                    GROUP BY Код_сеанса
                ) занятые ON s.Код_сеанса = занятые.Код_сеанса
                WHERE s.Дата = @Дата
                ORDER BY s.Время_начала, z.Наименование";

            var parameter = new System.Data.SqlClient.SqlParameter("@Дата", date.Date);
            return dbService.ExecuteQuery(query, parameter);
        }

        /// <summary>
        /// Отчёт: Продажи по дням недели
        /// </summary>
        public DataTable GetSalesByDayOfWeekReport(DateTime dateFrom, DateTime dateTo)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    CASE DATEPART(WEEKDAY, s.Дата)
                        WHEN 1 THEN 'Воскресенье'
                        WHEN 2 THEN 'Понедельник'
                        WHEN 3 THEN 'Вторник'
                        WHEN 4 THEN 'Среда'
                        WHEN 5 THEN 'Четверг'
                        WHEN 6 THEN 'Пятница'
                        WHEN 7 THEN 'Суббота'
                    END AS [День недели],
                    COUNT(b.Код_бронирования) AS [Билетов],
                    ISNULL(SUM(b.Сумма), 0) AS [Выручка],
                    COUNT(DISTINCT s.Код_сеанса) AS [Сеансов]
                FROM Сеанс s
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо
                GROUP BY DATEPART(WEEKDAY, s.Дата)
                ORDER BY DATEPART(WEEKDAY, s.Дата)";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Отчёт: Продажи по времени суток
        /// </summary>
        public DataTable GetSalesByTimeReport(DateTime dateFrom, DateTime dateTo)
        {
            var dbService = new DatabaseService();
            string query = @"
                SELECT 
                    CASE 
                        WHEN DATEPART(HOUR, s.Время_начала) < 12 THEN 'Утро (до 12:00)'
                        WHEN DATEPART(HOUR, s.Время_начала) < 17 THEN 'День (12:00-17:00)'
                        WHEN DATEPART(HOUR, s.Время_начала) < 21 THEN 'Вечер (17:00-21:00)'
                        ELSE 'Ночь (после 21:00)'
                    END AS [Время суток],
                    COUNT(b.Код_бронирования) AS [Билетов],
                    ISNULL(SUM(b.Сумма), 0) AS [Выручка],
                    COUNT(DISTINCT s.Код_сеанса) AS [Сеансов]
                FROM Сеанс s
                LEFT JOIN Бронирования b ON s.Код_сеанса = b.Код_сеанса 
                    AND b.Статус != N'Отменено'
                WHERE s.Дата BETWEEN @ДатаС AND @ДатаПо
                GROUP BY 
                    CASE 
                        WHEN DATEPART(HOUR, s.Время_начала) < 12 THEN 'Утро (до 12:00)'
                        WHEN DATEPART(HOUR, s.Время_начала) < 17 THEN 'День (12:00-17:00)'
                        WHEN DATEPART(HOUR, s.Время_начала) < 21 THEN 'Вечер (17:00-21:00)'
                        ELSE 'Ночь (после 21:00)'
                    END
                ORDER BY [Выручка] DESC";

            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@ДатаС", dateFrom.Date),
                new System.Data.SqlClient.SqlParameter("@ДатаПо", dateTo.Date)
            };
            return dbService.ExecuteQuery(query, parameters);
        }

        #endregion

        #region Генерация PDF

        /// <summary>
        /// Сохранить отчёт в PDF с диалогом выбора файла
        /// </summary>
        public string SaveReportToPdf(DataTable data, string reportTitle, string subtitle = "")
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PDF Document|*.pdf|PNG Image|*.png";
                saveDialog.Title = "Сохранить отчёт";
                saveDialog.FileName = $"{reportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    return GeneratePdfReport(data, reportTitle, saveDialog.FileName, subtitle);
                }
            }
            return null;
        }

        /// <summary>
        /// Генерация PDF отчёта
        /// </summary>
        public string GeneratePdfReport(DataTable data, string reportTitle, string filePath, string subtitle = "")
        {
            currentReportData = data;
            currentReportTitle = reportTitle;
            currentReportSubtitle = subtitle;
            currentPageIndex = 0;

            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".png")
            {
                return SaveReportAsImage(filePath);
            }

            try
            {
                using (PrintDocument printDoc = new PrintDocument())
                {
                    printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    printDoc.PrinterSettings.PrintToFile = true;
                    printDoc.PrinterSettings.PrintFileName = filePath;
                    printDoc.DefaultPageSettings.Landscape = true;

                    if (!printDoc.PrinterSettings.IsValid)
                    {
                        MessageBox.Show("PDF принтер недоступен. Отчёт будет сохранён как PNG.",
                            "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return SaveReportAsImage(Path.ChangeExtension(filePath, ".png"));
                    }

                    printDoc.PrintPage += PrintDocument_PrintPage;
                    printDoc.Print();
                }

                return filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания PDF: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int marginLeft = e.MarginBounds.Left;
            int marginTop = e.MarginBounds.Top;
            int pageWidth = e.MarginBounds.Width;
            int pageHeight = e.MarginBounds.Height;

            int yPos = marginTop;

            // Логотип/заголовок кинотеатра
            using (Font logoFont = new Font("Segoe UI", 14, FontStyle.Bold))
            {
                g.DrawString("🎬 КИНОТЕАТР", logoFont, new SolidBrush(Color.FromArgb(52, 73, 94)), marginLeft, yPos);
            }

            // Дата формирования справа
            using (Font dateFont = new Font("Segoe UI", 9))
            {
                string dateText = $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}";
                SizeF dateSize = g.MeasureString(dateText, dateFont);
                g.DrawString(dateText, dateFont, Brushes.Gray, marginLeft + pageWidth - dateSize.Width, yPos + 5);
            }
            yPos += 30;

            // Заголовок отчёта
            using (Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold))
            {
                g.DrawString(currentReportTitle, titleFont, Brushes.Black, marginLeft, yPos);
                yPos += 35;
            }

            // Подзаголовок (период)
            if (!string.IsNullOrEmpty(currentReportSubtitle))
            {
                using (Font subtitleFont = new Font("Segoe UI", 10))
                {
                    g.DrawString(currentReportSubtitle, subtitleFont, Brushes.Gray, marginLeft, yPos);
                    yPos += 25;
                }
            }

            // Линия
            g.DrawLine(new Pen(Color.FromArgb(52, 73, 94), 2), marginLeft, yPos, marginLeft + pageWidth, yPos);
            yPos += 15;

            // Таблица данных
            if (currentReportData != null && currentReportData.Rows.Count > 0)
            {
                DrawTable(g, marginLeft, yPos, pageWidth, pageHeight - (yPos - marginTop) - 30, e);
            }
            else
            {
                using (Font font = new Font("Segoe UI", 12, FontStyle.Italic))
                {
                    g.DrawString("Нет данных для отображения", font, Brushes.Gray, marginLeft, yPos);
                }
                e.HasMorePages = false;
            }
        }

        private void DrawTable(Graphics g, int x, int y, int width, int availableHeight, PrintPageEventArgs e)
        {
            int columnCount = currentReportData.Columns.Count;

            // Вычисляем ширину столбцов пропорционально содержимому
            int[] columnWidths = CalculateColumnWidths(g, width);

            int rowHeight = 22;
            int headerHeight = 30;

            Font headerFont = new Font("Segoe UI", 9, FontStyle.Bold);
            Font cellFont = new Font("Segoe UI", 8);

            int tableStartY = y;

            // Заголовки столбцов
            g.FillRectangle(new SolidBrush(Color.FromArgb(52, 73, 94)), x, y, width, headerHeight);

            int xOffset = x;
            for (int col = 0; col < columnCount; col++)
            {
                string header = currentReportData.Columns[col].ColumnName;
                RectangleF headerRect = new RectangleF(xOffset + 3, y, columnWidths[col] - 6, headerHeight);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(header, headerFont, Brushes.White, headerRect, sf);
                xOffset += columnWidths[col];
            }

            y += headerHeight;

            // Строки данных
            int startRow = currentPageIndex * rowsPerPage;
            int endRow = Math.Min(startRow + rowsPerPage, currentReportData.Rows.Count);
            int maxRows = (availableHeight - headerHeight) / rowHeight;
            endRow = Math.Min(endRow, startRow + maxRows);

            for (int row = startRow; row < endRow; row++)
            {
                Color rowColor = (row - startRow) % 2 == 0 ? Color.White : Color.FromArgb(248, 248, 248);
                g.FillRectangle(new SolidBrush(rowColor), x, y, width, rowHeight);

                xOffset = x;
                for (int col = 0; col < columnCount; col++)
                {
                    string cellValue = FormatCellValue(currentReportData.Rows[row][col], currentReportData.Columns[col].DataType);

                    RectangleF cellRect = new RectangleF(xOffset + 3, y, columnWidths[col] - 6, rowHeight);
                    StringFormat sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        Alignment = IsNumericColumn(currentReportData.Columns[col].DataType) ? StringAlignment.Far : StringAlignment.Near
                    };
                    g.DrawString(cellValue, cellFont, Brushes.Black, cellRect, sf);
                    xOffset += columnWidths[col];
                }

                g.DrawLine(new Pen(Color.FromArgb(220, 220, 220)), x, y + rowHeight, x + width, y + rowHeight);
                y += rowHeight;
            }

            // Рамка таблицы
            g.DrawRectangle(new Pen(Color.FromArgb(52, 73, 94), 1), x, tableStartY, width, y - tableStartY);

            // Вертикальные линии
            xOffset = x;
            for (int col = 0; col < columnCount - 1; col++)
            {
                xOffset += columnWidths[col];
                g.DrawLine(new Pen(Color.FromArgb(200, 200, 200)), xOffset, tableStartY, xOffset, y);
            }

            // Итоговая строка
            y += 10;
            using (Font summaryFont = new Font("Segoe UI", 9))
            {
                g.DrawString($"Всего записей: {currentReportData.Rows.Count}", summaryFont, Brushes.Gray, x, y);
            }

            // Номер страницы
            int totalPages = (int)Math.Ceiling((double)currentReportData.Rows.Count / Math.Max(maxRows, 1));
            using (Font pageFont = new Font("Segoe UI", 9))
            {
                string pageText = $"Страница {currentPageIndex + 1} из {Math.Max(totalPages, 1)}";
                SizeF textSize = g.MeasureString(pageText, pageFont);
                g.DrawString(pageText, pageFont, Brushes.Gray,
                    e.MarginBounds.Right - textSize.Width, e.MarginBounds.Bottom - 15);
            }

            // Проверка, есть ли ещё страницы
            currentPageIndex++;
            e.HasMorePages = (currentPageIndex * maxRows < currentReportData.Rows.Count);

            headerFont.Dispose();
            cellFont.Dispose();
        }

        private int[] CalculateColumnWidths(Graphics g, int totalWidth)
        {
            int columnCount = currentReportData.Columns.Count;
            int[] widths = new int[columnCount];
            Font measureFont = new Font("Segoe UI", 9);

            // Минимальная ширина
            int minWidth = 60;
            int maxWidth = 250;

            for (int col = 0; col < columnCount; col++)
            {
                // Ширина заголовка
                float headerWidth = g.MeasureString(currentReportData.Columns[col].ColumnName, measureFont).Width;

                // Максимальная ширина данных (проверяем первые 20 строк)
                float maxDataWidth = 0;
                for (int row = 0; row < Math.Min(20, currentReportData.Rows.Count); row++)
                {
                    string value = FormatCellValue(currentReportData.Rows[row][col], currentReportData.Columns[col].DataType);
                    float dataWidth = g.MeasureString(value, measureFont).Width;
                    if (dataWidth > maxDataWidth) maxDataWidth = dataWidth;
                }

                widths[col] = Math.Max(minWidth, Math.Min(maxWidth, (int)Math.Max(headerWidth, maxDataWidth) + 20));
            }

            // Нормализуем до общей ширины
            int totalCalculated = 0;
            foreach (int w in widths) totalCalculated += w;

            if (totalCalculated != totalWidth)
            {
                float ratio = (float)totalWidth / totalCalculated;
                for (int i = 0; i < widths.Length; i++)
                {
                    widths[i] = (int)(widths[i] * ratio);
                }
            }

            measureFont.Dispose();
            return widths;
        }

        private string FormatCellValue(object value, Type dataType)
        {
            if (value == null || value == DBNull.Value) return "";

            if (dataType == typeof(decimal) || dataType == typeof(double) || dataType == typeof(float))
            {
                if (decimal.TryParse(value.ToString(), out decimal numVal))
                {
                    return numVal.ToString("N0");
                }
            }
            else if (dataType == typeof(TimeSpan))
            {
                TimeSpan ts = (TimeSpan)value;
                return ts.ToString(@"hh\:mm");
            }

            return value.ToString();
        }

        private bool IsNumericColumn(Type dataType)
        {
            return dataType == typeof(int) || dataType == typeof(decimal) ||
                   dataType == typeof(double) || dataType == typeof(float) ||
                   dataType == typeof(long);
        }

        private string SaveReportAsImage(string filePath)
        {
            int width = 1400;
            int rowHeight = 24;
            int headerHeight = 130;
            int tableHeaderHeight = 32;

            int rowCount = currentReportData?.Rows.Count ?? 0;
            int height = headerHeight + tableHeaderHeight + rowCount * rowHeight + 80;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.Clear(Color.White);

                int yPos = 20;

                // Логотип
                using (Font logoFont = new Font("Segoe UI", 16, FontStyle.Bold))
                {
                    g.DrawString("🎬 КИНОТЕАТР", logoFont, new SolidBrush(Color.FromArgb(52, 73, 94)), 20, yPos);
                }

                // Дата
                using (Font dateFont = new Font("Segoe UI", 10))
                {
                    string dateText = $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}";
                    SizeF dateSize = g.MeasureString(dateText, dateFont);
                    g.DrawString(dateText, dateFont, Brushes.Gray, width - dateSize.Width - 20, yPos + 5);
                }
                yPos += 35;

                // Заголовок
                using (Font titleFont = new Font("Segoe UI", 18, FontStyle.Bold))
                {
                    g.DrawString(currentReportTitle, titleFont, Brushes.Black, 20, yPos);
                    yPos += 40;
                }

                // Подзаголовок
                if (!string.IsNullOrEmpty(currentReportSubtitle))
                {
                    using (Font subtitleFont = new Font("Segoe UI", 11))
                    {
                        g.DrawString(currentReportSubtitle, subtitleFont, Brushes.Gray, 20, yPos);
                        yPos += 30;
                    }
                }

                // Линия
                g.DrawLine(new Pen(Color.FromArgb(52, 73, 94), 3), 20, yPos, width - 20, yPos);
                yPos += 15;

                // Таблица
                if (currentReportData != null && currentReportData.Rows.Count > 0)
                {
                    int columnCount = currentReportData.Columns.Count;
                    int tableWidth = width - 40;
                    int[] columnWidths = CalculateColumnWidths(g, tableWidth);

                    Font headerFont = new Font("Segoe UI", 10, FontStyle.Bold);
                    Font cellFont = new Font("Segoe UI", 9);

                    // Заголовки
                    g.FillRectangle(new SolidBrush(Color.FromArgb(52, 73, 94)), 20, yPos, tableWidth, tableHeaderHeight);

                    int xOffset = 20;
                    for (int col = 0; col < columnCount; col++)
                    {
                        string header = currentReportData.Columns[col].ColumnName;
                        RectangleF rect = new RectangleF(xOffset + 3, yPos, columnWidths[col] - 6, tableHeaderHeight);
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(header, headerFont, Brushes.White, rect, sf);
                        xOffset += columnWidths[col];
                    }

                    yPos += tableHeaderHeight;
                    int tableStartY = yPos - tableHeaderHeight;

                    // Данные
                    for (int row = 0; row < currentReportData.Rows.Count; row++)
                    {
                        Color rowColor = row % 2 == 0 ? Color.White : Color.FromArgb(248, 248, 248);
                        g.FillRectangle(new SolidBrush(rowColor), 20, yPos, tableWidth, rowHeight);

                        xOffset = 20;
                        for (int col = 0; col < columnCount; col++)
                        {
                            string cellValue = FormatCellValue(currentReportData.Rows[row][col], currentReportData.Columns[col].DataType);

                            RectangleF rect = new RectangleF(xOffset + 3, yPos, columnWidths[col] - 6, rowHeight);
                            StringFormat sf = new StringFormat
                            {
                                LineAlignment = StringAlignment.Center,
                                Alignment = IsNumericColumn(currentReportData.Columns[col].DataType) ? StringAlignment.Far : StringAlignment.Near
                            };
                            g.DrawString(cellValue, cellFont, Brushes.Black, rect, sf);
                            xOffset += columnWidths[col];
                        }

                        g.DrawLine(new Pen(Color.FromArgb(220, 220, 220)), 20, yPos + rowHeight, width - 20, yPos + rowHeight);
                        yPos += rowHeight;
                    }

                    // Рамка
                    g.DrawRectangle(new Pen(Color.FromArgb(52, 73, 94), 2), 20, tableStartY, tableWidth, yPos - tableStartY);

                    // Вертикальные линии
                    xOffset = 20;
                    for (int col = 0; col < columnCount - 1; col++)
                    {
                        xOffset += columnWidths[col];
                        g.DrawLine(new Pen(Color.FromArgb(200, 200, 200)), xOffset, tableStartY, xOffset, yPos);
                    }

                    // Итого
                    yPos += 15;
                    using (Font summaryFont = new Font("Segoe UI", 10))
                    {
                        g.DrawString($"Всего записей: {currentReportData.Rows.Count}", summaryFont, Brushes.Gray, 20, yPos);
                    }

                    headerFont.Dispose();
                    cellFont.Dispose();
                }

                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return filePath;
        }

        #endregion

        #region Предпросмотр отчёта

        /// <summary>
        /// Показать предпросмотр отчёта
        /// </summary>
        public void ShowReportPreview(DataTable data, string reportTitle, string subtitle = "")
        {
            currentReportData = data;
            currentReportTitle = reportTitle;
            currentReportSubtitle = subtitle;
            currentPageIndex = 0;

            using (PrintDocument printDoc = new PrintDocument())
            {
                printDoc.DefaultPageSettings.Landscape = true;
                printDoc.PrintPage += PrintDocument_PrintPage;

                using (PrintPreviewDialog previewDialog = new PrintPreviewDialog())
                {
                    previewDialog.Document = printDoc;
                    previewDialog.Width = 1200;
                    previewDialog.Height = 800;
                    previewDialog.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Печать отчёта
        /// </summary>
        public void PrintReport(DataTable data, string reportTitle, string subtitle = "")
        {
            currentReportData = data;
            currentReportTitle = reportTitle;
            currentReportSubtitle = subtitle;
            currentPageIndex = 0;

            using (PrintDocument printDoc = new PrintDocument())
            {
                printDoc.DefaultPageSettings.Landscape = true;
                printDoc.PrintPage += PrintDocument_PrintPage;

                using (PrintDialog printDialog = new PrintDialog())
                {
                    printDialog.Document = printDoc;
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            printDoc.Print();
                            MessageBox.Show("Отчёт отправлен на печать!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка печати: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        #endregion
    }
}