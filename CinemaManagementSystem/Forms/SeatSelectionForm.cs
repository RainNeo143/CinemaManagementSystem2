using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    public partial class SeatSelectionForm : Form
    {
        private readonly int userId;
        private readonly int sessionId;
        private readonly BookingService bookingService;
        private Panel seatPanel;
        private Button selectedSeatButton;
        private Label lblInfo;
        private Label lblSelectedInfo;
        private Button btnConfirm;
        private Button btnCancel;
        private Panel legendPanel;
        private decimal ticketPrice = 0;
        private string selectedSeatType = "";

        public SeatSelectionForm(int userId, int sessionId)
        {
            this.userId = userId;
            this.sessionId = sessionId;
            this.bookingService = new BookingService();
            InitializeComponent();
            LoadSeats();
        }

        private void InitializeComponent()
        {
            this.seatPanel = new Panel();
            this.lblInfo = new Label();
            this.lblSelectedInfo = new Label();
            this.btnConfirm = new Button();
            this.btnCancel = new Button();
            this.legendPanel = new Panel();

            this.SuspendLayout();

            // lblInfo
            this.lblInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblInfo.ForeColor = Color.FromArgb(52, 73, 94);
            this.lblInfo.Location = new Point(20, 15);
            this.lblInfo.Size = new Size(900, 30);
            this.lblInfo.Text = "🎬 Выберите место в зале";

            // legendPanel - легенда
            this.legendPanel.Location = new Point(20, 50);
            this.legendPanel.Size = new Size(900, 45);
            this.legendPanel.BackColor = Color.White;
            this.legendPanel.BorderStyle = BorderStyle.FixedSingle;

            CreateLegend();

            // seatPanel
            this.seatPanel.Location = new Point(20, 105);
            this.seatPanel.Size = new Size(900, 450);
            this.seatPanel.AutoScroll = true;
            this.seatPanel.BackColor = Color.FromArgb(236, 240, 241);
            this.seatPanel.BorderStyle = BorderStyle.FixedSingle;

            // lblSelectedInfo
            this.lblSelectedInfo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lblSelectedInfo.ForeColor = Color.FromArgb(41, 128, 185);
            this.lblSelectedInfo.Location = new Point(20, 565);
            this.lblSelectedInfo.Size = new Size(600, 30);
            this.lblSelectedInfo.Text = "Место не выбрано";

            // btnConfirm
            this.btnConfirm.Location = new Point(650, 560);
            this.btnConfirm.Size = new Size(130, 40);
            this.btnConfirm.Text = "✓ Забронировать";
            this.btnConfirm.BackColor = Color.FromArgb(46, 204, 113);
            this.btnConfirm.ForeColor = Color.White;
            this.btnConfirm.FlatStyle = FlatStyle.Flat;
            this.btnConfirm.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnConfirm.Cursor = Cursors.Hand;
            this.btnConfirm.Enabled = false;
            this.btnConfirm.Click += btnConfirm_Click;

            // btnCancel
            this.btnCancel.Location = new Point(790, 560);
            this.btnCancel.Size = new Size(130, 40);
            this.btnCancel.Text = "✕ Отмена";
            this.btnCancel.BackColor = Color.FromArgb(149, 165, 166);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnCancel.Cursor = Cursors.Hand;
            this.btnCancel.Click += (s, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // SeatSelectionForm
            this.ClientSize = new Size(940, 620);
            this.BackColor = Color.White;
            this.Controls.Add(this.lblInfo);
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
            this.Text = "Выбор места";
            this.Icon = SystemIcons.Information;
            this.ResumeLayout(false);
        }

        private void CreateLegend()
        {
            int xPos = 20;

            // Свободное обычное место
            Panel box1 = CreateLegendBox(Color.FromArgb(46, 204, 113), xPos);
            Label lbl1 = CreateLegendLabel("Свободно", xPos + 35);
            legendPanel.Controls.Add(box1);
            legendPanel.Controls.Add(lbl1);
            xPos += 150;

            // VIP место
            Panel box2 = CreateLegendBox(Color.Gold, xPos);
            Label lbl2 = CreateLegendLabel("VIP", xPos + 35);
            legendPanel.Controls.Add(box2);
            legendPanel.Controls.Add(lbl2);
            xPos += 120;

            // Занятое место
            Panel box3 = CreateLegendBox(Color.FromArgb(189, 195, 199), xPos);
            Label lbl3 = CreateLegendLabel("Занято", xPos + 35);
            legendPanel.Controls.Add(box3);
            legendPanel.Controls.Add(lbl3);
            xPos += 130;

            // Выбранное место
            Panel box4 = CreateLegendBox(Color.FromArgb(52, 152, 219), xPos);
            Label lbl4 = CreateLegendLabel("Выбрано", xPos + 35);
            legendPanel.Controls.Add(box4);
            legendPanel.Controls.Add(lbl4);
        }

        private Panel CreateLegendBox(Color color, int x)
        {
            return new Panel
            {
                Size = new Size(25, 25),
                Location = new Point(x, 10),
                BackColor = color,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Label CreateLegendLabel(string text, int x)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, 13),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9F)
            };
        }

        private void LoadSeats()
        {
            try
            {
                DataTable seats = bookingService.GetAvailableSeats(sessionId);

                if (seats.Rows.Count == 0)
                {
                    MessageBox.Show("Нет доступных мест!", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    return;
                }

                // Получаем цену билета
                string priceQuery = "SELECT Цена_билета FROM Сеанс WHERE Код_сеанса = @Код_сеанса";
                var dbService = new DatabaseService();
                var param = new System.Data.SqlClient.SqlParameter("@Код_сеанса", sessionId);
                DataTable priceResult = dbService.ExecuteQuery(priceQuery, param);
                if (priceResult.Rows.Count > 0)
                {
                    ticketPrice = Convert.ToDecimal(priceResult.Rows[0]["Цена_билета"]);
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

                int seatWidth = 45;
                int seatHeight = 40;
                int horizontalSpacing = 10;
                int verticalSpacing = 10;

                // Создаем экран
                Panel screen = new Panel
                {
                    Location = new Point((900 - (maxSeat * (seatWidth + horizontalSpacing))) / 2, 20),
                    Size = new Size(maxSeat * (seatWidth + horizontalSpacing), 40),
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

                // Создаем кнопки для мест
                int startX = (900 - (maxSeat * (seatWidth + horizontalSpacing))) / 2;

                foreach (DataRow row in seats.Rows)
                {
                    int rowNum = Convert.ToInt32(row["Ряд"]);
                    int seatNum = Convert.ToInt32(row["Номер_места"]);
                    string status = row["Статус_места"].ToString();
                    string seatType = row["Тип_места"].ToString();

                    Button seatButton = new Button
                    {
                        Size = new Size(seatWidth, seatHeight),
                        Location = new Point(
                            startX + (seatNum - 1) * (seatWidth + horizontalSpacing),
                            75 + rowNum * (seatHeight + verticalSpacing)
                        ),
                        Text = seatNum.ToString(),
                        Tag = $"{rowNum}|{seatNum}|{seatType}",
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };

                    seatButton.FlatAppearance.BorderSize = 2;
                    seatButton.FlatAppearance.BorderColor = Color.FromArgb(52, 73, 94);

                    if (status == "Занято")
                    {
                        seatButton.BackColor = Color.FromArgb(189, 195, 199);
                        seatButton.ForeColor = Color.White;
                        seatButton.Enabled = false;
                        seatButton.Cursor = Cursors.No;
                        seatButton.Text = "✕";
                    }
                    else
                    {
                        seatButton.BackColor = seatType == "VIP" ? Color.Gold : Color.FromArgb(46, 204, 113);
                        seatButton.ForeColor = seatType == "VIP" ? Color.FromArgb(52, 73, 94) : Color.White;
                        seatButton.Click += SeatButton_Click;

                        // Эффект наведения
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
                                seatButton.BackColor = seatType == "VIP" ? Color.Gold : Color.FromArgb(46, 204, 113);
                                seatButton.ForeColor = seatType == "VIP" ? Color.FromArgb(52, 73, 94) : Color.White;
                            }
                        };
                    }

                    // Показываем номер ряда
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мест: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            // Сбрасываем предыдущий выбор
            if (selectedSeatButton != null && selectedSeatButton != clickedButton)
            {
                string oldInfo = selectedSeatButton.Tag.ToString();
                string oldType = oldInfo.Split('|')[2];
                selectedSeatButton.BackColor = oldType == "VIP" ? Color.Gold : Color.FromArgb(46, 204, 113);
                selectedSeatButton.ForeColor = oldType == "VIP" ? Color.FromArgb(52, 73, 94) : Color.White;
            }

            // Выбираем новое место
            selectedSeatButton = clickedButton;
            selectedSeatButton.BackColor = Color.FromArgb(52, 152, 219);
            selectedSeatButton.ForeColor = Color.White;

            string[] seatInfo = selectedSeatButton.Tag.ToString().Split('|');
            int row = int.Parse(seatInfo[0]);
            int seatNumber = int.Parse(seatInfo[1]);
            selectedSeatType = seatInfo[2];

            decimal price = selectedSeatType == "VIP" ? ticketPrice * 1.5m : ticketPrice;

            lblSelectedInfo.Text = $"✓ Выбрано: Ряд {row}, Место {seatNumber} ({selectedSeatType})  |  Цена: {price:N0} тг";
            btnConfirm.Enabled = true;
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
                string[] seatInfo = selectedSeatButton.Tag.ToString().Split('|');
                int row = int.Parse(seatInfo[0]);
                int seatNumber = int.Parse(seatInfo[1]);

                DialogResult confirm = MessageBox.Show(
                    $"Подтвердить бронирование?\n\nРяд: {row}\nМесто: {seatNumber}\nТип: {selectedSeatType}\nЦена: {(selectedSeatType == "VIP" ? ticketPrice * 1.5m : ticketPrice):N0} тг",
                    "Подтверждение бронирования",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (confirm == DialogResult.Yes)
                {
                    int bookingId = bookingService.BookTicket(userId, sessionId, row, seatNumber);

                    if (bookingId > 0)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось забронировать билет! Возможно, место уже занято.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка бронирования: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}