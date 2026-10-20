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

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())   // ← если контекст называется иначе — поправь
                {
                    var user = db.Users          // ← если сущность называется User, то db.Users или db.User
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
                            MessageBox.Show("Слишком много неудачных попыток. Позже добавим капчу.", "Внимание");
                        }
                        return;
                    }

                    // Успешный вход
                    MessageBox.Show($"Добро пожаловать, {user.FIO ?? user.Login}!", "Успех");

                    // Пока просто сообщение. Позже добавим переход на страницы по ролям
                    switch (user.Role)
                    {
                        case "Admin":
                        case "Администратор":
                            MessageBox.Show("Вы вошли как Администратор");
                            // NavigationService?.Navigate(new AdminPage());
                            break;

                        case "User":
                        case "Пользователь":
                            MessageBox.Show("Вы вошли как Пользователь");
                            // NavigationService?.Navigate(new UserPage());
                            break;

                        default:
                            MessageBox.Show("Неизвестная роль пользователя");
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

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница смены пароля будет создана позже");
        }
    }
}