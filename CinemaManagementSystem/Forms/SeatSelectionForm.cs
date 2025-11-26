using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CinemaManagementSystem.Models;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    public partial class SeatSelectionForm : Form
    {
        private readonly int userId;
        private readonly int sessionId;
        private readonly BookingService bookingService;
        private readonly TicketService ticketService;
        private readonly AuthService authService;
        private Panel seatPanel;
        private Button selectedSeatButton;
        private Label lblInfo;
        private Label lblSelectedInfo;
        private Label lblBalance;
        private Button btnConfirm;
        private Button btnCancel;
        private Panel legendPanel;
        private decimal ticketPrice = 0;
        private decimal userBalance = 0;
        private string selectedSeatType = "";
        private int selectedRow = 0;
        private int selectedSeat = 0;

        public int LastBookingId { get; private set; }

        public SeatSelectionForm(int userId, int sessionId)
        {
            this.userId = userId;
            this.sessionId = sessionId;
            this.bookingService = new BookingService();
            this.ticketService = new TicketService();
            this.authService = new AuthService();
            InitializeComponent();
            LoadUserBalance();
            LoadSessionInfo();
            LoadSeats();
        }

        private void InitializeComponent()
        {
            this.seatPanel = new Panel();
            this.lblInfo = new Label();
            this.lblSelectedInfo = new Label();
            this.lblBalance = new Label();
            this.btnConfirm = new Button();
            this.btnCancel = new Button();
            this.legendPanel = new Panel();

            this.SuspendLayout();

            // lblInfo
            this.lblInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblInfo.ForeColor = Color.FromArgb(52, 73, 94);
            this.lblInfo.Location = new Point(20, 15);
            this.lblInfo.Size = new Size(600, 30);
            this.lblInfo.Text = "🎬 Выберите место в зале";

            // lblBalance
            this.lblBalance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lblBalance.ForeColor = Color.FromArgb(46, 204, 113);
            this.lblBalance.Location = new Point(700, 15);
            this.lblBalance.Size = new Size(220, 30);
            this.lblBalance.TextAlign = ContentAlignment.MiddleRight;
            this.lblBalance.Text = "💰 Баланс: 0 ₸";

            // legendPanel
            this.legendPanel.Location = new Point(20, 50);
            this.legendPanel.Size = new Size(900, 50);
            this.legendPanel.BackColor = Color.White;
            this.legendPanel.BorderStyle = BorderStyle.FixedSingle;
            CreateLegend();

            // seatPanel
            this.seatPanel.Location = new Point(20, 110);
            this.seatPanel.Size = new Size(900, 450);
            this.seatPanel.AutoScroll = true;
            this.seatPanel.BackColor = Color.FromArgb(236, 240, 241);
            this.seatPanel.BorderStyle = BorderStyle.FixedSingle;

            // lblSelectedInfo
            this.lblSelectedInfo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lblSelectedInfo.ForeColor = Color.FromArgb(41, 128, 185);
            this.lblSelectedInfo.Location = new Point(20, 570);
            this.lblSelectedInfo.Size = new Size(600, 30);
            this.lblSelectedInfo.Text = "👆 Выберите место на схеме зала";

            // btnConfirm
            this.btnConfirm.Location = new Point(640, 565);
            this.btnConfirm.Size = new Size(140, 40);
            this.btnConfirm.Text = "💳 Купить билет";
            this.btnConfirm.BackColor = Color.FromArgb(149, 165, 166);
            this.btnConfirm.ForeColor = Color.White;
            this.btnConfirm.FlatStyle = FlatStyle.Flat;
            this.btnConfirm.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnConfirm.Cursor = Cursors.Hand;
            this.btnConfirm.Enabled = false;
            this.btnConfirm.Click += btnConfirm_Click;

            // btnCancel
            this.btnCancel.Location = new Point(790, 565);
            this.btnCancel.Size = new Size(130, 40);
            this.btnCancel.Text = "✕ Отмена";
            this.btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnCancel.Cursor = Cursors.Hand;
            this.btnCancel.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // Form
            this.ClientSize = new Size(940, 620);
            this.BackColor = Color.White;
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblBalance);
            this.Controls.Add(this.legendPanel);
            this.Controls.Add(this.seatPanel);
            this.Controls.Add(this.lblSelectedInfo);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SeatSelectionForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Выбор места и покупка билета";
            this.Icon = SystemIcons.Information;
            this.ResumeLayout(false);
        }

        private void CreateLegend()
        {
            int xPos = 20;

            Panel box1 = CreateLegendBox(Color.FromArgb(189, 195, 199), xPos);
            Label lbl1 = CreateLegendLabel("Свободно", xPos + 35);
            legendPanel.Controls.Add(box1);
            legendPanel.Controls.Add(lbl1);
            xPos += 130;

            Panel box2 = CreateLegendBox(Color.Gold, xPos);
            Label lbl2 = CreateLegendLabel("VIP", xPos + 35);
            legendPanel.Controls.Add(box2);
            legendPanel.Controls.Add(lbl2);
            xPos += 80;

            Panel box3 = CreateLegendBox(Color.FromArgb(231, 76, 60), xPos);
            Label lbl3 = CreateLegendLabel("Занято", xPos + 35);
            legendPanel.Controls.Add(box3);
            legendPanel.Controls.Add(lbl3);
            xPos += 110;

            Panel box4 = CreateLegendBox(Color.FromArgb(46, 204, 113), xPos);
            Label lbl4 = CreateLegendLabel("Моё", xPos + 35);
            legendPanel.Controls.Add(box4);
            legendPanel.Controls.Add(lbl4);
            xPos += 90;

            Panel box5 = CreateLegendBox(Color.FromArgb(52, 152, 219), xPos);
            Label lbl5 = CreateLegendLabel("Выбрано", xPos + 35);
            legendPanel.Controls.Add(box5);
            legendPanel.Controls.Add(lbl5);
        }

        private Panel CreateLegendBox(Color color, int x)
        {
            return new Panel
            {
                Size = new Size(25, 25),
                Location = new Point(x, 12),
                BackColor = color,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Label CreateLegendLabel(string text, int x)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 15),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9F)
            };
        }

        private void LoadUserBalance()
        {
            try
            {
                userBalance = authService.GetUserBalance(userId);
                lblBalance.Text = $"💰 Баланс: {userBalance:N0} ₸";
            }
            catch
            {
                userBalance = 20000;
                lblBalance.Text = $"💰 Баланс: {userBalance:N0} ₸";
            }
        }

        private void LoadSessionInfo()
        {
            try
            {
                ticketPrice = bookingService.GetTicketPrice(sessionId);
            }
            catch
            {
                ticketPrice = 2500;
            }
        }

        private void LoadSeats()
        {
            try
            {
                // ИСПРАВЛЕНО: Используем правильный метод с учетом бронирований пользователя
                DataTable seats = bookingService.GetAvailableSeatsWithUserBookings(sessionId, userId);

                if (seats.Rows.Count == 0)
                {
                    CreateVirtualSeats();
                    return;
                }

                int maxRow = 0;
                int maxSeat = 0;

                foreach (DataRow row in seats.Rows)
                {
                    int rowNum = Convert.ToInt32(row["Ряд"]);
                    int seatNum = Convert.ToInt32(row["Номер_места"]);
                    if (rowNum > maxRow) maxRow = rowNum;
                    if (seatNum > maxSeat) maxSeat = seatNum;
                }

                CreateSeatsFromData(seats, maxRow, maxSeat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мест: {ex.Message}\n\nБудут созданы виртуальные места.",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CreateVirtualSeats();
            }
        }

        private void CreateSeatsFromData(DataTable seats, int maxRow, int maxSeat)
        {
            int seatWidth = 45;
            int seatHeight = 40;
            int horizontalSpacing = 10;
            int verticalSpacing = 10;

            // Экран
            int screenWidth = Math.Min(maxSeat * (seatWidth + horizontalSpacing), 850);
            Panel screen = new Panel
            {
                Location = new Point((900 - screenWidth) / 2, 20),
                Size = new Size(screenWidth, 40),
                BackColor = Color.FromArgb(52, 73, 94)
            };
            Label lblScreen = new Label
            {
                Text = "🎬 ЭКРАН",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            };
            screen.Controls.Add(lblScreen);
            seatPanel.Controls.Add(screen);

            int startX = (900 - (maxSeat * (seatWidth + horizontalSpacing))) / 2;
            if (startX < 80) startX = 80;

            // ИСПРАВЛЕНО: Правильная обработка статуса мест
            foreach (DataRow row in seats.Rows)
            {
                int rowNum = Convert.ToInt32(row["Ряд"]);
                int seatNum = Convert.ToInt32(row["Номер_места"]);
                string status = row["Статус_места"].ToString();
                string seatType = row["Тип_места"] != DBNull.Value ? row["Тип_места"].ToString() : "Обычное";

                Button seatButton = new Button
                {
                    Size = new Size(seatWidth, seatHeight),
                    Location = new Point(
                        startX + (seatNum - 1) * (seatWidth + horizontalSpacing),
                        75 + rowNum * (seatHeight + verticalSpacing)
                    ),
                    Text = seatNum.ToString(),
                    Tag = $"{rowNum}|{seatNum}|{seatType}|{status}",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };

                seatButton.FlatAppearance.BorderSize = 2;
                seatButton.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94);

                // ИСПРАВЛЕНО: Правильное применение цветов в зависимости от статуса
                if (status == "Занято")
                {
                    // КРАСНОЕ - место занято другим пользователем
                    seatButton.BackColor = Color.FromArgb(231, 76, 60);
                    seatButton.ForeColor = Color.White;
                    seatButton.Enabled = false;
                    seatButton.Cursor = Cursors.No;
                    seatButton.Text = "✕";
                }
                else if (status == "МоёБронирование")
                {
                    // ЗЕЛЁНОЕ - забронировано текущим пользователем
                    seatButton.BackColor = Color.FromArgb(46, 204, 113);
                    seatButton.ForeColor = Color.White;
                    seatButton.Enabled = false;
                    seatButton.Text = "✓";

                    ToolTip tip = new ToolTip();
                    tip.SetToolTip(seatButton, "Это место уже забронировано вами");
                }
                else
                {
                    // Свободное место
                    if (seatType == "VIP")
                    {
                        seatButton.BackColor = Color.Gold;
                        seatButton.ForeColor = Color.FromArgb(52, 73, 94);
                    }
                    else
                    {
                        seatButton.BackColor = Color.FromArgb(189, 195, 199);
                        seatButton.ForeColor = Color.FromArgb(52, 73, 94);
                    }

                    seatButton.Click += SeatButton_Click;
                    AddHoverEffect(seatButton, seatType);
                }

                // Номер ряда
                if (seatNum == 1)
                {
                    Label lblRow = new Label
                    {
                        Text = $"Ряд {rowNum}",
                        Location = new Point(10, 82 + rowNum * (seatHeight + verticalSpacing)),
                        Size = new Size(60, 30),
                        TextAlign = ContentAlignment.MiddleRight,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(52, 73, 94)
                    };
                    seatPanel.Controls.Add(lblRow);
                }

                seatPanel.Controls.Add(seatButton);
            }
        }

        private void CreateVirtualSeats()
        {
            int rows = 8;
            int seatsPerRow = 10;
            int seatWidth = 45;
            int seatHeight = 40;
            int horizontalSpacing = 10;
            int verticalSpacing = 10;

            int screenWidth = seatsPerRow * (seatWidth + horizontalSpacing);
            Panel screen = new Panel
            {
                Location = new Point((900 - screenWidth) / 2, 20),
                Size = new Size(screenWidth, 40),
                BackColor = Color.FromArgb(52, 73, 94)
            };
            Label lblScreen = new Label
            {
                Text = "🎬 ЭКРАН",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            };
            screen.Controls.Add(lblScreen);
            seatPanel.Controls.Add(screen);

            int startX = (900 - (seatsPerRow * (seatWidth + horizontalSpacing))) / 2;
            if (startX < 80) startX = 80;

            for (int rowNum = 1; rowNum <= rows; rowNum++)
            {
                for (int seatNum = 1; seatNum <= seatsPerRow; seatNum++)
                {
                    string seatType = rowNum >= rows - 1 ? "VIP" : "Обычное";

                    Button seatButton = new Button
                    {
                        Size = new Size(seatWidth, seatHeight),
                        Location = new Point(
                            startX + (seatNum - 1) * (seatWidth + horizontalSpacing),
                            75 + rowNum * (seatHeight + verticalSpacing)
                        ),
                        Text = seatNum.ToString(),
                        Tag = $"{rowNum}|{seatNum}|{seatType}|Свободно",
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };

                    seatButton.FlatAppearance.BorderSize = 2;
                    seatButton.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94);

                    if (seatType == "VIP")
                    {
                        seatButton.BackColor = Color.Gold;
                        seatButton.ForeColor = Color.FromArgb(52, 73, 94);
                    }
                    else
                    {
                        seatButton.BackColor = Color.FromArgb(189, 195, 199);
                        seatButton.ForeColor = Color.FromArgb(52, 73, 94);
                    }

                    seatButton.Click += SeatButton_Click;
                    AddHoverEffect(seatButton, seatType);

                    if (seatNum == 1)
                    {
                        Label lblRow = new Label
                        {
                            Text = $"Ряд {rowNum}",
                            Location = new Point(10, 82 + rowNum * (seatHeight + verticalSpacing)),
                            Size = new Size(60, 30),
                            TextAlign = ContentAlignment.MiddleRight,
                            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                            ForeColor = Color.FromArgb(52, 73, 94)
                        };
                        seatPanel.Controls.Add(lblRow);
                    }

                    seatPanel.Controls.Add(seatButton);
                }
            }
        }

        private void AddHoverEffect(Button seatButton, string seatType)
        {
            seatButton.MouseEnter += (s, e) => {
                if (seatButton != selectedSeatButton)
                {
                    seatButton.BackColor = Color.FromArgb(52, 152, 219);
                    seatButton.ForeColor = Color.White;
                }
            };
            seatButton.MouseLeave += (s, e) => {
                if (seatButton != selectedSeatButton)
                {
                    if (seatType == "VIP")
                    {
                        seatButton.BackColor = Color.Gold;
                        seatButton.ForeColor = Color.FromArgb(52, 73, 94);
                    }
                    else
                    {
                        seatButton.BackColor = Color.FromArgb(189, 195, 199);
                        seatButton.ForeColor = Color.FromArgb(52, 73, 94);
                    }
                }
            };
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            if (selectedSeatButton != null && selectedSeatButton != clickedButton)
            {
                string oldInfo = selectedSeatButton.Tag.ToString();
                string oldType = oldInfo.Split('|')[2];
                if (oldType == "VIP")
                {
                    selectedSeatButton.BackColor = Color.Gold;
                    selectedSeatButton.ForeColor = Color.FromArgb(52, 73, 94);
                }
                else
                {
                    selectedSeatButton.BackColor = Color.FromArgb(189, 195, 199);
                    selectedSeatButton.ForeColor = Color.FromArgb(52, 73, 94);
                }
            }

            selectedSeatButton = clickedButton;
            selectedSeatButton.BackColor = Color.FromArgb(52, 152, 219);
            selectedSeatButton.ForeColor = Color.White;

            string[] seatInfo = selectedSeatButton.Tag.ToString().Split('|');
            selectedRow = int.Parse(seatInfo[0]);
            selectedSeat = int.Parse(seatInfo[1]);
            selectedSeatType = seatInfo[2];

            decimal price = selectedSeatType == "VIP" ? ticketPrice * 1.5m : ticketPrice;

            lblSelectedInfo.Text = $"✓ Выбрано: Ряд {selectedRow}, Место {selectedSeat} ({selectedSeatType})  |  Цена: {price:N0} ₸";

            if (price > userBalance)
            {
                lblSelectedInfo.ForeColor = Color.FromArgb(231, 76, 60);
                lblSelectedInfo.Text += "  ⚠️ Недостаточно средств!";
                btnConfirm.Enabled = false;
                btnConfirm.BackColor = Color.FromArgb(149, 165, 166);
            }
            else
            {
                lblSelectedInfo.ForeColor = Color.FromArgb(41, 128, 185);
                btnConfirm.Enabled = true;
                btnConfirm.BackColor = Color.FromArgb(46, 204, 113);
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (selectedSeatButton == null)
            {
                MessageBox.Show("Выберите место!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal price = selectedSeatType == "VIP" ? ticketPrice * 1.5m : ticketPrice;

                DialogResult confirm = MessageBox.Show(
                    $"Подтвердить покупку билета?\n\n" +
                    $"📍 Ряд: {selectedRow}\n" +
                    $"💺 Место: {selectedSeat}\n" +
                    $"🏷️ Тип: {selectedSeatType}\n" +
                    $"💰 Цена: {price:N0} ₸\n\n" +
                    $"━━━━━━━━━━━━━━━━━━━━━━━\n" +
                    $"Ваш баланс: {userBalance:N0} ₸\n" +
                    $"После покупки: {userBalance - price:N0} ₸",
                    "Подтверждение покупки",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirm == DialogResult.Yes)
                {
                    BookingResult result = bookingService.BookAndPayTicket(userId, sessionId, selectedRow, selectedSeat);

                    if (result.Success)
                    {
                        LastBookingId = result.BookingId;
                        TicketInfo ticketInfo = bookingService.GetTicketInfo(result.BookingId);

                        if (ticketInfo != null)
                        {
                            DialogResult saveResult = MessageBox.Show(
                                $"🎉 Билет успешно куплен!\n\n" +
                                $"📋 Номер билета: {result.TicketNumber}\n" +
                                $"💳 Списано: {result.Amount:N0} ₸\n\n" +
                                $"Хотите сохранить или распечатать билет?",
                                "Покупка успешна!",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information
                            );

                            if (saveResult == DialogResult.Yes)
                            {
                                ticketService.ShowTicketPreview(ticketInfo);
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                $"🎉 Билет успешно куплен!\n\nНомер билета: {result.TicketNumber}",
                                "Покупка успешна!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка покупки: {result.ErrorMessage}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}