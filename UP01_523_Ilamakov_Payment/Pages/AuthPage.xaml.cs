using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using UP01_523_Ilamakov_Payment;
using UP01_523_Ilamakov_Payment.Pages;

namespace UP01_Ilamakov_Payment.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        // Хеширование пароля (SHA1, как в методичке)
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(
                    hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                        .Select(x => x.ToString("X2")));
            }
        }
        private string currentCaptcha = "";

        // Генерация капчи
        private void GenerateCaptcha()
        {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz23456789";
            var random = new Random();
            currentCaptcha = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            CaptchaText.Text = currentCaptcha;
            TextBoxCaptcha.Text = "";
        }

        private void CaptchaSwitch()
        {
            CaptchaPanel.Visibility = Visibility.Visible;
            GenerateCaptcha();
        }

        private void CaptchaRefresh_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка капчи, если она видна
            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(TextBoxCaptcha.Text) ||
                    TextBoxCaptcha.Text != currentCaptcha)
                {
                    MessageBox.Show("Неверный код капчи", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    GenerateCaptcha();
                    return;
                }
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var user = db.Users
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Login == TextBoxLogin.Text &&
                                             u.Password == hashedPassword);

                    if (user == null)
                    {
                        failedAttempts++;
                        MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        if (failedAttempts >= 3)
                        {
                            CaptchaSwitch();
                        }
                        return;
                    }

                    // Успешный вход — сбрасываем счётчик и капчу
                    failedAttempts = 0;
                    CaptchaPanel.Visibility = Visibility.Collapsed;

                    MessageBox.Show($"Добро пожаловать, {user.FIO ?? user.Login}!", "Успех");

                    switch (user.Role)
                    {
                        case "Admin":
                        case "Администратор":
                            MessageBox.Show("Вы вошли как Администратор");
                            NavigationService?.Navigate(new AdminPage(user));
                            break;

                        case "User":
                        case "Пользователь":
                            NavigationService?.Navigate(new UserPage(user));
                            break;

                        default:
                            MessageBox.Show($"Неизвестная роль: {user.Role}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных:\n" + ex.Message, "Ошибка");
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }
    }
}