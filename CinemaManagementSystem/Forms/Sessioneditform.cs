using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    /// <summary>
    /// Форма добавления/редактирования сеанса
    /// </summary>
    public class SessionEditForm : Form
    {
        private readonly DatabaseService dbService;
        private readonly int? sessionId;
        private readonly bool isEditMode;

        private ComboBox cmbFilm;
        private ComboBox cmbHall;
        private DateTimePicker dtpDate;
        private DateTimePicker dtpStartTime;
        private DateTimePicker dtpEndTime;
        private NumericUpDown numPrice;
        private Button btnSave;
        private Button btnCancel;
        private Label lblFilmInfo;

        public SessionEditForm(int? sessionId = null)
        {
            this.sessionId = sessionId;
            this.isEditMode = sessionId.HasValue;
            this.dbService = new DatabaseService();
            InitializeComponent();
            LoadFilms();
            LoadHalls();

            if (isEditMode)
                LoadSessionData();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "Редактирование сеанса" : "Добавление сеанса";
            this.Size = new Size(500, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int y = 20;
            int labelWidth = 130;
            int controlWidth = 320;
            int rowHeight = 40;

            // Фильм
            AddLabel("🎬 Фильм *:", 20, y);
            cmbFilm = new ComboBox
            {
                Location = new Point(labelWidth + 20, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilm.SelectedIndexChanged += CmbFilm_SelectedIndexChanged;
            this.Controls.Add(cmbFilm);
            y += rowHeight;

            // Информация о фильме
            lblFilmInfo = new Label
            {
                Location = new Point(labelWidth + 20, y),
                Size = new Size(controlWidth, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblFilmInfo);
            y += 30;

            // Зал
            AddLabel("🏛️ Зал *:", 20, y);
            cmbHall = new ComboBox
            {
                Location = new Point(labelWidth + 20, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbHall);
            y += rowHeight;

            // Дата
            AddLabel("📅 Дата *:", 20, y);
            dtpDate = new DateTimePicker
            {
                Location = new Point(labelWidth + 20, y),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 10F),
                MinDate = DateTime.Today
            };
            this.Controls.Add(dtpDate);
            y += rowHeight;

            // Время начала
            AddLabel("🕐 Время начала *:", 20, y);
            dtpStartTime = new DateTimePicker
            {
                Location = new Point(labelWidth + 20, y),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Today.AddHours(10)
            };
            dtpStartTime.ValueChanged += DtpStartTime_ValueChanged;
            this.Controls.Add(dtpStartTime);
            y += rowHeight;

            // Время окончания
            AddLabel("🕑 Время окончания:", 20, y);
            dtpEndTime = new DateTimePicker
            {
                Location = new Point(labelWidth + 20, y),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Today.AddHours(12)
            };
            this.Controls.Add(dtpEndTime);
            y += rowHeight;

            // Цена
            AddLabel("💰 Цена билета *:", 20, y);
            numPrice = new NumericUpDown
            {
                Location = new Point(labelWidth + 20, y),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F),
                Minimum = 100,
                Maximum = 50000,
                Value = 2500,
                Increment = 100,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };

            Label lblCurrency = new Label
            {
                Text = "₸",
                Location = new Point(labelWidth + 180, y + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblCurrency);
            this.Controls.Add(numPrice);
            y += rowHeight + 20;

            // Кнопки
            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(labelWidth + 20, y),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(labelWidth + 170, y),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, y + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lbl);
        }

        private void LoadFilms()
        {
            try
            {
                DataTable films = dbService.ExecuteQuery(
                    "SELECT Код_фильма, Наименование, Длительность FROM Фильмы ORDER BY Наименование");

                cmbFilm.Items.Clear();
                cmbFilm.Items.Add(new FilmItem { Value = 0, Text = "-- Выберите фильм --", Duration = 0 });

                foreach (DataRow row in films.Rows)
                {
                    cmbFilm.Items.Add(new FilmItem
                    {
                        Value = Convert.ToInt32(row["Код_фильма"]),
                        Text = row["Наименование"].ToString(),
                        Duration = row["Длительность"] != DBNull.Value ? Convert.ToInt32(row["Длительность"]) : 90
                    });
                }

                cmbFilm.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильмов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHalls()
        {
            try
            {
                DataTable halls = dbService.ExecuteQuery(
                    "SELECT Номер_зала, Наименование, Количество_мест FROM Залы ORDER BY Номер_зала");

                cmbHall.Items.Clear();
                cmbHall.Items.Add(new HallItem { Value = 0, Text = "-- Выберите зал --", Seats = 0 });

                foreach (DataRow row in halls.Rows)
                {
                    cmbHall.Items.Add(new HallItem
                    {
                        Value = Convert.ToInt32(row["Номер_зала"]),
                        Text = $"{row["Наименование"]} ({row["Количество_мест"]} мест)",
                        Seats = Convert.ToInt32(row["Количество_мест"])
                    });
                }

                cmbHall.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки залов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbFilm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFilm.SelectedItem is FilmItem film && film.Value > 0)
            {
                lblFilmInfo.Text = $"Длительность: {film.Duration} мин";
                UpdateEndTime();
            }
            else
            {
                lblFilmInfo.Text = "";
            }
        }

        private void DtpStartTime_ValueChanged(object sender, EventArgs e)
        {
            UpdateEndTime();
        }

        private void UpdateEndTime()
        {
            if (cmbFilm.SelectedItem is FilmItem film && film.Value > 0)
            {
                // Добавляем длительность фильма + 15 минут на рекламу
                dtpEndTime.Value = dtpStartTime.Value.AddMinutes(film.Duration + 15);
            }
        }

        private void LoadSessionData()
        {
            try
            {
                string query = @"SELECT * FROM Сеанс WHERE Код_сеанса = @id";
                var param = new System.Data.SqlClient.SqlParameter("@id", sessionId.Value);
                DataTable dt = dbService.ExecuteQuery(query, param);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    // Выбор фильма
                    int filmId = Convert.ToInt32(row["Код_фильма"]);
                    for (int i = 0; i < cmbFilm.Items.Count; i++)
                    {
                        if (cmbFilm.Items[i] is FilmItem item && item.Value == filmId)
                        {
                            cmbFilm.SelectedIndex = i;
                            break;
                        }
                    }

                    // Выбор зала
                    int hallId = Convert.ToInt32(row["Номер_зала"]);
                    for (int i = 0; i < cmbHall.Items.Count; i++)
                    {
                        if (cmbHall.Items[i] is HallItem item && item.Value == hallId)
                        {
                            cmbHall.SelectedIndex = i;
                            break;
                        }
                    }

                    dtpDate.Value = Convert.ToDateTime(row["Дата"]);

                    TimeSpan startTime = (TimeSpan)row["Время_начала"];
                    dtpStartTime.Value = DateTime.Today.Add(startTime);

                    TimeSpan endTime = (TimeSpan)row["Время_окончания"];
                    dtpEndTime.Value = DateTime.Today.Add(endTime);

                    numPrice.Value = Convert.ToDecimal(row["Цена_билета"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных сеанса: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (!(cmbFilm.SelectedItem is FilmItem film) || film.Value == 0)
            {
                MessageBox.Show("Выберите фильм!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbFilm.Focus();
                return;
            }

            if (!(cmbHall.SelectedItem is HallItem hall) || hall.Value == 0)
            {
                MessageBox.Show("Выберите зал!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbHall.Focus();
                return;
            }

            if (dtpEndTime.Value <= dtpStartTime.Value)
            {
                MessageBox.Show("Время окончания должно быть позже времени начала!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Проверка на пересечение сеансов
                string checkQuery = @"
                    SELECT COUNT(*) FROM Сеанс 
                    WHERE Номер_зала = @Зал 
                    AND Дата = @Дата 
                    AND Код_сеанса != @id
                    AND (
                        (@Начало >= Время_начала AND @Начало < Время_окончания)
                        OR (@Конец > Время_начала AND @Конец <= Время_окончания)
                        OR (@Начало <= Время_начала AND @Конец >= Время_окончания)
                    )";

                var checkParams = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@Зал", hall.Value),
                    new System.Data.SqlClient.SqlParameter("@Дата", dtpDate.Value.Date),
                    new System.Data.SqlClient.SqlParameter("@Начало", dtpStartTime.Value.TimeOfDay),
                    new System.Data.SqlClient.SqlParameter("@Конец", dtpEndTime.Value.TimeOfDay),
                    new System.Data.SqlClient.SqlParameter("@id", sessionId ?? 0)
                };

                int conflicts = Convert.ToInt32(dbService.ExecuteScalar(checkQuery, checkParams));
                if (conflicts > 0)
                {
                    MessageBox.Show("В выбранном зале уже есть сеанс в это время!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Получаем первого сотрудника для назначения на сеанс (администратор по умолчанию)
                int defaultEmployeeId = 1;
                try
                {
                    object result = dbService.ExecuteScalar("SELECT TOP 1 Код_сотрудника FROM Сотрудники ORDER BY Код_сотрудника");
                    if (result != null && result != DBNull.Value)
                        defaultEmployeeId = Convert.ToInt32(result);
                }
                catch { }

                var parameters = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@Код_фильма", film.Value),
                    new System.Data.SqlClient.SqlParameter("@Номер_зала", hall.Value),
                    new System.Data.SqlClient.SqlParameter("@Дата", dtpDate.Value.Date),
                    new System.Data.SqlClient.SqlParameter("@Время_начала", dtpStartTime.Value.TimeOfDay),
                    new System.Data.SqlClient.SqlParameter("@Время_окончания", dtpEndTime.Value.TimeOfDay),
                    new System.Data.SqlClient.SqlParameter("@Цена_билета", numPrice.Value),
                    new System.Data.SqlClient.SqlParameter("@Код_сотрудника", defaultEmployeeId)
                };

                string query;
                if (isEditMode)
                {
                    query = @"UPDATE Сеанс SET 
                        Код_фильма = @Код_фильма,
                        Номер_зала = @Номер_зала,
                        Дата = @Дата,
                        Время_начала = @Время_начала,
                        Время_окончания = @Время_окончания,
                        Цена_билета = @Цена_билета
                        WHERE Код_сеанса = @id";

                    var paramList = new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>(parameters);
                    paramList.Add(new System.Data.SqlClient.SqlParameter("@id", sessionId.Value));
                    dbService.ExecuteNonQuery(query, paramList.ToArray());
                }
                else
                {
                    query = @"INSERT INTO Сеанс (Код_фильма, Номер_зала, Дата, Время_начала, Время_окончания, Цена_билета, Код_сотрудника)
                        VALUES (@Код_фильма, @Номер_зала, @Дата, @Время_начала, @Время_окончания, @Цена_билета, @Код_сотрудника)";
                    dbService.ExecuteNonQuery(query, parameters);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Вспомогательный класс для фильмов в ComboBox
    /// </summary>
    public class FilmItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public int Duration { get; set; }
        public override string ToString() => Text;
    }

    /// <summary>
    /// Вспомогательный класс для залов в ComboBox
    /// </summary>
    public class HallItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public int Seats { get; set; }
        public override string ToString() => Text;
    }
}