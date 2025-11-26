using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using CinemaManagementSystem.Models;

namespace CinemaManagementSystem.Services
{
    /// <summary>
    /// Сервис генерации билетов
    /// Поддерживает PNG, PDF (через виртуальный принтер) и печать
    /// </summary>
    public class TicketService
    {
        private const int TICKET_WIDTH = 800;
        private const int TICKET_HEIGHT = 400;

        /// <summary>
        /// Генерирует изображение билета
        /// </summary>
        public Bitmap GenerateTicketImage(TicketInfo ticket)
        {
            Bitmap bitmap = new Bitmap(TICKET_WIDTH, TICKET_HEIGHT);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Фон билета - градиент
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, TICKET_WIDTH, TICKET_HEIGHT),
                    Color.FromArgb(41, 128, 185),
                    Color.FromArgb(52, 73, 94),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 0, 0, TICKET_WIDTH, TICKET_HEIGHT);
                }

                // Белая область для информации
                g.FillRectangle(Brushes.White, 20, 20, TICKET_WIDTH - 40, TICKET_HEIGHT - 40);

                // Декоративная полоса слева
                g.FillRectangle(new SolidBrush(Color.FromArgb(231, 76, 60)), 20, 20, 10, TICKET_HEIGHT - 40);

                // Заголовок "КИНОТЕАТР"
                using (Font titleFont = new Font("Segoe UI", 24, FontStyle.Bold))
                {
                    g.DrawString("🎬 КИНОТЕАТР", titleFont, new SolidBrush(Color.FromArgb(52, 73, 94)), 50, 35);
                }

                // Номер билета
                using (Font ticketNumFont = new Font("Consolas", 12, FontStyle.Bold))
                {
                    g.DrawString($"№ {ticket.TicketNumber}", ticketNumFont, new SolidBrush(Color.FromArgb(149, 165, 166)), 550, 40);
                }

                // Линия разделения
                using (Pen pen = new Pen(Color.FromArgb(189, 195, 199), 2))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawLine(pen, 50, 80, TICKET_WIDTH - 50, 80);
                }

                // Название фильма
                using (Font filmFont = new Font("Segoe UI", 18, FontStyle.Bold))
                {
                    string filmTitle = ticket.FilmTitle ?? "Фильм";
                    if (filmTitle.Length > 35)
                        filmTitle = filmTitle.Substring(0, 32) + "...";
                    g.DrawString(filmTitle, filmFont, new SolidBrush(Color.FromArgb(44, 62, 80)), 50, 95);
                }

                // Жанр и возрастное ограничение
                using (Font infoFont = new Font("Segoe UI", 10))
                {
                    string genreInfo = "";
                    if (!string.IsNullOrEmpty(ticket.Genre))
                        genreInfo = ticket.Genre;
                    if (!string.IsNullOrEmpty(ticket.AgeRating))
                        genreInfo += (genreInfo.Length > 0 ? " • " : "") + ticket.AgeRating;
                    if (ticket.Duration > 0)
                        genreInfo += (genreInfo.Length > 0 ? " • " : "") + $"{ticket.Duration} мин";

                    g.DrawString(genreInfo, infoFont, new SolidBrush(Color.FromArgb(127, 140, 141)), 50, 130);
                }

                // Информация о сеансе (левая колонка)
                int leftCol = 50;
                int rightCol = 400;
                int startY = 165;
                int lineHeight = 35;

                using (Font labelFont = new Font("Segoe UI", 9))
                using (Font valueFont = new Font("Segoe UI", 11, FontStyle.Bold))
                {
                    // Дата
                    g.DrawString("ДАТА", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol, startY);
                    g.DrawString(ticket.SessionDate.ToString("dd MMMM yyyy"), valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol, startY + 15);

                    // Время
                    g.DrawString("ВРЕМЯ", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol + 150, startY);
                    g.DrawString(ticket.StartTime.ToString(@"hh\:mm"), valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol + 150, startY + 15);

                    // Зал
                    g.DrawString("ЗАЛ", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol + 270, startY);
                    g.DrawString(ticket.HallName ?? "1", valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol + 270, startY + 15);

                    // Ряд
                    g.DrawString("РЯД", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol, startY + lineHeight + 10);
                    g.DrawString(ticket.Row.ToString(), valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol, startY + lineHeight + 25);

                    // Место
                    g.DrawString("МЕСТО", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol + 80, startY + lineHeight + 10);
                    g.DrawString(ticket.SeatNumber.ToString(), valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol + 80, startY + lineHeight + 25);

                    // Тип места
                    g.DrawString("ТИП", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol + 160, startY + lineHeight + 10);
                    Color seatTypeColor = ticket.SeatType == "VIP" ? Color.Gold : Color.FromArgb(44, 62, 80);
                    g.DrawString(ticket.SeatType ?? "Обычное", valueFont, new SolidBrush(seatTypeColor), leftCol + 160, startY + lineHeight + 25);
                }

                // Цена (большая, справа)
                using (Font priceLabel = new Font("Segoe UI", 10))
                using (Font priceFont = new Font("Segoe UI", 22, FontStyle.Bold))
                {
                    g.DrawString("ЦЕНА", priceLabel, new SolidBrush(Color.FromArgb(149, 165, 166)), rightCol + 100, startY);
                    g.DrawString($"{ticket.Amount:N0} ₸", priceFont, new SolidBrush(Color.FromArgb(46, 204, 113)), rightCol + 100, startY + 20);
                }

                // Линия отрыва
                using (Pen tearPen = new Pen(Color.FromArgb(189, 195, 199), 1))
                {
                    tearPen.DashStyle = DashStyle.Dot;
                    g.DrawLine(tearPen, rightCol + 80, 100, rightCol + 80, TICKET_HEIGHT - 60);
                }

                // QR код (заглушка - квадрат с паттерном)
                g.FillRectangle(Brushes.White, rightCol + 100, startY + lineHeight + 50, 100, 100);
                g.DrawRectangle(new Pen(Color.FromArgb(44, 62, 80), 2), rightCol + 100, startY + lineHeight + 50, 100, 100);

                // Имитация QR кода
                Random rnd = new Random(ticket.BookingId);
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (rnd.Next(2) == 1)
                        {
                            g.FillRectangle(Brushes.Black, rightCol + 105 + i * 9, startY + lineHeight + 55 + j * 9, 8, 8);
                        }
                    }
                }

                // Нижняя информация
                using (Font smallFont = new Font("Segoe UI", 8))
                {
                    g.DrawString($"Покупатель: {ticket.BuyerName ?? "Гость"}", smallFont, new SolidBrush(Color.FromArgb(127, 140, 141)), 50, TICKET_HEIGHT - 55);
                    g.DrawString($"Дата покупки: {ticket.BookingDate:dd.MM.yyyy HH:mm}", smallFont, new SolidBrush(Color.FromArgb(127, 140, 141)), 50, TICKET_HEIGHT - 40);
                    g.DrawString("Сохраните билет до окончания сеанса", smallFont, new SolidBrush(Color.FromArgb(149, 165, 166)), rightCol - 50, TICKET_HEIGHT - 40);
                }

                // Рамка
                g.DrawRectangle(new Pen(Color.FromArgb(52, 73, 94), 3), 20, 20, TICKET_WIDTH - 40, TICKET_HEIGHT - 40);
            }

            return bitmap;
        }

        /// <summary>
        /// Сохраняет билет как изображение PNG
        /// </summary>
        public string SaveTicketAsImage(TicketInfo ticket, string folderPath = null)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            string fileName = $"Билет_{ticket.TicketNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(folderPath, fileName);

            using (Bitmap bitmap = GenerateTicketImage(ticket))
            {
                bitmap.Save(filePath, ImageFormat.Png);
            }

            return filePath;
        }

        /// <summary>
        /// Показывает диалог сохранения и сохраняет билет (PNG или PDF)
        /// </summary>
        public string SaveTicketWithDialog(TicketInfo ticket)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|PDF Document|*.pdf|Все файлы|*.*";
                saveDialog.Title = "Сохранить билет";
                saveDialog.FileName = $"Билет_{ticket.TicketNumber}";
                saveDialog.FilterIndex = 1;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string extension = Path.GetExtension(saveDialog.FileName).ToLower();

                    if (extension == ".pdf")
                    {
                        // Сохранение PDF через печать
                        return SaveTicketAsPdf(ticket, saveDialog.FileName);
                    }
                    else
                    {
                        // Сохранение как изображение
                        using (Bitmap bitmap = GenerateTicketImage(ticket))
                        {
                            ImageFormat format = ImageFormat.Png;
                            if (extension == ".jpg" || extension == ".jpeg")
                                format = ImageFormat.Jpeg;

                            bitmap.Save(saveDialog.FileName, format);
                            return saveDialog.FileName;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Сохраняет билет как PDF (используя Microsoft Print to PDF)
        /// </summary>
        public string SaveTicketAsPdf(TicketInfo ticket, string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                filePath = Path.Combine(folder, $"Билет_{ticket.TicketNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }

            try
            {
                // Создаём документ для печати
                using (PrintDocument printDoc = new PrintDocument())
                {
                    // Настраиваем печать в PDF
                    printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                    printDoc.PrinterSettings.PrintToFile = true;
                    printDoc.PrinterSettings.PrintFileName = filePath;

                    // Проверяем доступен ли принтер
                    if (!printDoc.PrinterSettings.IsValid)
                    {
                        // Если Microsoft Print to PDF недоступен, сохраняем как PNG
                        MessageBox.Show("PDF принтер недоступен. Билет будет сохранён как изображение PNG.",
                            "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        string pngPath = Path.ChangeExtension(filePath, ".png");
                        return SaveTicketAsImage(ticket, Path.GetDirectoryName(pngPath));
                    }

                    bool printed = false;

                    printDoc.PrintPage += (sender, e) =>
                    {
                        if (!printed)
                        {
                            using (Bitmap bitmap = GenerateTicketImage(ticket))
                            {
                                // Масштабируем билет для страницы
                                float scale = Math.Min(
                                    (float)e.MarginBounds.Width / bitmap.Width,
                                    (float)e.MarginBounds.Height / bitmap.Height
                                );

                                int newWidth = (int)(bitmap.Width * scale);
                                int newHeight = (int)(bitmap.Height * scale);

                                int x = e.MarginBounds.Left + (e.MarginBounds.Width - newWidth) / 2;
                                int y = e.MarginBounds.Top;

                                e.Graphics.DrawImage(bitmap, x, y, newWidth, newHeight);
                            }
                            printed = true;
                            e.HasMorePages = false;
                        }
                    };

                    printDoc.Print();
                }

                return filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания PDF: {ex.Message}\nБилет будет сохранён как PNG.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                string pngPath = Path.ChangeExtension(filePath, ".png");
                return SaveTicketAsImage(ticket, Path.GetDirectoryName(pngPath));
            }
        }

        /// <summary>
        /// Показывает предпросмотр билета с возможностью сохранения
        /// </summary>
        public void ShowTicketPreview(TicketInfo ticket)
        {
            using (Bitmap bitmap = GenerateTicketImage(ticket))
            {
                Form previewForm = new Form
                {
                    Text = $"Билет {ticket.TicketNumber}",
                    ClientSize = new Size(TICKET_WIDTH + 40, TICKET_HEIGHT + 120),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    BackColor = Color.FromArgb(236, 240, 241)
                };

                PictureBox pictureBox = new PictureBox
                {
                    Image = new Bitmap(bitmap),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(20, 20),
                    Size = new Size(TICKET_WIDTH, TICKET_HEIGHT),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Кнопка "Сохранить как PNG"
                Button btnSavePng = new Button
                {
                    Text = "💾 Сохранить PNG",
                    Location = new Point(20, TICKET_HEIGHT + 35),
                    Size = new Size(140, 40),
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSavePng.Click += (s, e) =>
                {
                    using (SaveFileDialog dlg = new SaveFileDialog())
                    {
                        dlg.Filter = "PNG Image|*.png";
                        dlg.FileName = $"Билет_{ticket.TicketNumber}";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            using (Bitmap bmp = GenerateTicketImage(ticket))
                            {
                                bmp.Save(dlg.FileName, ImageFormat.Png);
                            }
                            MessageBox.Show($"Билет сохранён:\n{dlg.FileName}", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };

                // Кнопка "Сохранить как PDF"
                Button btnSavePdf = new Button
                {
                    Text = "📄 Сохранить PDF",
                    Location = new Point(170, TICKET_HEIGHT + 35),
                    Size = new Size(140, 40),
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSavePdf.Click += (s, e) =>
                {
                    using (SaveFileDialog dlg = new SaveFileDialog())
                    {
                        dlg.Filter = "PDF Document|*.pdf";
                        dlg.FileName = $"Билет_{ticket.TicketNumber}";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            string savedPath = SaveTicketAsPdf(ticket, dlg.FileName);
                            if (!string.IsNullOrEmpty(savedPath))
                            {
                                MessageBox.Show($"Билет сохранён:\n{savedPath}", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                };

                // Кнопка "Печать"
                Button btnPrint = new Button
                {
                    Text = "🖨️ Печать",
                    Location = new Point(320, TICKET_HEIGHT + 35),
                    Size = new Size(120, 40),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnPrint.Click += (s, e) =>
                {
                    PrintTicket(ticket);
                };

                // Кнопка "Закрыть"
                Button btnClose = new Button
                {
                    Text = "Закрыть",
                    Location = new Point(TICKET_WIDTH - 80, TICKET_HEIGHT + 35),
                    Size = new Size(100, 40),
                    BackColor = Color.FromArgb(149, 165, 166),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnClose.Click += (s, e) => previewForm.Close();

                // Информация о билете
                Label lblInfo = new Label
                {
                    Text = $"Фильм: {ticket.FilmTitle}  |  Дата: {ticket.SessionDate:dd.MM.yyyy}  |  " +
                           $"Ряд: {ticket.Row}, Место: {ticket.SeatNumber}  |  Цена: {ticket.Amount:N0} ₸",
                    Location = new Point(20, TICKET_HEIGHT + 85),
                    Size = new Size(TICKET_WIDTH, 25),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(52, 73, 94)
                };

                previewForm.Controls.AddRange(new Control[] { pictureBox, btnSavePng, btnSavePdf, btnPrint, btnClose, lblInfo });
                previewForm.ShowDialog();
            }
        }

        /// <summary>
        /// Печатает билет на выбранном принтере
        /// </summary>
        public void PrintTicket(TicketInfo ticket)
        {
            using (PrintDocument printDoc = new PrintDocument())
            {
                bool printed = false;

                printDoc.PrintPage += (sender, e) =>
                {
                    if (!printed)
                    {
                        using (Bitmap bitmap = GenerateTicketImage(ticket))
                        {
                            // Центрируем и масштабируем билет
                            float scale = Math.Min(
                                (float)e.MarginBounds.Width / bitmap.Width,
                                (float)e.MarginBounds.Height / bitmap.Height
                            );

                            int newWidth = (int)(bitmap.Width * scale);
                            int newHeight = (int)(bitmap.Height * scale);

                            int x = e.MarginBounds.Left + (e.MarginBounds.Width - newWidth) / 2;
                            int y = e.MarginBounds.Top;

                            e.Graphics.DrawImage(bitmap, x, y, newWidth, newHeight);
                        }
                        printed = true;
                        e.HasMorePages = false;
                    }
                };

                using (PrintDialog printDialog = new PrintDialog())
                {
                    printDialog.Document = printDoc;
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            printDoc.Print();
                            MessageBox.Show("Билет отправлен на печать!", "Успех",
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

        /// <summary>
        /// Быстрое сохранение билета в папку Документы
        /// </summary>
        public string QuickSaveTicket(TicketInfo ticket, string format = "png")
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string subFolder = Path.Combine(folder, "Билеты кинотеатра");

            // Создаём подпапку если не существует
            if (!Directory.Exists(subFolder))
            {
                Directory.CreateDirectory(subFolder);
            }

            string fileName = $"Билет_{ticket.TicketNumber}_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (format.ToLower() == "pdf")
            {
                string filePath = Path.Combine(subFolder, fileName + ".pdf");
                return SaveTicketAsPdf(ticket, filePath);
            }
            else
            {
                string filePath = Path.Combine(subFolder, fileName + ".png");
                using (Bitmap bitmap = GenerateTicketImage(ticket))
                {
                    bitmap.Save(filePath, ImageFormat.Png);
                }
                return filePath;
            }
        }
    }
}