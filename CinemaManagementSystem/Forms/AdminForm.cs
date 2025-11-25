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
        private TabControl tabControl;
        private DataGridView dgvStatistics;
        private DataGridView dgvPopularFilms;
        private Chart chartRevenue;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnLoadStatistics;
        private Button btnLogout;
        private Label lblWelcome;
        private Panel headerPanel;

        public AdminForm(User user)
        {
            currentUser = user;
            bookingService = new BookingService();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.dgvStatistics = new DataGridView();
            this.dgvPopularFilms = new DataGridView();
            this.chartRevenue = new Chart();
            this.dtpFrom = new DateTimePicker();
            this.dtpTo = new DateTimePicker();
            this.btnLoadStatistics = new Button();
            this.btnLogout = new Button();
            this.lblWelcome = new Label();
            this.headerPanel = new Panel();

            this.SuspendLayout();

            // Header Panel
            this.headerPanel.BackColor = Color.FromArgb(192, 57, 43);
            this.headerPanel.Location = new Point(0, 0);
            this.headerPanel.Size = new Size(1200, 70);
            this.headerPanel.Dock = DockStyle.Top;

            // lblWelcome
            this.lblWelcome.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblWelcome.ForeColor = Color.White;
            this.lblWelcome.Location = new Point(20, 20);
            this.lblWelcome.Size = new Size(900, 30);
            this.lblWelcome.Text = $"⚙️ Панель администратора - {currentUser.FullName}";

            // btnLogout
            this.btnLogout.BackColor = Color.FromArgb(44, 62, 80);
            this.btnLogout.FlatStyle = FlatStyle.Flat;
            this.btnLogout.ForeColor = Color.White;
            this.btnLogout.Location = new Point(1050, 20);
            this.btnLogout.Size = new Size(120, 30);
            this.btnLogout.Text = "Выход";
            this.btnLogout.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnLogout.Cursor = Cursors.Hand;
            this.btnLogout.Click += (s, e) => {
                if (MessageBox.Show("Выйти из системы?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Restart();
                }
            };

            this.headerPanel.Controls.Add(this.lblWelcome);
            this.headerPanel.Controls.Add(this.btnLogout);

            // TabControl
            this.tabControl.Location = new Point(20, 90);
            this.tabControl.Size = new Size(1160, 620);
            this.tabControl.Font = new Font("Segoe UI", 10F);

            // Вкладка "Статистика посещаемости"
            TabPage tabStatistics = CreateStatisticsTab();
            this.tabControl.TabPages.Add(tabStatistics);

            // Вкладка "Популярные фильмы"
            TabPage tabPopular = CreatePopularFilmsTab();
            this.tabControl.TabPages.Add(tabPopular);

            // Вкладка "Графики выручки"
            TabPage tabCharts = CreateChartsTab();
            this.tabControl.TabPages.Add(tabCharts);

            // AdminForm
            this.ClientSize = new Size(1200, 730);
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.tabControl);
            this.Name = "AdminForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Кинотеатр - Администратор";
            this.Icon = SystemIcons.Shield;
            this.ResumeLayout(false);
        }

        private TabPage CreateStatisticsTab()
        {
            TabPage tab = new TabPage("📊 Статистика посещаемости");
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Панель фильтров
            Panel filterPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1120, 70),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblFrom = new Label
            {
                Text = "С:",
                Location = new Point(15, 25),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            this.dtpFrom.Location = new Point(50, 22);
            this.dtpFrom.Size = new Size(180, 25);
            this.dtpFrom.Value = DateTime.Today.AddMonths(-1);
            this.dtpFrom.Font = new Font("Segoe UI", 9F);

            Label lblTo = new Label
            {
                Text = "По:",
                Location = new Point(250, 25),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            this.dtpTo.Location = new Point(285, 22);
            this.dtpTo.Size = new Size(180, 25);
            this.dtpTo.Value = DateTime.Today;
            this.dtpTo.Font = new Font("Segoe UI", 9F);

            this.btnLoadStatistics.Location = new Point(485, 17);
            this.btnLoadStatistics.Size = new Size(150, 35);
            this.btnLoadStatistics.Text = "🔍 Загрузить";
            this.btnLoadStatistics.BackColor = Color.FromArgb(52, 152, 219);
            this.btnLoadStatistics.ForeColor = Color.White;
            this.btnLoadStatistics.FlatStyle = FlatStyle.Flat;
            this.btnLoadStatistics.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnLoadStatistics.Cursor = Cursors.Hand;
            this.btnLoadStatistics.Click += btnLoadStatistics_Click;

            filterPanel.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnLoadStatistics });

            // DataGridView для статистики
            this.dgvStatistics.Location = new Point(10, 90);
            this.dgvStatistics.Size = new Size(1120, 420);
            this.dgvStatistics.ReadOnly = true;
            this.dgvStatistics.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStatistics.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvStatistics.BackgroundColor = Color.White;
            this.dgvStatistics.BorderStyle = BorderStyle.None;
            this.dgvStatistics.RowTemplate.Height = 30;

            StyleDataGridView(this.dgvStatistics);

            // Итоговая информация
            Label lblTotal = new Label
            {
                Location = new Point(10, 520),
                Size = new Size(1120, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Name = "lblTotalInfo"
            };

            panel.Controls.Add(filterPanel);
            panel.Controls.Add(this.dgvStatistics);
            panel.Controls.Add(lblTotal);
            tab.Controls.Add(panel);

            return tab;
        }

        private TabPage CreatePopularFilmsTab()
        {
            TabPage tab = new TabPage("🏆 Популярные фильмы");
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            Label lblTitle = new Label
            {
                Text = "ТОП-10 самых популярных фильмов",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(10, 15),
                Size = new Size(500, 30)
            };

            this.dgvPopularFilms.Location = new Point(10, 55);
            this.dgvPopularFilms.Size = new Size(1120, 500);
            this.dgvPopularFilms.ReadOnly = true;
            this.dgvPopularFilms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPopularFilms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvPopularFilms.BackgroundColor = Color.White;
            this.dgvPopularFilms.BorderStyle = BorderStyle.None;
            this.dgvPopularFilms.RowTemplate.Height = 35;

            StyleDataGridView(this.dgvPopularFilms);

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(this.dgvPopularFilms);
            tab.Controls.Add(panel);

            return tab;
        }

        private TabPage CreateChartsTab()
        {
            TabPage tab = new TabPage("📈 Графики");
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };

            ((System.ComponentModel.ISupportInitialize)(this.chartRevenue)).BeginInit();

            this.chartRevenue.Location = new Point(10, 10);
            this.chartRevenue.Size = new Size(1120, 560);
            this.chartRevenue.BackColor = Color.White;

            ChartArea chartArea = new ChartArea();
            chartArea.Name = "ChartArea1";
            chartArea.BackColor = Color.WhiteSmoke;
            chartArea.BorderWidth = 2;
            chartArea.BorderColor = Color.FromArgb(189, 195, 199);
            this.chartRevenue.ChartAreas.Add(chartArea);

            Legend legend = new Legend();
            legend.Name = "Legend1";
            legend.Font = new Font("Segoe UI", 10F);
            this.chartRevenue.Legends.Add(legend);

            this.chartRevenue.Name = "chartRevenue";

            Series series = new Series
            {
                Name = "Выручка",
                ChartType = SeriesChartType.Column,
                XValueType = ChartValueType.String,
                Color = Color.FromArgb(52, 152, 219),
                BorderWidth = 2
            };
            this.chartRevenue.Series.Add(series);

            Title title = new Title
            {
                Text = "Выручка по фильмам (ТОП-10)",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.chartRevenue.Titles.Add(title);

            ((System.ComponentModel.ISupportInitialize)(this.chartRevenue)).EndInit();

            panel.Controls.Add(this.chartRevenue);
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

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(192, 57, 43);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(224, 224, 224);
        }

        private void LoadData()
        {
            LoadStatistics(dtpFrom.Value, dtpTo.Value);
            LoadPopularFilms();
            LoadChart();
        }

        private void btnLoadStatistics_Click(object sender, EventArgs e)
        {
            LoadStatistics(dtpFrom.Value, dtpTo.Value);
            LoadChart();
        }

        private void LoadStatistics(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                DataTable stats = bookingService.GetStatistics(dateFrom, dateTo);
                dgvStatistics.DataSource = stats;

                // Подсчет итогов
                decimal totalRevenue = 0;
                int totalBookings = 0;

                foreach (DataRow row in stats.Rows)
                {
                    if (row["Общая_сумма"] != DBNull.Value)
                        totalRevenue += Convert.ToDecimal(row["Общая_сумма"]);

                    if (row["Количество_бронирований"] != DBNull.Value)
                        totalBookings += Convert.ToInt32(row["Количество_бронирований"]);
                }

                Label lblTotal = (Label)tabControl.TabPages[0].Controls[0].Controls["lblTotalInfo"];
                lblTotal.Text = $"💰 Всего бронирований: {totalBookings} | Общая выручка: {totalRevenue:N0} тг";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPopularFilms()
        {
            try
            {
                DataTable popular = bookingService.GetPopularFilms(10);
                dgvPopularFilms.DataSource = popular;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки популярных фильмов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChart()
        {
            try
            {
                DataTable popular = bookingService.GetPopularFilms(10);

                chartRevenue.Series[0].Points.Clear();

                foreach (DataRow row in popular.Rows)
                {
                    string filmName = row["Фильм"].ToString();
                    if (filmName.Length > 20)
                        filmName = filmName.Substring(0, 17) + "...";

                    double revenue = row["Выручка"] != DBNull.Value ?
                        Convert.ToDouble(row["Выручка"]) : 0;

                    chartRevenue.Series[0].Points.AddXY(filmName, revenue);
                }

                chartRevenue.ChartAreas[0].AxisX.Interval = 1;
                chartRevenue.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
                chartRevenue.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
                chartRevenue.ChartAreas[0].AxisY.Title = "Выручка (тг)";
                chartRevenue.ChartAreas[0].AxisY.TitleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
                chartRevenue.ChartAreas[0].AxisX.Title = "Фильмы";
                chartRevenue.ChartAreas[0].AxisX.TitleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения графика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}