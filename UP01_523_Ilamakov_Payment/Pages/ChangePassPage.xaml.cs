using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Ilamakov_Payment.Pages;

namespace UP01_523_Ilamakov_Payment.Pages
{
    public partial class ChangePassPage : Page
    {
        private User currentUser;

        public ChangePassPage(User user)
        {
            InitializeComponent();
            currentUser = user;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBoxOld.Password) ||
                string.IsNullOrWhiteSpace(PasswordBoxNew.Password) ||
                string.IsNullOrWhiteSpace(PasswordBoxConfirm.Password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBoxNew.Password != PasswordBoxConfirm.Password)
            {
                MessageBox.Show("Новые пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBoxNew.Password.Length < 4)
            {
                MessageBox.Show("Новый пароль должен содержать минимум 4 символа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string oldHash = AuthPage.GetHash(PasswordBoxOld.Password);

            if (oldHash != currentUser.Password)
            {
                MessageBox.Show("Текущий пароль введён неверно", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var user = db.Users.FirstOrDefault(u => u.ID == currentUser.ID);
                    if (user != null)
                    {
                        user.Password = AuthPage.GetHash(PasswordBoxNew.Password);
                        db.SaveChanges();

                        MessageBox.Show("Пароль успешно изменён", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        NavigationService?.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при смене пароля:\n" + ex.Message, "Ошибка");
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}