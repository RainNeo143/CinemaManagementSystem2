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
        private Label lblWelcome;

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
            this.lblWelcome = new Label();

            this.SuspendLayout();

            // lblWelcome
            this.lblWelcome.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblWelcome.Location = new Point(20, 15);
            this.lblWelcome.Size = new Size(800, 25);
            this.lblWelcome.Text = $"Панель администратора - {currentUser.FullName}";

            // TabControl
            this.tabControl.Location = new Point(20, 50);
            this.tabControl.Size = new Size(1140, 600);

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
            this.ClientSize = new Size(1180, 680);
            this.Controls.Add(this.lblWelcome);
            this.Controls.Add(this.tabControl);
            this.Name = "AdminForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Система управления кинотеатром - Администратор";
            this.ResumeLayout(false);
        }

        private TabPage CreateStatisticsTab()
        {
            TabPage tab = new TabPage("Статистика посещаемости");
            Panel panel = new Panel { Dock = DockStyle.Fill };

            // Панель фильтров
            Panel filterPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1100, 60),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblFrom = new Label
            {
                Text = "С:",
                Location = new Point(10, 20),
                Size = new Size(30, 20)
            };

            this.dtpFrom.Location = new Point(45, 17);
            this.dtpFrom.Size = new Size(150, 20);
            this.dtpFrom.Value = DateTime.Today.AddMonths(-1);

            Label lblTo = new Label
            {
                Text = "По:",
                Location = new Point(210, 20),
                Size = new Size(30, 20)
            };

            this.dtpTo.Location = new Point(245, 17);
            this.dtpTo.Size = new Size(150, 20);
            this.dtpTo.Value = DateTime.Today;

            this.btnLoadStatistics.Location = new Point(410, 15);
            this.btnLoadStatistics.Size = new Size(150, 30);
            this.btnLoadStatistics.Text = "Загрузить";
            this.btnLoadStatistics.Click += btnLoadStatistics_Click;

            filterPanel.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnLoadStatistics });

            // DataGridView для статистики
            this.dgvStatistics.Location = new Point(10, 80);
            this.dgvStatistics.Size = new Size(1100, 450);
            this.dgvStatistics.ReadOnly = true;
            this.dgvStatistics.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStatistics.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Итоговая информация
            Label lblTotal = new Label
            {
                Location = new Point(10, 540),
                Size = new Size(1100, 20),
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold),
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
            TabPage tab = new TabPage("Популярные фильмы");
            Panel panel = new Panel { Dock = DockStyle.Fill };

            Label lblTitle = new Label
            {
                Text = "ТОП-10 самых популярных фильмов",
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                Location = new Point(10, 15),
                Size = new Size(400, 25)
            };

            this.dgvPopularFilms.Location = new Point(10, 50);
            this.dgvPopularFilms.Size = new Size(1100, 500);
            this.dgvPopularFilms.ReadOnly = true;
            this.dgvPopularFilms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPopularFilms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(this.dgvPopularFilms);
            tab.Controls.Add(panel);

            return tab;
        }

        private TabPage CreateChartsTab()
        {
            TabPage tab = new TabPage("Графики");
            Panel panel = new Panel { Dock = DockStyle.Fill };

            ((System.ComponentModel.ISupportInitialize)(this.chartRevenue)).BeginInit();

            this.chartRevenue.Location = new Point(10, 10);
            this.chartRevenue.Size = new Size(1100, 540);
            this.chartRevenue.Text = "График выручки";

            ChartArea chartArea = new ChartArea();
            chartArea.Name = "ChartArea1";
            this.chartRevenue.ChartAreas.Add(chartArea);

            Legend legend = new Legend();
            legend.Name = "Legend1";
            this.chartRevenue.Legends.Add(legend);

            this.chartRevenue.Name = "chartRevenue";

            Series series = new Series
            {
                Name = "Выручка",
                ChartType = SeriesChartType.Column,
                XValueType = ChartValueType.String
            };
            this.chartRevenue.Series.Add(series);

            Title title = new Title
            {
                Text = "Выручка по фильмам",
                Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold)
            };
            this.chartRevenue.Titles.Add(title);

            ((System.ComponentModel.ISupportInitialize)(this.chartRevenue)).EndInit();

            panel.Controls.Add(this.chartRevenue);
            tab.Controls.Add(panel);

            return tab;
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
                lblTotal.Text = $"Всего бронирований: {totalBookings} | Общая выручка: {totalRevenue:N2} тг";
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
                chartRevenue.ChartAreas[0].AxisY.Title = "Выручка (тг)";
                chartRevenue.ChartAreas[0].AxisX.Title = "Фильмы";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения графика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}