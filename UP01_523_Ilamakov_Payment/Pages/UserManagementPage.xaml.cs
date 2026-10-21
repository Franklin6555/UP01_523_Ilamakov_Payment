using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP01_523_Ilamakov_Payment.Pages
{
    public partial class UserManagementPage : Page
    {
        public UserManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        // Загрузка пользователей из базы данных

        private void LoadUsers()
        {
            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var users = db.Users
                        .OrderBy(u => u.ID)
                        .ToList()
                        .Select(u => new User
                        {
                            ID = u.ID,
                            Login = u.Login,
                            FIO = u.FIO,
                            Role = u.Role
                        })
                        .ToList();

                    DataGridUsers.ItemsSource = users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить пользователей:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // Сохранение изменений
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            DataGridUsers.CommitEdit(
                DataGridEditingUnit.Cell, true);

            DataGridUsers.CommitEdit(
                DataGridEditingUnit.Row, true);

            var users = DataGridUsers.ItemsSource
                .Cast<User>()
                .ToList();

            if (users.Any(u =>
                string.IsNullOrWhiteSpace(u.Login) ||
                string.IsNullOrWhiteSpace(u.FIO) ||
                string.IsNullOrWhiteSpace(u.Role)))
            {
                MessageBox.Show(
                    "Заполните логин, ФИО и роль каждого пользователя.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (users.Any(u =>
                u.Role.Trim() != "User" &&
                u.Role.Trim() != "Admin"))
            {
                MessageBox.Show(
                    "Допустимые роли: User и Admin.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (users.GroupBy(
                    u => u.Login.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1))
            {
                MessageBox.Show(
                    "У пользователей не должно быть одинаковых логинов.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    foreach (var editedUser in users)
                    {
                        string login = editedUser.Login.Trim();

                        bool loginExists = db.Users.Any(u =>
                            u.ID != editedUser.ID &&
                            u.Login == login);

                        if (loginExists)
                        {
                            MessageBox.Show(
                                $"Логин «{login}» уже используется.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        var user = db.Users.FirstOrDefault(
                            u => u.ID == editedUser.ID);

                        if (user == null)
                        {
                            MessageBox.Show(
                                $"Пользователь с ID {editedUser.ID} не найден.");
                            return;
                        }

                        user.Login = login;
                        user.FIO = editedUser.FIO.Trim();
                        user.Role = editedUser.Role.Trim();
                    }

                    db.SaveChanges();
                }

                MessageBox.Show(
                    "Изменения успешно сохранены.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при сохранении:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Удаление выбранного пользователя
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGridUsers.CommitEdit(
                DataGridEditingUnit.Cell, true);

            DataGridUsers.CommitEdit(
                DataGridEditingUnit.Row, true);

            var selectedUser = DataGridUsers.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show(
                    "Сначала выберите пользователя в таблице.",
                    "Пользователь не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var answer = MessageBox.Show(
                $"Вы действительно хотите удалить пользователя «{selectedUser.Login}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var user = db.Users.FirstOrDefault(
                        u => u.ID == selectedUser.ID);

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь уже удалён.");
                        LoadUsers();
                        return;
                    }

                    // Проверяем, есть ли у пользователя платежи
                    bool hasPayments = db.Payments.Any(
                        p => p.User.ID == user.ID);

                    if (hasPayments)
                    {
                        MessageBox.Show(
                            "Нельзя удалить этого пользователя, поскольку с ним связаны платежи.",
                            "Удаление невозможно",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    db.Users.Remove(user);
                    db.SaveChanges();
                }

                MessageBox.Show(
                    "Пользователь успешно удалён.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при удалении:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}