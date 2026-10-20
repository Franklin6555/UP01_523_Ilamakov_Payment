using System.Windows;
using System.Windows.Controls;

namespace UP01_Ilamakov_Payment.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Логика входа будет добавлена после подключения Entity Framework", "Информация");
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница регистрации будет создана далее");
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Страница смены пароля будет создана далее");
        }
    }
}