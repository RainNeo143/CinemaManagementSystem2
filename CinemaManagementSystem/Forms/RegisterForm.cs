using System;
using System.Windows.Forms;
using CinemaManagementSystem.Services;

namespace CinemaManagementSystem.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly AuthService authService;

        public RegisterForm()
        {
            InitializeComponent();
            authService = new AuthService();
        }

        private void InitializeComponent()
        {
            this.txtLogin = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtPhone = new System.Windows.Forms.MaskedTextBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(120, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(200, 24);
            this.lblTitle.Text = "Регистрация";

            // Логин
            this.Controls.Add(CreateLabel("Логин:", 50, 60));
            this.txtLogin.Location = new System.Drawing.Point(150, 57);
            this.txtLogin.Size = new System.Drawing.Size(250, 20);
            this.Controls.Add(this.txtLogin);

            // Пароль
            this.Controls.Add(CreateLabel("Пароль:", 50, 90));
            this.txtPassword.Location = new System.Drawing.Point(150, 87);
            this.txtPassword.Size = new System.Drawing.Size(250, 20);
            this.txtPassword.PasswordChar = '●';
            this.Controls.Add(this.txtPassword);

            // Подтверждение пароля
            this.Controls.Add(CreateLabel("Повторите:", 50, 120));
            this.txtConfirmPassword.Location = new System.Drawing.Point(150, 117);
            this.txtConfirmPassword.Size = new System.Drawing.Size(250, 20);
            this.txtConfirmPassword.PasswordChar = '●';
            this.Controls.Add(this.txtConfirmPassword);

            // ФИО
            this.Controls.Add(CreateLabel("ФИО:", 50, 150));
            this.txtFullName.Location = new System.Drawing.Point(150, 147);
            this.txtFullName.Size = new System.Drawing.Size(250, 20);
            this.Controls.Add(this.txtFullName);

            // Email
            this.Controls.Add(CreateLabel("Email:", 50, 180));
            this.txtEmail.Location = new System.Drawing.Point(150, 177);
            this.txtEmail.Size = new System.Drawing.Size(250, 20);
            this.Controls.Add(this.txtEmail);

            // Телефон
            this.Controls.Add(CreateLabel("Телефон:", 50, 210));
            this.txtPhone.Location = new System.Drawing.Point(150, 207);
            this.txtPhone.Size = new System.Drawing.Size(250, 20);
            this.txtPhone.Mask = "8 (000) 000-00-00";
            this.Controls.Add(this.txtPhone);

            // Кнопки
            this.btnRegister.Location = new System.Drawing.Point(150, 250);
            this.btnRegister.Size = new System.Drawing.Size(120, 35);
            this.btnRegister.Text = "Зарегистрироваться";
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            this.Controls.Add(this.btnRegister);

            this.btnCancel.Location = new System.Drawing.Point(280, 250);
            this.btnCancel.Size = new System.Drawing.Size(120, 35);
            this.btnCancel.Text = "Отмена";
            this.btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(this.btnCancel);

            // Форма
            this.ClientSize = new System.Drawing.Size(450, 320);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RegisterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Регистрация нового пользователя";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private System.Windows.Forms.TextBox txtLogin;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.MaskedTextBox txtPhone;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblTitle;

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtLogin.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success = authService.Register(
                    txtLogin.Text.Trim(),
                    txtPassword.Text,
                    txtFullName.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtPhone.Text.Trim()
                );

                if (success)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}