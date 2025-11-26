using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CinemaManagementSystem.Models;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    public partial class AdminForm : Form
    {
        private readonly User currentUser;
        private readonly BookingService bookingService;
        private readonly ReportService reportService;
        private readonly DatabaseService dbService;

        private TabControl tabControl;
        private Panel headerPanel;
        private Label lblWelcome;
        private Button btnLogout;

        // Вкладка "Управление фильмами"
        private DataGridView dgvFilms;
        private Button btnAddFilm;
        private Button btnEditFilm;
        private Button btnDeleteFilm;
        private TextBox txtFilmSearch;

        // Вкладка "Управление сеансами"
        private DataGridView dgvSessions;
        private Button btnAddSession;
        private Button btnEditSession;
        private Button btnDeleteSession;
        private DateTimePicker dtpSessionFilter;
        private ComboBox cmbFilmFilter;

        // Вкладка "Управление залами"
        private DataGridView dgvHalls;
        private Button btnAddHall;
        private Button btnEditHall;

        // Вкладка "Отчёты"
        private ComboBox cmbReportType;
        private DateTimePicker dtpReportFrom;
        private DateTimePicker dtpReportTo;
        private Button btnGenerateReport;
        private Button btnPreviewReport;
        private Button btnPrintReport;
        private DataGridView dgvReportPreview;
        private Label lblReportSummary;

        // Вкладка "Статистика"
        private DataGridView dgvStatistics;
        private Chart chartRevenue;
        private DateTimePicker dtpStatsFrom;
        private DateTimePicker dtpStatsTo;
        private Button btnLoadStats;

        public AdminForm(User user)
        {
            currentUser = user;
            bookingService = new BookingService();
            reportService = new ReportService();
            dbService = new DatabaseService();
            InitializeComponent();
            LoadAllData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Основные настройки формы
            this.ClientSize = new Size(1400, 850);
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.Name = "AdminForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Кинотеатр - Панель администратора";
            this.Icon = SystemIcons.Shield;
            this.WindowState = FormWindowState.Maximized;

            CreateHeader();
            CreateTabs();

            this.ResumeLayout(false);
        }

        private void CreateHeader()
        {
            this.headerPanel = new Panel
            {
                BackColor = Color.FromArgb(192, 57, 43),
                Dock = DockStyle.Top,
                Height = 70
            };

            this.lblWelcome = new Label
            {
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true,
                Text = $"⚙️ Панель администратора — {currentUser.FullName}"
            };

            this.btnLogout = new Button
            {
                BackColor = Color.FromArgb(44, 62, 80),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Size = new Size(120, 35),
                Text = "🚪 Выход",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.Location = new Point(this.ClientSize.Width - 140, 18);
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Выйти из системы?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Restart();
                }
            };

            this.headerPanel.Controls.Add(this.lblWelcome);
            this.headerPanel.Controls.Add(this.btnLogout);
            this.Controls.Add(this.headerPanel);
        }

        private void CreateTabs()
        {
            this.tabControl = new TabControl
            {
                Location = new Point(10, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F)
            };
            tabControl.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 90);

            // Вкладка 1: Управление фильмами
            tabControl.TabPages.Add(CreateFilmsTab());

            // Вкладка 2: Управление сеансами
            tabControl.TabPages.Add(CreateSessionsTab());

            // Вкладка 3: Управление залами
            tabControl.TabPages.Add(CreateHallsTab());

            // Вкладка 4: Отчёты
            tabControl.TabPages.Add(CreateReportsTab());

            // Вкладка 5: Статистика
            tabControl.TabPages.Add(CreateStatisticsTab());

            this.Controls.Add(this.tabControl);
        }

        #region Вкладка "Фильмы"

        private TabPage CreateFilmsTab()
        {
            TabPage tab = new TabPage("🎬 Фильмы");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            // Панель инструментов
            Panel toolPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Label lblSearch = new Label
            {
                Text = "🔍 Поиск:",
                Location = new Point(10, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            txtFilmSearch = new TextBox
            {
                Location = new Point(80, 17),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10F)
            };
            txtFilmSearch.TextChanged += (s, e) => FilterFilms();

            btnAddFilm = CreateToolButton("➕ Добавить", Color.FromArgb(46, 204, 113), 350);
            btnAddFilm.Click += BtnAddFilm_Click;

            btnEditFilm = CreateToolButton("✏️ Редактировать", Color.FromArgb(52, 152, 219), 500);
            btnEditFilm.Click += BtnEditFilm_Click;

            btnDeleteFilm = CreateToolButton("🗑️ Удалить", Color.FromArgb(231, 76, 60), 680);
            btnDeleteFilm.Click += BtnDeleteFilm_Click;

            Button btnRefreshFilms = CreateToolButton("🔄 Обновить", Color.FromArgb(149, 165, 166), 830);
            btnRefreshFilms.Click += (s, e) => LoadFilms();

            toolPanel.Controls.AddRange(new Control[] { lblSearch, txtFilmSearch, btnAddFilm, btnEditFilm, btnDeleteFilm, btnRefreshFilms });

            // Таблица фильмов
            dgvFilms = CreateStyledDataGridView();
            dgvFilms.Dock = DockStyle.Fill;

            tab.Controls.Add(dgvFilms);
            tab.Controls.Add(toolPanel);

            return tab;
        }

        private void LoadFilms()
        {
            try
            {
                string query = @"
                    SELECT 
                        f.Код_фильма AS [ID],
                        f.Наименование AS [Название],
                        ISNULL(j.Наименование, 'Не указан') AS [Жанр],
                        f.Длительность AS [Мин],
                        f.Возрастные_ограничения AS [Возраст],
                        f.Режиссёр AS [Режиссёр],
                        f.Страна AS [Страна],
                        LEFT(f.Описание, 50) + '...' AS [Описание]
                    FROM Фильмы f
                    LEFT JOIN Жанры j ON f.Код_жанра = j.Код_жанра
                    ORDER BY f.Наименование";

                dgvFilms.DataSource = dbService.ExecuteQuery(query);
                if (dgvFilms.Columns.Contains("ID"))
                    dgvFilms.Columns["ID"].Width = 50;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильмов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterFilms()
        {
            if (dgvFilms.DataSource is DataTable dt)
            {
                string filter = txtFilmSearch.Text.Trim();
                if (string.IsNullOrEmpty(filter))
                    dt.DefaultView.RowFilter = "";
                else
                    dt.DefaultView.RowFilter = $"[Название] LIKE '%{filter}%' OR [Жанр] LIKE '%{filter}%'";
            }
        }

        private void BtnAddFilm_Click(object sender, EventArgs e)
        {
            using (FilmEditForm form = new FilmEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadFilms();
                    MessageBox.Show("Фильм успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnEditFilm_Click(object sender, EventArgs e)
        {
            if (dgvFilms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите фильм для редактирования!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int filmId = Convert.ToInt32(dgvFilms.SelectedRows[0].Cells["ID"].Value);
            using (FilmEditForm form = new FilmEditForm(filmId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadFilms();
                    MessageBox.Show("Фильм успешно обновлён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnDeleteFilm_Click(object sender, EventArgs e)
        {
            if (dgvFilms.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите фильм для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filmName = dgvFilms.SelectedRows[0].Cells["Название"].Value.ToString();
            if (MessageBox.Show($"Удалить фильм \"{filmName}\"?\n\nВнимание: будут удалены все связанные сеансы!",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int filmId = Convert.ToInt32(dgvFilms.SelectedRows[0].Cells["ID"].Value);

                    // Сначала удаляем сеансы
                    dbService.ExecuteNonQuery("DELETE FROM Сеанс WHERE Код_фильма = @id",
                        new System.Data.SqlClient.SqlParameter("@id", filmId));

                    // Затем удаляем фильм
                    dbService.ExecuteNonQuery("DELETE FROM Фильмы WHERE Код_фильма = @id",
                        new System.Data.SqlClient.SqlParameter("@id", filmId));

                    LoadFilms();
                    LoadSessions();
                    MessageBox.Show("Фильм удалён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Вкладка "Сеансы"

        private TabPage CreateSessionsTab()
        {
            TabPage tab = new TabPage("🎥 Сеансы");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            // Панель фильтров
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Label lblDate = new Label
            {
                Text = "📅 Дата:",
                Location = new Point(10, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            dtpSessionFilter = new DateTimePicker
            {
                Location = new Point(70, 17),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F)
            };
            dtpSessionFilter.ValueChanged += (s, e) => LoadSessions();

            Label lblFilm = new Label
            {
                Text = "🎬 Фильм:",
                Location = new Point(240, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            cmbFilmFilter = new ComboBox
            {
                Location = new Point(310, 17),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilmFilter.SelectedIndexChanged += (s, e) => LoadSessions();

            btnAddSession = CreateToolButton("➕ Добавить", Color.FromArgb(46, 204, 113), 530);
            btnAddSession.Click += BtnAddSession_Click;

            btnEditSession = CreateToolButton("✏️ Редактировать", Color.FromArgb(52, 152, 219), 680);
            btnEditSession.Click += BtnEditSession_Click;

            btnDeleteSession = CreateToolButton("🗑️ Удалить", Color.FromArgb(231, 76, 60), 860);
            btnDeleteSession.Click += BtnDeleteSession_Click;

            Button btnRefreshSessions = CreateToolButton("🔄", Color.FromArgb(149, 165, 166), 1010);
            btnRefreshSessions.Size = new Size(40, 35);
            btnRefreshSessions.Click += (s, e) => { LoadFilmsComboBox(); LoadSessions(); };

            filterPanel.Controls.AddRange(new Control[] { lblDate, dtpSessionFilter, lblFilm, cmbFilmFilter,
                btnAddSession, btnEditSession, btnDeleteSession, btnRefreshSessions });

            // Таблица сеансов
            dgvSessions = CreateStyledDataGridView();
            dgvSessions.Dock = DockStyle.Fill;

            tab.Controls.Add(dgvSessions);
            tab.Controls.Add(filterPanel);

            return tab;
        }

        private void LoadFilmsComboBox()
        {
            try
            {
                DataTable films = dbService.ExecuteQuery("SELECT Код_фильма, Наименование FROM Фильмы ORDER BY Наименование");

                cmbFilmFilter.Items.Clear();
                cmbFilmFilter.Items.Add(new ComboBoxItem { Value = 0, Text = "-- Все фильмы --" });

                foreach (DataRow row in films.Rows)
                {
                    cmbFilmFilter.Items.Add(new ComboBoxItem
                    {
                        Value = Convert.ToInt32(row["Код_фильма"]),
                        Text = row["Наименование"].ToString()
                    });
                }

                cmbFilmFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка фильмов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSessions()
        {
            try
            {
                string query = @"
                    SELECT 
                        s.Код_сеанса AS [ID],
                        CONVERT(varchar, s.Дата, 104) AS [Дата],
                        CONVERT(varchar(5), s.Время_начала, 108) AS [Начало],
                        CONVERT(varchar(5), s.Время_окончания, 108) AS [Конец],
                        f.Наименование AS [Фильм],
                        z.Наименование AS [Зал],
                        s.Цена_билета AS [Цена],
                        z.Количество_мест - ISNULL(b.Занято, 0) AS [Свободно],
                        ISNULL(b.Занято, 0) AS [Продано]
                    FROM Сеанс s
                    JOIN Фильмы f ON s.Код_фильма = f.Код_фильма
                    JOIN Залы z ON s.Номер_зала = z.Номер_зала
                    LEFT JOIN (
                        SELECT Код_сеанса, COUNT(*) AS Занято
                        FROM Бронирования WHERE Статус != N'Отменено'
                        GROUP BY Код_сеанса
                    ) b ON s.Код_сеанса = b.Код_сеанса
                    WHERE s.Дата >= @Дата";

                var parameters = new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>
                {
                    new System.Data.SqlClient.SqlParameter("@Дата", dtpSessionFilter.Value.Date)
                };

                if (cmbFilmFilter.SelectedItem is ComboBoxItem item && item.Value > 0)
                {
                    query += " AND s.Код_фильма = @КодФильма";
                    parameters.Add(new System.Data.SqlClient.SqlParameter("@КодФильма", item.Value));
                }

                query += " ORDER BY s.Дата, s.Время_начала";

                dgvSessions.DataSource = dbService.ExecuteQuery(query, parameters.ToArray());

                if (dgvSessions.Columns.Contains("ID"))
                    dgvSessions.Columns["ID"].Width = 50;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сеансов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddSession_Click(object sender, EventArgs e)
        {
            using (SessionEditForm form = new SessionEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadSessions();
                    MessageBox.Show("Сеанс успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnEditSession_Click(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сеанс для редактирования!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sessionId = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["ID"].Value);
            using (SessionEditForm form = new SessionEditForm(sessionId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadSessions();
                    MessageBox.Show("Сеанс успешно обновлён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnDeleteSession_Click(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сеанс для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sold = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["Продано"].Value);
            if (sold > 0)
            {
                MessageBox.Show($"Невозможно удалить сеанс!\nНа него продано {sold} билетов.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Удалить выбранный сеанс?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int sessionId = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["ID"].Value);
                    dbService.ExecuteNonQuery("DELETE FROM Сеанс WHERE Код_сеанса = @id",
                        new System.Data.SqlClient.SqlParameter("@id", sessionId));

                    LoadSessions();
                    MessageBox.Show("Сеанс удалён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Вкладка "Залы"

        private TabPage CreateHallsTab()
        {
            TabPage tab = new TabPage("🏛️ Залы");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            // Панель инструментов
            Panel toolPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            btnAddHall = CreateToolButton("➕ Добавить зал", Color.FromArgb(46, 204, 113), 10);
            btnAddHall.Click += BtnAddHall_Click;

            btnEditHall = CreateToolButton("✏️ Редактировать", Color.FromArgb(52, 152, 219), 180);
            btnEditHall.Click += BtnEditHall_Click;

            Button btnRefreshHalls = CreateToolButton("🔄 Обновить", Color.FromArgb(149, 165, 166), 360);
            btnRefreshHalls.Click += (s, e) => LoadHalls();

            toolPanel.Controls.AddRange(new Control[] { btnAddHall, btnEditHall, btnRefreshHalls });

            // Таблица залов
            dgvHalls = CreateStyledDataGridView();
            dgvHalls.Dock = DockStyle.Fill;

            tab.Controls.Add(dgvHalls);
            tab.Controls.Add(toolPanel);

            return tab;
        }

        private void LoadHalls()
        {
            try
            {
                string query = @"
                    SELECT 
                        z.Номер_зала AS [№],
                        z.Наименование AS [Название],
                        z.Количество_мест AS [Мест],
                        z.Тип_зала AS [Тип],
                        (SELECT COUNT(*) FROM Места_в_залах WHERE Номер_зала = z.Номер_зала AND Тип_места = 'VIP') AS [VIP мест],
                        (SELECT COUNT(DISTINCT Код_сеанса) FROM Сеанс WHERE Номер_зала = z.Номер_зала AND Дата >= GETDATE()) AS [Сеансов]
                    FROM Залы z
                    ORDER BY z.Номер_зала";

                dgvHalls.DataSource = dbService.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки залов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddHall_Click(object sender, EventArgs e)
        {
            using (HallEditForm form = new HallEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadHalls();
                    MessageBox.Show("Зал успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnEditHall_Click(object sender, EventArgs e)
        {
            if (dgvHalls.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите зал для редактирования!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int hallId = Convert.ToInt32(dgvHalls.SelectedRows[0].Cells["№"].Value);
            using (HallEditForm form = new HallEditForm(hallId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadHalls();
                    MessageBox.Show("Зал успешно обновлён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        #region Вкладка "Отчёты"

        private TabPage CreateReportsTab()
        {
            TabPage tab = new TabPage("📊 Отчёты");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            // Панель параметров отчёта
            Panel paramPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblReportType = new Label
            {
                Text = "📋 Тип отчёта:",
                Location = new Point(15, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            cmbReportType = new ComboBox
            {
                Location = new Point(130, 17),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbReportType.Items.AddRange(new object[]
            {
                "Продажи за день",
                "Продажи за период",
                "ТОП фильмов по выручке",
                "Загруженность залов",
                "Статистика по жанрам",
                "ТОП покупателей",
                "Отменённые бронирования",
                "Сводка за период",
                "Расписание на день",
                "Продажи по дням недели",
                "Продажи по времени суток"
            });
            cmbReportType.SelectedIndex = 0;

            Label lblFrom = new Label
            {
                Text = "📅 С:",
                Location = new Point(15, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            dtpReportFrom = new DateTimePicker
            {
                Location = new Point(50, 57),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F),
                Value = DateTime.Today.AddMonths(-1)
            };

            Label lblTo = new Label
            {
                Text = "По:",
                Location = new Point(220, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            dtpReportTo = new DateTimePicker
            {
                Location = new Point(255, 57),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F),
                Value = DateTime.Today
            };

            btnGenerateReport = new Button
            {
                Text = "📄 Сформировать отчёт",
                Location = new Point(430, 55),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGenerateReport.Click += BtnGenerateReport_Click;

            btnPreviewReport = new Button
            {
                Text = "👁️ Предпросмотр",
                Location = new Point(620, 55),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPreviewReport.Click += BtnPreviewReport_Click;

            btnPrintReport = new Button
            {
                Text = "🖨️ Печать",
                Location = new Point(770, 55),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrintReport.Click += BtnPrintReport_Click;

            Button btnSavePdf = new Button
            {
                Text = "💾 Сохранить PDF",
                Location = new Point(880, 55),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSavePdf.Click += BtnSavePdf_Click;

            paramPanel.Controls.AddRange(new Control[] { lblReportType, cmbReportType, lblFrom, dtpReportFrom,
                lblTo, dtpReportTo, btnGenerateReport, btnPreviewReport, btnPrintReport, btnSavePdf });

            // Таблица предпросмотра
            dgvReportPreview = CreateStyledDataGridView();
            dgvReportPreview.Dock = DockStyle.Fill;

            // Итоговая информация
            lblReportSummary = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            tab.Controls.Add(dgvReportPreview);
            tab.Controls.Add(lblReportSummary);
            tab.Controls.Add(paramPanel);

            return tab;
        }

        private void BtnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable data = GetReportData();
                dgvReportPreview.DataSource = data;

                // Подсчёт итогов
                decimal totalSum = 0;
                int totalCount = data.Rows.Count;

                foreach (DataRow row in data.Rows)
                {
                    foreach (DataColumn col in data.Columns)
                    {
                        if (col.ColumnName.Contains("Выручка") || col.ColumnName.Contains("Сумма"))
                        {
                            if (row[col] != DBNull.Value)
                                totalSum += Convert.ToDecimal(row[col]);
                        }
                    }
                }

                lblReportSummary.Text = $"📊 Записей: {totalCount} | 💰 Общая сумма: {totalSum:N0} ₸";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable data = GetReportData();
                string title = cmbReportType.SelectedItem.ToString();
                string subtitle = $"Период: {dtpReportFrom.Value:dd.MM.yyyy} — {dtpReportTo.Value:dd.MM.yyyy}";

                reportService.ShowReportPreview(data, title, subtitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrintReport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable data = GetReportData();
                string title = cmbReportType.SelectedItem.ToString();
                string subtitle = $"Период: {dtpReportFrom.Value:dd.MM.yyyy} — {dtpReportTo.Value:dd.MM.yyyy}";

                reportService.PrintReport(data, title, subtitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSavePdf_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable data = GetReportData();
                string title = cmbReportType.SelectedItem.ToString();
                string subtitle = $"Период: {dtpReportFrom.Value:dd.MM.yyyy} — {dtpReportTo.Value:dd.MM.yyyy}";

                string filePath = reportService.SaveReportToPdf(data, title, subtitle);

                if (!string.IsNullOrEmpty(filePath))
                {
                    MessageBox.Show($"Отчёт сохранён:\n{filePath}", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Открыть файл
                    if (MessageBox.Show("Открыть файл?", "Вопрос",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetReportData()
        {
            DateTime dateFrom = dtpReportFrom.Value.Date;
            DateTime dateTo = dtpReportTo.Value.Date;

            switch (cmbReportType.SelectedIndex)
            {
                case 0: return reportService.GetDailySalesReport(dateFrom);
                case 1: return reportService.GetSalesReport(dateFrom, dateTo);
                case 2: return reportService.GetTopFilmsByRevenueReport(dateFrom, dateTo);
                case 3: return reportService.GetHallOccupancyReport(dateFrom, dateTo);
                case 4: return reportService.GetGenreStatisticsReport(dateFrom, dateTo);
                case 5: return reportService.GetUserActivityReport(dateFrom, dateTo);
                case 6: return reportService.GetCancelledBookingsReport(dateFrom, dateTo);
                case 7: return reportService.GetPeriodSummaryReport(dateFrom, dateTo);
                case 8: return reportService.GetScheduleReport(dateFrom);
                case 9: return reportService.GetSalesByDayOfWeekReport(dateFrom, dateTo);
                case 10: return reportService.GetSalesByTimeReport(dateFrom, dateTo);
                default: return new DataTable();
            }
        }

        #endregion

        #region Вкладка "Статистика"

        private TabPage CreateStatisticsTab()
        {
            TabPage tab = new TabPage("📈 Статистика");
            tab.BackColor = Color.White;
            tab.Padding = new Padding(10);

            // Панель фильтров
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Label lblFrom = new Label { Text = "С:", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            dtpStatsFrom = new DateTimePicker { Location = new Point(35, 17), Size = new Size(150, 25), Value = DateTime.Today.AddMonths(-1) };

            Label lblTo = new Label { Text = "По:", Location = new Point(200, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            dtpStatsTo = new DateTimePicker { Location = new Point(230, 17), Size = new Size(150, 25), Value = DateTime.Today };

            btnLoadStats = CreateToolButton("📊 Загрузить", Color.FromArgb(52, 152, 219), 400);
            btnLoadStats.Click += (s, e) => LoadStatistics();

            filterPanel.Controls.AddRange(new Control[] { lblFrom, dtpStatsFrom, lblTo, dtpStatsTo, btnLoadStats });

            // Разделяем на две области
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };

            // Верхняя часть - таблица
            dgvStatistics = CreateStyledDataGridView();
            dgvStatistics.Dock = DockStyle.Fill;
            splitContainer.Panel1.Controls.Add(dgvStatistics);

            // Нижняя часть - график
            chartRevenue = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            ChartArea chartArea = new ChartArea { Name = "ChartArea1", BackColor = Color.WhiteSmoke };
            chartRevenue.ChartAreas.Add(chartArea);
            chartRevenue.Legends.Add(new Legend { Name = "Legend1", Font = new Font("Segoe UI", 9F) });

            Series series = new Series
            {
                Name = "Выручка",
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(52, 152, 219),
                BorderWidth = 2
            };
            chartRevenue.Series.Add(series);

            Title title = new Title
            {
                Text = "Выручка по фильмам (ТОП-10)",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            chartRevenue.Titles.Add(title);

            splitContainer.Panel2.Controls.Add(chartRevenue);

            tab.Controls.Add(splitContainer);
            tab.Controls.Add(filterPanel);

            return tab;
        }

        private void LoadStatistics()
        {
            try
            {
                // Загружаем статистику
                dgvStatistics.DataSource = bookingService.GetStatistics(dtpStatsFrom.Value, dtpStatsTo.Value);

                // Загружаем график
                DataTable popular = bookingService.GetPopularFilms(10);
                chartRevenue.Series[0].Points.Clear();

                foreach (DataRow row in popular.Rows)
                {
                    string filmName = row["Фильм"].ToString();
                    if (filmName.Length > 15)
                        filmName = filmName.Substring(0, 12) + "...";

                    double revenue = row["Выручка"] != DBNull.Value ? Convert.ToDouble(row["Выручка"]) : 0;
                    chartRevenue.Series[0].Points.AddXY(filmName, revenue);
                }

                chartRevenue.ChartAreas[0].AxisX.Interval = 1;
                chartRevenue.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Вспомогательные методы

        private Button CreateToolButton(string text, Color backColor, int x)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, 13),
                Size = new Size(140, 35),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private DataGridView CreateStyledDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowTemplate = { Height = 30 },
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(192, 57, 43);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(224, 224, 224);

            return dgv;
        }

        private void LoadAllData()
        {
            LoadFilms();
            LoadFilmsComboBox();
            LoadSessions();
            LoadHalls();
            LoadStatistics();
        }

        #endregion
    }

    /// <summary>
    /// Вспомогательный класс для ComboBox
    /// </summary>
    public class ComboBoxItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public override string ToString() => Text;
    }
}