using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    /// <summary>
    /// Форма добавления/редактирования зала
    /// </summary>
    public class HallEditForm : Form
    {
        private readonly DatabaseService dbService;
        private readonly int? hallId;
        private readonly bool isEditMode;

        // Контролы формы
        private TextBox txtName;
        private NumericUpDown numSeats;
        private NumericUpDown numRows;
        private NumericUpDown numSeatsPerRow;
        private CheckBox chkVip;
        private Button btnSave;
        private Button btnCancel;

        public HallEditForm(int? hallId = null)
        {
            this.hallId = hallId;
            this.isEditMode = hallId.HasValue;
            this.dbService = new DatabaseService();
            InitializeComponent();

            if (isEditMode)
                LoadHallData();
        }

        private void InitializeComponent()
        {
            this.Text = isEditMode ? "✏️ Редактирование зала" : "➕ Добавление зала";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 25;
            int labelX = 20;
            int controlX = 180;
            int controlWidth = 220;
            int rowHeight = 40;

            // Название зала
            AddLabel("🏛️ Название зала *:", labelX, y);
            txtName = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 28),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(txtName);
            y += rowHeight;

            y += 0; // Тип зала не используется в базе данных

            // Количество мест
            AddLabel("💺 Количество мест *:", labelX, y);
            numSeats = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(120, 28),
                Font = new Font("Segoe UI", 10F),
                Minimum = 10,
                Maximum = 500,
                Value = 100
            };
            this.Controls.Add(numSeats);
            y += rowHeight;

            // Количество рядов (опционально)
            AddLabel("📊 Количество рядов:", labelX, y);
            numRows = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 10F),
                Minimum = 1,
                Maximum = 30,
                Value = 10
            };
            this.Controls.Add(numRows);

            Label lblX = new Label
            {
                Text = "×",
                Location = new Point(controlX + 90, y + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            this.Controls.Add(lblX);

            numSeatsPerRow = new NumericUpDown
            {
                Location = new Point(controlX + 115, y),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 10F),
                Minimum = 5,
                Maximum = 30,
                Value = 10
            };
            this.Controls.Add(numSeatsPerRow);

            // Автоматическое обновление количества мест
            numRows.ValueChanged += (s, e) => UpdateTotalSeats();
            numSeatsPerRow.ValueChanged += (s, e) => UpdateTotalSeats();
            y += rowHeight;

            // VIP места
            chkVip = new CheckBox
            {
                Text = "⭐ Зал с VIP местами",
                Location = new Point(controlX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(chkVip);
            y += rowHeight + 15;

            // Кнопки
            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(controlX, y),
                Size = new Size(130, 45),
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
                Location = new Point(controlX + 145, y),
                Size = new Size(110, 45),
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

        private void UpdateTotalSeats()
        {
            numSeats.Value = numRows.Value * numSeatsPerRow.Value;
        }

        private void LoadHallData()
        {
            try
            {
                string query = "SELECT * FROM Залы WHERE Номер_зала = @id";
                DataTable dt = dbService.ExecuteQuery(query, new SqlParameter("@id", hallId.Value));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    txtName.Text = row["Наименование"]?.ToString() ?? "";
                    numSeats.Value = row["Количество_мест"] != DBNull.Value ? Convert.ToInt32(row["Количество_мест"]) : 100;

                    // Примерный расчёт рядов
                    int seats = (int)numSeats.Value;
                    int rows = (int)Math.Ceiling(Math.Sqrt(seats));
                    int seatsPerRow = (int)Math.Ceiling((double)seats / rows);

                    if (rows >= numRows.Minimum && rows <= numRows.Maximum)
                        numRows.Value = rows;
                    if (seatsPerRow >= numSeatsPerRow.Minimum && seatsPerRow <= numSeatsPerRow.Maximum)
                        numSeatsPerRow.Value = seatsPerRow;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных зала: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название зала!", "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                if (isEditMode)
                {
                    // Обновление
                    string updateQuery = @"UPDATE Залы SET 
                        Наименование = @Наименование,
                        Количество_мест = @Количество_мест
                        WHERE Номер_зала = @id";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Наименование", txtName.Text.Trim()),
                        new SqlParameter("@Количество_мест", (int)numSeats.Value),
                        new SqlParameter("@id", hallId.Value)
                    };

                    dbService.ExecuteNonQuery(updateQuery, parameters);
                }
                else
                {
                    // Вставка нового зала - нужно указать Номер_зала, т.к. это NOT IDENTITY
                    // Получаем максимальный номер зала
                    object maxId = dbService.ExecuteScalar("SELECT ISNULL(MAX(Номер_зала), 0) + 1 FROM Залы");
                    int newHallNumber = Convert.ToInt32(maxId);

                    string insertQuery = @"INSERT INTO Залы (Номер_зала, Наименование, Количество_мест)
                        VALUES (@Номер_зала, @Наименование, @Количество_мест);
                        SELECT @Номер_зала;";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Номер_зала", newHallNumber),
                        new SqlParameter("@Наименование", txtName.Text.Trim()),
                        new SqlParameter("@Количество_мест", (int)numSeats.Value)
                    };

                    object result = dbService.ExecuteScalar(insertQuery, parameters);
                    int newHallId = Convert.ToInt32(result);

                    // Опционально: создание мест в зале
                    if (MessageBox.Show("Создать места в зале автоматически?", "Создание мест",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        GenerateSeats(newHallId);
                    }
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

        private void GenerateSeats(int hallId)
        {
            try
            {
                int rows = (int)numRows.Value;
                int seatsPerRow = (int)numSeatsPerRow.Value;
                bool hasVip = chkVip.Checked;

                for (int row = 1; row <= rows; row++)
                {
                    // Последние 2 ряда - VIP (если включено)
                    string seatType = (hasVip && row > rows - 2) ? "VIP" : "Обычное";

                    for (int seat = 1; seat <= seatsPerRow; seat++)
                    {
                        string insertSeat = @"INSERT INTO Места_в_залах (Номер_зала, Ряд, Номер_места, Тип_места)
                            VALUES (@Номер_зала, @Ряд, @Номер_места, @Тип_места)";

                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Номер_зала", hallId),
                            new SqlParameter("@Ряд", row),
                            new SqlParameter("@Номер_места", seat),
                            new SqlParameter("@Тип_места", seatType)
                        };

                        dbService.ExecuteNonQuery(insertSeat, parameters);
                    }
                }

                MessageBox.Show($"Создано {rows * seatsPerRow} мест в зале!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания мест: {ex.Message}\n\nЗал создан, но места нужно добавить вручную.",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}