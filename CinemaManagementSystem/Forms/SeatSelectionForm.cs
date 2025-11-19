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
        private Button btnConfirm;
        private Button btnCancel;

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
            this.btnConfirm = new Button();
            this.btnCancel = new Button();

            this.SuspendLayout();

            // lblInfo
            this.lblInfo.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            this.lblInfo.Location = new Point(20, 20);
            this.lblInfo.Size = new Size(760, 40);
            this.lblInfo.Text = "Выберите место:\n⬜ - Свободно   ⬛ - Занято   🟦 - Выбранное";

            // seatPanel
            this.seatPanel.Location = new Point(20, 70);
            this.seatPanel.Size = new Size(760, 400);
            this.seatPanel.AutoScroll = true;
            this.seatPanel.BorderStyle = BorderStyle.FixedSingle;

            // btnConfirm
            this.btnConfirm.Location = new Point(250, 490);
            this.btnConfirm.Size = new Size(150, 40);
            this.btnConfirm.Text = "Забронировать";
            this.btnConfirm.Enabled = false;
            this.btnConfirm.Click += btnConfirm_Click;

            // btnCancel
            this.btnCancel.Location = new Point(420, 490);
            this.btnCancel.Size = new Size(150, 40);
            this.btnCancel.Text = "Отмена";
            this.btnCancel.Click += (s, e) => this.Close();

            // SeatSelectionForm
            this.ClientSize = new Size(800, 560);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.seatPanel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SeatSelectionForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Выбор места";
            this.ResumeLayout(false);
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

                int maxRow = 0;
                int maxSeat = 0;

                foreach (DataRow row in seats.Rows)
                {
                    int rowNum = Convert.ToInt32(row["Ряд"]);
                    int seatNum = Convert.ToInt32(row["Номер_места"]);
                    if (rowNum > maxRow) maxRow = rowNum;
                    if (seatNum > maxSeat) maxSeat = seatNum;
                }

                // Создаем экран
                Panel screen = new Panel
                {
                    Location = new Point(50, 10),
                    Size = new Size(maxSeat * 55, 30),
                    BackColor = Color.LightGray
                };
                Label lblScreen = new Label
                {
                    Text = "ЭКРАН",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                screen.Controls.Add(lblScreen);
                seatPanel.Controls.Add(screen);

                // Создаем кнопки для мест
                foreach (DataRow row in seats.Rows)
                {
                    int rowNum = Convert.ToInt32(row["Ряд"]);
                    int seatNum = Convert.ToInt32(row["Номер_места"]);
                    string status = row["Статус_места"].ToString();
                    string seatType = row["Тип_места"].ToString();

                    Button seatButton = new Button
                    {
                        Size = new Size(50, 40),
                        Location = new Point(50 + (seatNum - 1) * 55, 50 + rowNum * 45),
                        Text = $"{rowNum}-{seatNum}",
                        Tag = $"{rowNum}|{seatNum}",
                        Font = new Font("Arial", 8)
                    };

                    if (status == "Занято")
                    {
                        seatButton.BackColor = Color.Gray;
                        seatButton.Enabled = false;
                    }
                    else
                    {
                        seatButton.BackColor = seatType == "VIP" ? Color.Gold : Color.LightGreen;
                        seatButton.Click += SeatButton_Click;
                    }

                    // Показываем номер ряда
                    if (seatNum == 1)
                    {
                        Label lblRow = new Label
                        {
                            Text = $"Ряд {rowNum}",
                            Location = new Point(5, 55 + rowNum * 45),
                            Size = new Size(40, 30),
                            TextAlign = ContentAlignment.MiddleRight
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
                string oldType = selectedSeatButton.BackColor == Color.Gold ? "VIP" : "Regular";
                selectedSeatButton.BackColor = oldType == "VIP" ? Color.Gold : Color.LightGreen;
            }

            // Выбираем новое место
            selectedSeatButton = clickedButton;
            selectedSeatButton.BackColor = Color.DodgerBlue;
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

                int bookingId = bookingService.BookTicket(userId, sessionId, row, seatNumber);

                if (bookingId > 0)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось забронировать билет!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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