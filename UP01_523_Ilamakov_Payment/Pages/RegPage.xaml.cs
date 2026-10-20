using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Ilamakov_Payment.Pages;

namespace UP01_523_Ilamakov_Payment.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            // Проверки
            if (string.IsNullOrWhiteSpace(TextBoxFIO.Text) ||
                string.IsNullOrWhiteSpace(TextBoxLogin.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(PasswordBoxConfirm.Password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password != PasswordBoxConfirm.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    // Проверка, существует ли уже такой логин
                    bool loginExists = db.Users.Any(u => u.Login == TextBoxLogin.Text);
                    if (loginExists)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Генерация нового ID
                    int newId = db.Users.Any() ? db.Users.Max(u => u.ID) + 1 : 1;

                    var newUser = new User
                    {
                        ID = newId,
                        Login = TextBoxLogin.Text.Trim(),
                        Password = AuthPage.GetHash(PasswordBox.Password),
                        Role = "User",
                        FIO = TextBoxFIO.Text.Trim(),
                        Photo = null
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно!\nТеперь вы можете войти.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Возврат на страницу авторизации
                    NavigationService?.Navigate(new AuthPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации:\n" + ex.Message, "Ошибка");
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }
    }
}