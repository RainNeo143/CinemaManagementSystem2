using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    /// <summary>
    /// Форма добавления/редактирования фильма
    /// </summary>
    public class FilmEditForm : Form
    {
        private readonly DatabaseService dbService;
        private readonly int? filmId;
        private readonly bool isEditMode;

        // Контролы формы
        private TextBox txtTitle;
        private ComboBox cmbGenre;
        private NumericUpDown numDuration;
        private TextBox txtDirector;
        private TextBox txtCountry;
        private TextBox txtActors;
        private ComboBox cmbAge;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;

        public FilmEditForm(int? filmId = null)
        {
            this.filmId = filmId;
            this.isEditMode = filmId.HasValue;
            this.dbService = new DatabaseService();
            InitializeComponent();
            LoadGenres();

            if (isEditMode)
                LoadFilmData();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "✏️ Редактирование фильма" : "➕ Добавление фильма";
            this.Size = new Size(550, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20;
            int labelWidth = 140;
            int controlX = 160;
            int controlWidth = 350;
            int rowHeight = 38;

            // Название
            AddLabel("🎬 Название *:", 20, y);
            txtTitle = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(txtTitle);
            y += rowHeight;

            // Жанр
            AddLabel("🎭 Жанр:", 20, y);
            cmbGenre = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(cmbGenre);
            y += rowHeight;

            // Длительность
            AddLabel("⏱️ Длительность (мин):", 20, y);
            numDuration = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10F),
                Minimum = 1,
                Maximum = 500,
                Value = 90
            };
            this.Controls.Add(numDuration);
            y += rowHeight;

            // Режиссёр
            AddLabel("🎥 Режиссёр:", 20, y);
            txtDirector = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(txtDirector);
            y += rowHeight;

            // Страна
            AddLabel("🌍 Страна:", 20, y);
            txtCountry = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(txtCountry);
            y += rowHeight;

            // Актёры
            AddLabel("👥 Актёры:", 20, y);
            txtActors = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(txtActors);
            y += rowHeight;

            // Возрастное ограничение
            AddLabel("🔞 Возраст:", 20, y);
            cmbAge = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbAge.Items.AddRange(new object[] { "0+", "6+", "12+", "16+", "18+" });
            cmbAge.SelectedIndex = 0;
            this.Controls.Add(cmbAge);
            y += rowHeight;

            // Описание
            AddLabel("📝 Описание:", 20, y);
            txtDescription = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 80),
                Font = new Font("Segoe UI", 10F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtDescription);
            y += 95;

            // Кнопки
            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(controlX, y),
                Size = new Size(140, 45),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(controlX + 155, y),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
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

        private void LoadGenres()
        {
            try
            {
                cmbGenre.Items.Clear();
                cmbGenre.Items.Add(new ComboItem { Value = null, Text = "— Не выбран —" });

                string query = "SELECT Код_жанра, Наименование FROM Жанры ORDER BY Наименование";
                DataTable dt = dbService.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    cmbGenre.Items.Add(new ComboItem
                    {
                        Value = Convert.ToInt32(row["Код_жанра"]),
                        Text = row["Наименование"].ToString()
                    });
                }

                cmbGenre.DisplayMember = "Text";
                cmbGenre.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки жанров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFilmData()
        {
            try
            {
                string query = "SELECT * FROM Фильмы WHERE Код_фильма = @id";
                DataTable dt = dbService.ExecuteQuery(query, new SqlParameter("@id", filmId.Value));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    txtTitle.Text = row["Наименование"]?.ToString() ?? "";
                    numDuration.Value = row["Длительность"] != DBNull.Value ? Convert.ToInt32(row["Длительность"]) : 90;
                    txtDirector.Text = row["Режиссёр"]?.ToString() ?? "";
                    txtCountry.Text = row["Страна"]?.ToString() ?? "";
                    txtActors.Text = row["Актёры"]?.ToString() ?? "";
                    txtDescription.Text = row["Описание"]?.ToString() ?? "";

                    // Выбор жанра
                    if (row["Код_жанра"] != DBNull.Value)
                    {
                        int genreId = Convert.ToInt32(row["Код_жанра"]);
                        for (int i = 0; i < cmbGenre.Items.Count; i++)
                        {
                            if (cmbGenre.Items[i] is ComboItem item && item.Value == genreId)
                            {
                                cmbGenre.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Выбор возрастного ограничения
                    string age = row["Возрастные_ограничения"]?.ToString() ?? "0+";
                    int ageIndex = cmbAge.Items.IndexOf(age);
                    if (ageIndex >= 0) cmbAge.SelectedIndex = ageIndex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных фильма: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите название фильма!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            try
            {
                int? genreId = null;
                if (cmbGenre.SelectedItem is ComboItem selectedGenre)
                    genreId = selectedGenre.Value;

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Наименование", txtTitle.Text.Trim()),
                    new SqlParameter("@Код_жанра", genreId.HasValue ? (object)genreId.Value : DBNull.Value),
                    new SqlParameter("@Длительность", (int)numDuration.Value),
                    new SqlParameter("@Режиссёр", string.IsNullOrWhiteSpace(txtDirector.Text) ? DBNull.Value : (object)txtDirector.Text.Trim()),
                    new SqlParameter("@Страна", string.IsNullOrWhiteSpace(txtCountry.Text) ? DBNull.Value : (object)txtCountry.Text.Trim()),
                    new SqlParameter("@Актёры", string.IsNullOrWhiteSpace(txtActors.Text) ? DBNull.Value : (object)txtActors.Text.Trim()),
                    new SqlParameter("@Возрастные_ограничения", cmbAge.SelectedItem?.ToString() ?? "0+"),
                    new SqlParameter("@Описание", string.IsNullOrWhiteSpace(txtDescription.Text) ? DBNull.Value : (object)txtDescription.Text.Trim())
                };

                if (isEditMode)
                {
                    // Обновление
                    var paramList = new System.Collections.Generic.List<SqlParameter>(parameters);
                    paramList.Add(new SqlParameter("@id", filmId.Value));

                    string updateQuery = @"UPDATE Фильмы SET 
                        Наименование = @Наименование,
                        Код_жанра = @Код_жанра,
                        Длительность = @Длительность,
                        Режиссёр = @Режиссёр,
                        Страна = @Страна,
                        Актёры = @Актёры,
                        Возрастные_ограничения = @Возрастные_ограничения,
                        Описание = @Описание
                        WHERE Код_фильма = @id";

                    dbService.ExecuteNonQuery(updateQuery, paramList.ToArray());
                }
                else
                {
                    // Вставка
                    string insertQuery = @"INSERT INTO Фильмы 
                        (Наименование, Код_жанра, Длительность, Режиссёр, Страна, Актёры, Возрастные_ограничения, Описание)
                        VALUES (@Наименование, @Код_жанра, @Длительность, @Режиссёр, @Страна, @Актёры, @Возрастные_ограничения, @Описание)";

                    dbService.ExecuteNonQuery(insertQuery, parameters);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Вспомогательный класс для ComboBox
        private class ComboItem
        {
            public int? Value { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }
    }
}