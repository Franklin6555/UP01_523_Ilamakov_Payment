using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP01_523_Ilamakov_Payment.Pages
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                DataGridUsers.ItemsSource = db.Users
                    .OrderBy(u => u.ID)
                    .ToList();
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridUsers.SelectedItem as User;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя", "Внимание");
                return;
            }

            var window = new UserEditWindow(selected);
            if (window.ShowDialog() == true)
                LoadUsers();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridUsers.SelectedItem as User;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя", "Внимание");
                return;
            }

            // Нельзя удалить самого себя (текущего админа) — по желанию
            var result = MessageBox.Show(
                $"Удалить пользователя «{selected.FIO ?? selected.Login}»?\n\n" +
                "Все его платежи также будут удалены (каскадное удаление).",
                "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var user = db.Users.FirstOrDefault(u => u.ID == selected.ID);
                    if (user != null)
                    {
                        db.Users.Remove(user); // платежи удалятся каскадно
                        db.SaveChanges();
                        MessageBox.Show("Пользователь и его платежи удалены", "Успех");
                        LoadUsers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления:\n" + ex.Message);
            }
        }

        private void DataGridUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ButtonEdit_Click(sender, e);
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}