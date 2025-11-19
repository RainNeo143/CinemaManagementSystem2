using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CinemaManagementSystem.Models;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    public partial class MainUserForm : Form
    {
        private readonly User currentUser;
        private readonly BookingService bookingService;
        private DataGridView dgvFilms;
        private DataGridView dgvSessions;
        private DataGridView dgvMyBookings;
        private TabControl tabControl;
        private Button btnBookTicket;
        private Button btnCancelBooking;
        private Label lblWelcome;

        public MainUserForm(User user)
        {
            currentUser = user;
            bookingService = new BookingService();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.dgvFilms = new DataGridView();
            this.dgvSessions = new DataGridView();
            this.dgvMyBookings = new DataGridView();
            this.btnBookTicket = new Button();
            this.btnCancelBooking = new Button();
            this.lblWelcome = new Label();

            this.SuspendLayout();

            // lblWelcome
            this.lblWelcome.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblWelcome.Location = new Point(20, 15);
            this.lblWelcome.Size = new Size(800, 25);
            this.lblWelcome.Text = $"Добро пожаловать, {currentUser.FullName}!";

            // TabControl
            this.tabControl.Location = new Point(20, 50);
            this.tabControl.Size = new Size(940, 500);

            // Вкладка "Репертуар"
            TabPage tabRepertoire = new TabPage("Репертуар");
            this.dgvFilms.Dock = DockStyle.Fill;
            this.dgvFilms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvFilms.MultiSelect = false;
            this.dgvFilms.ReadOnly = true;
            this.dgvFilms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvFilms.SelectionChanged += dgvFilms_SelectionChanged;
            tabRepertoire.Controls.Add(this.dgvFilms);
            this.tabControl.TabPages.Add(tabRepertoire);

            // Вкладка "Сеансы"
            TabPage tabSessions = new TabPage("Сеансы");
            Panel sessionPanel = new Panel { Dock = DockStyle.Fill };
            this.dgvSessions.Location = new Point(10, 10);
            this.dgvSessions.Size = new Size(900, 380);
            this.dgvSessions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSessions.MultiSelect = false;
            this.dgvSessions.ReadOnly = true;
            this.dgvSessions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            this.btnBookTicket.Location = new Point(10, 400);
            this.btnBookTicket.Size = new Size(200, 40);
            this.btnBookTicket.Text = "Забронировать билет";
            this.btnBookTicket.Click += btnBookTicket_Click;

            sessionPanel.Controls.Add(this.dgvSessions);
            sessionPanel.Controls.Add(this.btnBookTicket);
            tabSessions.Controls.Add(sessionPanel);
            this.tabControl.TabPages.Add(tabSessions);

            // Вкладка "Мои бронирования"
            TabPage tabMyBookings = new TabPage("Мои бронирования");
            Panel bookingsPanel = new Panel { Dock = DockStyle.Fill };
            this.dgvMyBookings.Location = new Point(10, 10);
            this.dgvMyBookings.Size = new Size(900, 380);
            this.dgvMyBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvMyBookings.MultiSelect = false;
            this.dgvMyBookings.ReadOnly = true;
            this.dgvMyBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            this.btnCancelBooking.Location = new Point(10, 400);
            this.btnCancelBooking.Size = new Size(200, 40);
            this.btnCancelBooking.Text = "Отменить бронирование";
            this.btnCancelBooking.Click += btnCancelBooking_Click;

            bookingsPanel.Controls.Add(this.dgvMyBookings);
            bookingsPanel.Controls.Add(this.btnCancelBooking);
            tabMyBookings.Controls.Add(bookingsPanel);
            this.tabControl.TabPages.Add(tabMyBookings);

            // MainUserForm
            this.ClientSize = new Size(980, 580);
            this.Controls.Add(this.lblWelcome);
            this.Controls.Add(this.tabControl);
            this.Name = "MainUserForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Система управления кинотеатром - Пользователь";
            this.ResumeLayout(false);
        }

        private void LoadData()
        {
            LoadFilms();
            LoadMyBookings();
        }

        private void LoadFilms()
        {
            try
            {
                DataTable films = bookingService.GetFilmRepertoire();
                dgvFilms.DataSource = films;

                if (dgvFilms.Columns.Contains("Код_фильма"))
                    dgvFilms.Columns["Код_фильма"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки репертуара: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvFilms_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvFilms.SelectedRows.Count > 0)
            {
                int filmId = Convert.ToInt32(dgvFilms.SelectedRows[0].Cells["Код_фильма"].Value);
                LoadSessions(filmId);
            }
        }

        private void LoadSessions(int filmId)
        {
            try
            {
                DataTable sessions = bookingService.GetSessionsForFilm(filmId);
                dgvSessions.DataSource = sessions;

                if (dgvSessions.Columns.Contains("Код_сеанса"))
                    dgvSessions.Columns["Код_сеанса"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сеансов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBookTicket_Click(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сеанс!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sessionId = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["Код_сеанса"].Value);
            int availableSeats = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["Свободных_мест"].Value);

            if (availableSeats <= 0)
            {
                MessageBox.Show("К сожалению, все места заняты!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SeatSelectionForm seatForm = new SeatSelectionForm(currentUser.Id, sessionId);
            if (seatForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Билет успешно забронирован!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMyBookings();
                LoadSessions(Convert.ToInt32(dgvFilms.SelectedRows[0].Cells["Код_фильма"].Value));
            }
        }

        private void LoadMyBookings()
        {
            try
            {
                DataTable bookings = bookingService.GetUserBookings(currentUser.Id);
                dgvMyBookings.DataSource = bookings;

                if (dgvMyBookings.Columns.Contains("Код_бронирования"))
                    dgvMyBookings.Columns["Код_бронирования"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки бронирований: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelBooking_Click(object sender, EventArgs e)
        {
            if (dgvMyBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите бронирование для отмены!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string status = dgvMyBookings.SelectedRows[0].Cells["Статус"].Value.ToString();
            if (status == "Отменено")
            {
                MessageBox.Show("Это бронирование уже отменено!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("Вы уверены, что хотите отменить бронирование?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int bookingId = Convert.ToInt32(dgvMyBookings.SelectedRows[0].Cells["Код_бронирования"].Value);
                    bool success = bookingService.CancelBooking(bookingId, currentUser.Id);

                    if (success)
                    {
                        MessageBox.Show("Бронирование успешно отменено!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMyBookings();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось отменить бронирование!", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}