using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP01_523_Ilamakov_Payment
{
    public partial class UserEditWindow : Window
    {
        private User user;

        public UserEditWindow(User userToEdit = null)
        {
            InitializeComponent();
            user = userToEdit;

            TitleText.Text = "Редактирование пользователя";
            TextBoxFIO.Text = user.FIO;
            TextBoxLogin.Text = user.Login;
            // Пароль не показываем — если оставить пустым, пароль не меняется
            PasswordBox.Password = "";

            if (user.Role == "Admin" || user.Role == "Администратор")
                ComboBoxRole.SelectedIndex = 1;
            else
                ComboBoxRole.SelectedIndex = 0;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxFIO.Text) ||
                string.IsNullOrWhiteSpace(TextBoxLogin.Text))
            {
                MessageBox.Show("Заполните ФИО и логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string role = (ComboBoxRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "User";

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    // Проверка уникальности логина
                    bool loginExists = db.Users.Any(u => u.Login == TextBoxLogin.Text.Trim() &&
                                                         (user == null || u.ID != user.ID));
                    if (loginExists)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка");
                        return;
                    }


                    var existing = db.Users.FirstOrDefault(u => u.ID == user.ID);
                    if (existing != null)
                    {
                        existing.FIO = TextBoxFIO.Text.Trim();
                        existing.Login = TextBoxLogin.Text.Trim();
                        existing.Role = role;

                        // Меняем пароль только если ввели новый
                        if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                            {
                                existing.Password = Pages.AuthPage.GetHash(PasswordBox.Password);
                            }
                    }

                    db.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения:\n" + ex.Message);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}