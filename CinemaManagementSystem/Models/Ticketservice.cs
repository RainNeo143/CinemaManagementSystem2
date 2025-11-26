using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CinemaManagementSystem.Models;

namespace CinemaManagementSystem.Services
{
    /// <summary>
    /// Сервис генерации билетов в PDF формате
    /// Использует создание изображения билета и сохранение через PrintDocument
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

                // Фон билета
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
                    string filmTitle = ticket.FilmTitle;
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
                    g.DrawString(ticket.HallName, valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol + 270, startY + 15);

                    // Ряд
                    g.DrawString("РЯД", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol, startY + lineHeight + 10);
                    g.DrawString(ticket.Row.ToString(), valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol, startY + lineHeight + 25);

                    // Место
                    g.DrawString("МЕСТО", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol + 80, startY + lineHeight + 10);
                    g.DrawString(ticket.SeatNumber.ToString(), valueFont, new SolidBrush(Color.FromArgb(44, 62, 80)), leftCol + 80, startY + lineHeight + 25);

                    // Тип места
                    g.DrawString("ТИП", labelFont, new SolidBrush(Color.FromArgb(149, 165, 166)), leftCol + 160, startY + lineHeight + 10);
                    Color seatTypeColor = ticket.SeatType == "VIP" ? Color.Gold : Color.FromArgb(44, 62, 80);
                    g.DrawString(ticket.SeatType, valueFont, new SolidBrush(seatTypeColor), leftCol + 160, startY + lineHeight + 25);
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

                // QR код (заглушка - квадрат)
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
                    g.DrawString($"Покупатель: {ticket.BuyerName}", smallFont, new SolidBrush(Color.FromArgb(127, 140, 141)), 50, TICKET_HEIGHT - 55);
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
        /// Показывает диалог сохранения и сохраняет билет
        /// </summary>
        public string SaveTicketWithDialog(TicketInfo ticket)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Все файлы|*.*";
                saveDialog.Title = "Сохранить билет";
                saveDialog.FileName = $"Билет_{ticket.TicketNumber}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (Bitmap bitmap = GenerateTicketImage(ticket))
                    {
                        ImageFormat format = ImageFormat.Png;
                        if (saveDialog.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                            format = ImageFormat.Jpeg;

                        bitmap.Save(saveDialog.FileName, format);
                        return saveDialog.FileName;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Показывает предпросмотр билета
        /// </summary>
        public void ShowTicketPreview(TicketInfo ticket)
        {
            using (Bitmap bitmap = GenerateTicketImage(ticket))
            {
                Form previewForm = new Form
                {
                    Text = $"Билет {ticket.TicketNumber}",
                    ClientSize = new Size(TICKET_WIDTH + 40, TICKET_HEIGHT + 100),
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

                Button btnSave = new Button
                {
                    Text = "💾 Сохранить билет",
                    Location = new Point(20, TICKET_HEIGHT + 35),
                    Size = new Size(150, 40),
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSave.Click += (s, e) =>
                {
                    string savedPath = SaveTicketWithDialog(ticket);
                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        MessageBox.Show($"Билет сохранён:\n{savedPath}", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                Button btnPrint = new Button
                {
                    Text = "🖨️ Печать",
                    Location = new Point(180, TICKET_HEIGHT + 35),
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

                previewForm.Controls.AddRange(new Control[] { pictureBox, btnSave, btnPrint, btnClose });
                previewForm.ShowDialog();
            }
        }

        /// <summary>
        /// Печатает билет
        /// </summary>
        public void PrintTicket(TicketInfo ticket)
        {
            using (System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument())
            {
                printDoc.PrintPage += (sender, e) =>
                {
                    using (Bitmap bitmap = GenerateTicketImage(ticket))
                    {
                        // Центрируем билет на странице
                        float x = (e.PageBounds.Width - bitmap.Width) / 2;
                        float y = (e.PageBounds.Height - bitmap.Height) / 2;
                        e.Graphics.DrawImage(bitmap, x, y);
                    }
                };

                using (System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog())
                {
                    printDialog.Document = printDoc;
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        printDoc.Print();
                    }
                }
            }
        }
    }
}