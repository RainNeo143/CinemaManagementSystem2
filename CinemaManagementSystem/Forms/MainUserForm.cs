using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CinemaManagementSystem.Models;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    public partial class MainUserForm : Form
    {
        private readonly User currentUser;
        private readonly BookingService bookingService;
        private readonly AuthService authService;
        private TabControl tabControl;
        private Panel panelFilms;
        private DataGridView dgvSessions;
        private DataGridView dgvMyBookings;
        private Button btnRefresh;
        private Button btnCancelBooking;
        private Button btnBuyTicket;
        private Label lblWelcome;
        private Label lblBalance;
        private Panel headerPanel;
        private Button btnLogout;
        private DateTimePicker dtpSessionDate;
        private Panel filmDetailsPanel;
        private PictureBox selectedFilmPoster;
        private int selectedFilmId = -1;

        // Папка с постерами
        private string postersPath;

        // Маппинг названий фильмов к файлам постеров
        private Dictionary<string, string> posterFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Аватар", "Аватар.jpeg" },
            { "Аватар: Путь воды", "Аватар путь воды.png" },
            { "Джон Уик 4", "Джон Уик 4.jpg" },
            { "Звонок", "Звонок.jpeg" },
            { "Крушение", "Крушение.png" },
            { "Майор Гром: Игра", "Майор Гром Игра.jpg" },
            { "Мстители", "Мстители.jpg" },
            { "Операция Ы", "Операция Ы.jpg" },
            { "Оппенгеймер", "Оппенгеймер.jpg" }
        };

        public MainUserForm(User user)
        {
            currentUser = user;
            bookingService = new BookingService();
            authService = new AuthService();

            // ИЗМЕНИТЬ ЭТУ СТРОКУ:
            postersPath = @"D:\Постеры";

            // Создаём папку если её нет
            if (!Directory.Exists(postersPath))
            {
                Directory.CreateDirectory(postersPath);
            }

            InitializeComponent();
            LoadData();
            UpdateBalanceDisplay();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.panelFilms = new Panel();
            this.dgvSessions = new DataGridView();
            this.dgvMyBookings = new DataGridView();
            this.btnRefresh = new Button();
            this.btnCancelBooking = new Button();
            this.btnBuyTicket = new Button();
            this.btnLogout = new Button();
            this.lblWelcome = new Label();
            this.lblBalance = new Label();
            this.headerPanel = new Panel();
            this.dtpSessionDate = new DateTimePicker();
            this.filmDetailsPanel = new Panel();
            this.selectedFilmPoster = new PictureBox();

            this.SuspendLayout();

            // Header Panel
            this.headerPanel.BackColor = Color.FromArgb(41, 128, 185);
            this.headerPanel.Location = new Point(0, 0);
            this.headerPanel.Size = new Size(1400, 80);
            this.headerPanel.Dock = DockStyle.Top;

            // lblWelcome
            this.lblWelcome.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblWelcome.ForeColor = Color.White;
            this.lblWelcome.Location = new Point(20, 15);
            this.lblWelcome.Size = new Size(800, 30);
            this.lblWelcome.Text = $"🎬 Добро пожаловать, {currentUser.FullName}!";

            // lblBalance
            this.lblBalance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblBalance.ForeColor = Color.FromArgb(46, 204, 113);
            this.lblBalance.Location = new Point(20, 48);
            this.lblBalance.Size = new Size(400, 25);
            this.lblBalance.Text = $"💰 Баланс: {currentUser.Balance:N0} ₸";

            // btnLogout
            this.btnLogout.BackColor = Color.FromArgb(231, 76, 60);
            this.btnLogout.FlatStyle = FlatStyle.Flat;
            this.btnLogout.ForeColor = Color.White;
            this.btnLogout.Location = new Point(1250, 25);
            this.btnLogout.Size = new Size(120, 35);
            this.btnLogout.Text = "Выход";
            this.btnLogout.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnLogout.Cursor = Cursors.Hand;
            this.btnLogout.Click += (s, e) => {
                if (MessageBox.Show("Выйти из системы?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Restart();
                }
            };

            this.headerPanel.Controls.Add(this.lblWelcome);
            this.headerPanel.Controls.Add(this.lblBalance);
            this.headerPanel.Controls.Add(this.btnLogout);

            // TabControl
            this.tabControl.Location = new Point(10, 90);
            this.tabControl.Size = new Size(1380, 670);
            this.tabControl.Font = new Font("Segoe UI", 11F);

            // Вкладка "Афиша"
            TabPage tabRepertoire = CreateRepertoireTab();
            this.tabControl.TabPages.Add(tabRepertoire);

            // Вкладка "Мои бронирования"
            TabPage tabMyBookings = CreateMyBookingsTab();
            this.tabControl.TabPages.Add(tabMyBookings);

            // MainUserForm
            this.ClientSize = new Size(1400, 780);
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.tabControl);
            this.Name = "MainUserForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Система бронирования билетов";
            this.Icon = SystemIcons.Application;
            this.ResumeLayout(false);
        }

        private TabPage CreateRepertoireTab()
        {
            TabPage tab = new TabPage("🎥 Афиша фильмов");

            // Главный контейнер
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Левая панель - список фильмов
            Panel leftPanel = new Panel { Dock = DockStyle.Fill };
            Label lblFilmsTitle = new Label
            {
                Text = "📽️ Фильмы в прокате",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };

            this.panelFilms.Location = new Point(10, 50);
            this.panelFilms.Size = new Size(780, 550);
            this.panelFilms.AutoScroll = true;
            this.panelFilms.BackColor = Color.White;
            this.panelFilms.BorderStyle = BorderStyle.FixedSingle;

            leftPanel.Controls.Add(lblFilmsTitle);
            leftPanel.Controls.Add(this.panelFilms);

            // Правая панель - детали и сеансы
            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Панель деталей фильма с постером
            this.filmDetailsPanel.Location = new Point(10, 10);
            this.filmDetailsPanel.Size = new Size(480, 200);
            this.filmDetailsPanel.BackColor = Color.FromArgb(52, 73, 94);
            this.filmDetailsPanel.BorderStyle = BorderStyle.FixedSingle;
            this.filmDetailsPanel.Visible = false;

            // Панель выбора даты
            Panel datePanel = new Panel
            {
                Location = new Point(10, 220),
                Size = new Size(480, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblDate = new Label
            {
                Text = "📅 Выберите дату:",
                Location = new Point(10, 20),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            this.dtpSessionDate.Location = new Point(170, 17);
            this.dtpSessionDate.Size = new Size(200, 25);
            this.dtpSessionDate.Value = DateTime.Today;
            this.dtpSessionDate.Font = new Font("Segoe UI", 10F);
            this.dtpSessionDate.ValueChanged += (s, e) => {
                if (selectedFilmId != -1)
                    LoadSessions(selectedFilmId);
            };

            Button btnToday = new Button
            {
                Text = "Сегодня",
                Location = new Point(380, 15),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToday.Click += (s, e) => {
                dtpSessionDate.Value = DateTime.Today;
            };

            datePanel.Controls.Add(lblDate);
            datePanel.Controls.Add(this.dtpSessionDate);
            datePanel.Controls.Add(btnToday);

            // Список сеансов
            Label lblSessions = new Label
            {
                Text = "🕐 Доступные сеансы",
                Location = new Point(10, 290),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            this.dgvSessions.Location = new Point(10, 320);
            this.dgvSessions.Size = new Size(480, 220);
            this.dgvSessions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSessions.MultiSelect = false;
            this.dgvSessions.ReadOnly = true;
            this.dgvSessions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSessions.BackgroundColor = Color.White;
            this.dgvSessions.BorderStyle = BorderStyle.None;
            this.dgvSessions.RowTemplate.Height = 35;
            this.dgvSessions.DoubleClick += dgvSessions_DoubleClick;
            this.dgvSessions.SelectionChanged += dgvSessions_SelectionChanged;

            StyleDataGridView(this.dgvSessions);

            // Кнопка КУПИТЬ БИЛЕТ
            this.btnBuyTicket.Location = new Point(10, 550);
            this.btnBuyTicket.Size = new Size(480, 50);
            this.btnBuyTicket.Text = "🎫 КУПИТЬ БИЛЕТ";
            this.btnBuyTicket.BackColor = Color.FromArgb(46, 204, 113);
            this.btnBuyTicket.ForeColor = Color.White;
            this.btnBuyTicket.FlatStyle = FlatStyle.Flat;
            this.btnBuyTicket.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.btnBuyTicket.Cursor = Cursors.Hand;
            this.btnBuyTicket.Enabled = false;
            this.btnBuyTicket.Click += btnBuyTicket_Click;

            // Подсказка
            Label lblHint = new Label
            {
                Text = "💡 Выберите сеанс и нажмите кнопку или дважды кликните по сеансу",
                Location = new Point(10, 605),
                Size = new Size(480, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            rightPanel.Controls.Add(this.filmDetailsPanel);
            rightPanel.Controls.Add(datePanel);
            rightPanel.Controls.Add(lblSessions);
            rightPanel.Controls.Add(this.dgvSessions);
            rightPanel.Controls.Add(this.btnBuyTicket);
            rightPanel.Controls.Add(lblHint);

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);
            tab.Controls.Add(mainLayout);

            return tab;
        }

        private TabPage CreateMyBookingsTab()
        {
            TabPage tab = new TabPage("📋 Мои бронирования");
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            Label lblTitle = new Label
            {
                Text = "🎫 История бронирований",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(15, 15),
                Size = new Size(400, 30)
            };

            this.dgvMyBookings.Location = new Point(15, 60);
            this.dgvMyBookings.Size = new Size(1330, 490);
            this.dgvMyBookings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.dgvMyBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvMyBookings.MultiSelect = false;
            this.dgvMyBookings.ReadOnly = true;
            this.dgvMyBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMyBookings.BackgroundColor = Color.White;
            this.dgvMyBookings.BorderStyle = BorderStyle.None;
            this.dgvMyBookings.RowTemplate.Height = 35;

            StyleDataGridView(this.dgvMyBookings);

            this.btnCancelBooking.Location = new Point(15, 565);
            this.btnCancelBooking.Size = new Size(200, 45);
            this.btnCancelBooking.Text = "❌ Отменить бронирование";
            this.btnCancelBooking.BackColor = Color.FromArgb(231, 76, 60);
            this.btnCancelBooking.ForeColor = Color.White;
            this.btnCancelBooking.FlatStyle = FlatStyle.Flat;
            this.btnCancelBooking.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnCancelBooking.Cursor = Cursors.Hand;
            this.btnCancelBooking.Click += btnCancelBooking_Click;

            this.btnRefresh.Location = new Point(225, 565);
            this.btnRefresh.Size = new Size(150, 45);
            this.btnRefresh.Text = "🔄 Обновить";
            this.btnRefresh.BackColor = Color.FromArgb(52, 152, 219);
            this.btnRefresh.ForeColor = Color.White;
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnRefresh.Cursor = Cursors.Hand;
            this.btnRefresh.Click += (s, e) => {
                LoadMyBookings();
                UpdateBalanceDisplay();
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(this.dgvMyBookings);
            panel.Controls.Add(this.btnCancelBooking);
            panel.Controls.Add(this.btnRefresh);
            tab.Controls.Add(panel);

            return tab;
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(224, 224, 224);
        }

        private void UpdateBalanceDisplay()
        {
            try
            {
                decimal balance = authService.GetUserBalance(currentUser.Id);
                currentUser.Balance = balance;
                lblBalance.Text = $"💰 Баланс: {balance:N0} ₸";
            }
            catch
            {
                lblBalance.Text = $"💰 Баланс: {currentUser.Balance:N0} ₸";
            }
        }

        private void LoadData()
        {
            try
            {
                LoadFilms();
                LoadMyBookings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== ЗАГРУЗКА ПОСТЕРА ==========
        private Image LoadPoster(string filmName)
        {
            try
            {
                // Ищем точное совпадение
                if (posterFiles.ContainsKey(filmName))
                {
                    string posterPath = Path.Combine(postersPath, posterFiles[filmName]);
                    if (File.Exists(posterPath))
                    {
                        return Image.FromFile(posterPath);
                    }
                }

                // Ищем частичное совпадение
                foreach (var kvp in posterFiles)
                {
                    if (filmName.ToLower().Contains(kvp.Key.ToLower()) || 
                        kvp.Key.ToLower().Contains(filmName.ToLower()))
                    {
                        string posterPath = Path.Combine(postersPath, kvp.Value);
                        if (File.Exists(posterPath))
                        {
                            return Image.FromFile(posterPath);
                        }
                    }
                }

                // Ищем файл по имени фильма напрямую
                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                foreach (string ext in extensions)
                {
                    string directPath = Path.Combine(postersPath, filmName + ext);
                    if (File.Exists(directPath))
                    {
                        return Image.FromFile(directPath);
                    }
                }

                // Ищем любой файл содержащий часть названия
                if (Directory.Exists(postersPath))
                {
                    string[] files = Directory.GetFiles(postersPath);
                    string searchName = filmName.ToLower().Replace(":", "").Replace(" ", "");
                    
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file).ToLower().Replace(" ", "");
                        if (fileName.Contains(searchName) || searchName.Contains(fileName))
                        {
                            return Image.FromFile(file);
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки изображения
            }

            return null;
        }

        // Создание заглушки постера
        private Image CreatePlaceholderPoster(int width, int height)
        {
            Bitmap placeholder = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.Clear(Color.FromArgb(52, 152, 219));
                
                using (Font font = new Font("Segoe UI", 40F))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    string text = "🎬";
                    SizeF textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, 
                        (width - textSize.Width) / 2, 
                        (height - textSize.Height) / 2);
                }
            }
            return placeholder;
        }

        private void LoadFilms()
        {
            try
            {
                DataTable films = bookingService.GetFilmRepertoire(DateTime.Today);
                panelFilms.Controls.Clear();

                if (films.Rows.Count == 0)
                {
                    Label lblNoFilms = new Label
                    {
                        Text = "В репертуаре пока нет фильмов",
                        Font = new Font("Segoe UI", 12F, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        AutoSize = true,
                        Location = new Point(250, 200)
                    };
                    panelFilms.Controls.Add(lblNoFilms);
                    return;
                }

                int yPos = 10;
                foreach (DataRow row in films.Rows)
                {
                    Panel filmCard = CreateFilmCard(row);
                    filmCard.Location = new Point(10, yPos);
                    panelFilms.Controls.Add(filmCard);
                    yPos += filmCard.Height + 15;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильмов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateFilmCard(DataRow film)
        {
            int filmId = Convert.ToInt32(film["Код_фильма"]);
            string title = film["Фильм"].ToString();
            string genre = film["Жанр"] != DBNull.Value ? film["Жанр"].ToString() : "Не указан";
            int duration = film["Длительность"] != DBNull.Value ? Convert.ToInt32(film["Длительность"]) : 0;
            string ageRating = film["Возрастные_ограничения"] != DBNull.Value ? film["Возрастные_ограничения"].ToString() : "";
            string description = film["Описание"] != DBNull.Value ? film["Описание"].ToString() : "Описание отсутствует";

            Panel card = new Panel
            {
                Size = new Size(740, 160),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Tag = filmId
            };

            // ========== ПОСТЕР ФИЛЬМА ==========
            PictureBox posterBox = new PictureBox
            {
                Size = new Size(100, 140),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(52, 152, 219),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Загружаем постер
            Image poster = LoadPoster(title);
            if (poster != null)
            {
                posterBox.Image = poster;
            }
            else
            {
                posterBox.Image = CreatePlaceholderPoster(100, 140);
            }

            // Название фильма
            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(120, 10),
                Size = new Size(600, 30),
                AutoEllipsis = true
            };

            // Жанр и длительность
            Label lblInfo = new Label
            {
                Text = $"🎭 {genre}  |  ⏱️ {duration} мин  |  {ageRating}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                Location = new Point(120, 42),
                Size = new Size(600, 20)
            };

            // Описание
            Label lblDescription = new Label
            {
                Text = description.Length > 150 ? description.Substring(0, 147) + "..." : description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(120, 68),
                Size = new Size(500, 50),
                AutoEllipsis = true
            };

            // Кнопка выбора
            Button btnSelect = new Button
            {
                Text = "Выбрать сеансы ➜",
                Location = new Point(550, 120),
                Size = new Size(170, 32),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = filmId
            };
            btnSelect.Click += (s, e) => {
                SelectFilm(filmId, title, genre, duration, ageRating, description);
            };

            card.Controls.Add(posterBox);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblInfo);
            card.Controls.Add(lblDescription);
            card.Controls.Add(btnSelect);

            // При клике на карточку тоже выбирается фильм
            card.Click += (s, e) => {
                SelectFilm(filmId, title, genre, duration, ageRating, description);
            };

            // Эффект при наведении
            card.MouseEnter += (s, e) => {
                card.BackColor = Color.FromArgb(245, 250, 255);
            };
            card.MouseLeave += (s, e) => {
                card.BackColor = Color.White;
            };

            return card;
        }

        private void SelectFilm(int filmId, string title, string genre, int duration, string ageRating, string description)
        {
            selectedFilmId = filmId;

            // Обновляем панель деталей
            filmDetailsPanel.Controls.Clear();
            filmDetailsPanel.Visible = true;

            // Постер в панели деталей
            PictureBox detailPoster = new PictureBox
            {
                Size = new Size(120, 170),
                Location = new Point(10, 15),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Image poster = LoadPoster(title);
            detailPoster.Image = poster ?? CreatePlaceholderPoster(120, 170);

            Label lblDetailTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(140, 15),
                Size = new Size(330, 28),
                AutoEllipsis = true
            };

            Label lblDetailInfo = new Label
            {
                Text = $"{genre}  •  {duration} мин  •  {ageRating}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(140, 45),
                Size = new Size(330, 20)
            };

            Label lblDetailDescription = new Label
            {
                Text = description.Length > 200 ? description.Substring(0, 197) + "..." : description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Location = new Point(140, 70),
                Size = new Size(330, 115),
                AutoEllipsis = true
            };

            filmDetailsPanel.Controls.Add(detailPoster);
            filmDetailsPanel.Controls.Add(lblDetailTitle);
            filmDetailsPanel.Controls.Add(lblDetailInfo);
            filmDetailsPanel.Controls.Add(lblDetailDescription);

            LoadSessions(filmId);
        }

        private void LoadSessions(int filmId)
        {
            try
            {
                DataTable sessions = bookingService.GetSessionsForFilm(filmId, dtpSessionDate.Value);
                dgvSessions.DataSource = sessions;

                // Скрываем ненужные столбцы
                if (dgvSessions.Columns.Contains("Код_сеанса"))
                    dgvSessions.Columns["Код_сеанса"].Visible = false;
                if (dgvSessions.Columns.Contains("Номер_зала"))
                    dgvSessions.Columns["Номер_зала"].Visible = false;
                if (dgvSessions.Columns.Contains("Количество_мест"))
                    dgvSessions.Columns["Количество_мест"].Visible = false;

                // Отключаем кнопку если нет сеансов
                btnBuyTicket.Enabled = sessions.Rows.Count > 0;

                if (sessions.Rows.Count == 0)
                {
                    btnBuyTicket.Text = "🎫 НЕТ ДОСТУПНЫХ СЕАНСОВ";
                    btnBuyTicket.BackColor = Color.FromArgb(149, 165, 166);
                }
                else
                {
                    btnBuyTicket.Text = "🎫 КУПИТЬ БИЛЕТ";
                    btnBuyTicket.BackColor = Color.FromArgb(46, 204, 113);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сеансов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvSessions_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count > 0)
            {
                int availableSeats = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["Свободных_мест"].Value);
                decimal price = Convert.ToDecimal(dgvSessions.SelectedRows[0].Cells["Цена_билета"].Value);

                if (availableSeats <= 0)
                {
                    btnBuyTicket.Text = "🎫 ВСЕ МЕСТА ЗАНЯТЫ";
                    btnBuyTicket.BackColor = Color.FromArgb(231, 76, 60);
                    btnBuyTicket.Enabled = false;
                }
                else
                {
                    btnBuyTicket.Text = $"🎫 КУПИТЬ БИЛЕТ ({price:N0} ₸)";
                    btnBuyTicket.BackColor = Color.FromArgb(46, 204, 113);
                    btnBuyTicket.Enabled = true;
                }
            }
        }

        private void btnBuyTicket_Click(object sender, EventArgs e)
        {
            OpenSeatSelection();
        }

        private void dgvSessions_DoubleClick(object sender, EventArgs e)
        {
            OpenSeatSelection();
        }

        private void OpenSeatSelection()
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сеанс из списка!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sessionId = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["Код_сеанса"].Value);
            int availableSeats = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["Свободных_мест"].Value);

            if (availableSeats <= 0)
            {
                MessageBox.Show("К сожалению, все места на этот сеанс заняты!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Открываем окно выбора места
            SeatSelectionForm seatForm = new SeatSelectionForm(currentUser.Id, sessionId);
            if (seatForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("🎉 Билет успешно куплен!\n\nПосмотреть билет можно во вкладке 'Мои бронирования'", 
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Обновляем данные
                LoadMyBookings();
                LoadSessions(selectedFilmId);
                UpdateBalanceDisplay();
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

                // Раскрашиваем строки по статусу
                foreach (DataGridViewRow row in dgvMyBookings.Rows)
                {
                    if (row.Cells["Статус"].Value != null &&
                        row.Cells["Статус"].Value != DBNull.Value)
                    {
                        string status = row.Cells["Статус"].Value.ToString();

                        if (status == "Отменено")
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                            row.DefaultCellStyle.ForeColor = Color.Gray;
                        }
                        else if (status == "Забронировано")
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(200, 255, 200);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки бронирований: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            decimal amount = Convert.ToDecimal(dgvMyBookings.SelectedRows[0].Cells["Сумма"].Value);

            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите отменить бронирование?\n\nВам будет возвращено: {amount:N0} ₸",
                "Подтверждение отмены", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int bookingId = Convert.ToInt32(dgvMyBookings.SelectedRows[0].Cells["Код_бронирования"].Value);
                    bool success = bookingService.CancelBooking(bookingId, currentUser.Id);

                    if (success)
                    {
                        MessageBox.Show($"Бронирование отменено!\n\nНа ваш баланс возвращено: {amount:N0} ₸", 
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMyBookings();
                        UpdateBalanceDisplay();
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