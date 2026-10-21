using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Ilamakov_Payment.Pages;

namespace UP01_523_Ilamakov_Payment.Pages
{
    public partial class AdminPage : Page
    {
        private User currentUser;

        public AdminPage(User user)
        {
            InitializeComponent();
            currentUser = user;
            LoadCategories();
            LoadPayments();
        }

        private void LoadCategories()
        {
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                var categories = db.Categories.ToList();
                categories.Insert(0, new Category { ID = 0, Name = "Все категории" });
                ComboBoxCategory.ItemsSource = categories;
                ComboBoxCategory.SelectedIndex = 0;
            }
        }

        private void LoadPayments()
        {
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                var query = db.Payments
                    .Include("Category")
                    .Include("User")
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(TextBoxSearch.Text))
                {
                    string search = TextBoxSearch.Text.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(search));
                }

                if (ComboBoxCategory.SelectedValue != null && (int)ComboBoxCategory.SelectedValue != 0)
                {
                    int catId = (int)ComboBoxCategory.SelectedValue;
                    query = query.Where(p => p.CategoryID == catId);
                }

                var list = query
                    .OrderByDescending(p => p.Date)
                    .ToList()
                    .Select(p => new
                    {
                        p.ID,
                        p.Date,
                        User = p.User,
                        p.Name,
                        Category = p.Category,
                        p.Num,
                        p.Price,
                        Total = p.Num * p.Price,
                        Payment = p          // сохраняем исходный объект для редактирования
                    })
                    .ToList();

                DataGridPayments.ItemsSource = list;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadPayments();
        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) LoadPayments();
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = "";
            ComboBoxCategory.SelectedIndex = 0;
            LoadPayments();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new PaymentEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadPayments();
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DataGridPayments.SelectedItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание");
                return;
            }

            Payment paymentToEdit = null;
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                int id = selected.ID;
                paymentToEdit = db.Payments.FirstOrDefault(p => p.ID == id);
            }

            if (paymentToEdit == null) return;

            var window = new PaymentEditWindow(paymentToEdit);
            if (window.ShowDialog() == true)
            {
                LoadPayments();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DataGridPayments.SelectedItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание");
                return;
            }

            var result = MessageBox.Show("Вы действительно хотите удалить эту запись?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    int id = selected.ID;
                    var payment = db.Payments.FirstOrDefault(p => p.ID == id);
                    if (payment != null)
                    {
                        db.Payments.Remove(payment);
                        db.SaveChanges();
                        MessageBox.Show("Запись удалена", "Успех");
                        LoadPayments();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления:\n" + ex.Message);
            }
        }

        private void DataGridPayments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ButtonEdit_Click(sender, e);
        }

        private void ButtonChart_Click(object sender, RoutedEventArgs e)
        {
            var window = new ChartWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }

        private void ButtonExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Документ Word (*.doc)|*.doc",
                    FileName = $"Отчёт_платежи_{DateTime.Now:yyyy-MM-dd}.doc"
                };

                if (dialog.ShowDialog() != true) return;

                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var data = db.Payments
                        .Include("Category")
                        .Include("User")
                        .OrderByDescending(p => p.Date)
                        .ToList();

                    var sb = new System.Text.StringBuilder();

                    sb.AppendLine("<html>");
                    sb.AppendLine("<head><meta charset='utf-8'></head>");
                    sb.AppendLine("<body>");
                    sb.AppendLine("<h2 style='text-align:center;'>Отчёт по платежам</h2>");
                    sb.AppendLine($"<p>Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
                    sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse; width:100%;'>");
                    sb.AppendLine("<tr style='background-color:#0198AF; color:white;'>");
                    sb.AppendLine("<th>ID</th><th>Дата</th><th>Пользователь</th><th>Название</th><th>Категория</th><th>Кол-во</th><th>Цена</th><th>Сумма</th>");
                    sb.AppendLine("</tr>");

                    foreach (var p in data)
                    {
                        sb.AppendLine("<tr>");
                        sb.AppendLine($"<td>{p.ID}</td>");
                        sb.AppendLine($"<td>{p.Date:dd.MM.yyyy}</td>");
                        sb.AppendLine($"<td>{p.User?.FIO}</td>");
                        sb.AppendLine($"<td>{p.Name}</td>");
                        sb.AppendLine($"<td>{p.Category?.Name}</td>");
                        sb.AppendLine($"<td>{p.Num}</td>");
                        sb.AppendLine($"<td>{p.Price}</td>");
                        sb.AppendLine($"<td>{p.Num * p.Price}</td>");
                        sb.AppendLine("</tr>");
                    }

                    sb.AppendLine("</table>");
                    sb.AppendLine("</body></html>");

                    System.IO.File.WriteAllText(dialog.FileName, sb.ToString(),
                        new System.Text.UTF8Encoding(true));
                }

                MessageBox.Show("Отчёт успешно экспортирован в Word!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message, "Ошибка");
            }
        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel файлы (*.csv)|*.csv",
                    FileName = $"Платежи_{DateTime.Now:yyyy-MM-dd}.csv"
                };

                if (dialog.ShowDialog() != true) return;

                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var data = db.Payments
                        .Include("Category")
                        .Include("User")
                        .OrderByDescending(p => p.Date)
                        .ToList();

                    var sb = new System.Text.StringBuilder();
                    // Заголовки
                    sb.AppendLine("ID;Дата;Пользователь;Название;Категория;Количество;Цена;Сумма");

                    foreach (var p in data)
                    {
                        string line = $"{p.ID};" +
                                      $"{p.Date:dd.MM.yyyy};" +
                                      $"\"{p.User?.FIO}\";" +
                                      $"\"{p.Name}\";" +
                                      $"\"{p.Category?.Name}\";" +
                                      $"{p.Num};" +
                                      $"{p.Price};" +
                                      $"{p.Num * p.Price}";
                        sb.AppendLine(line);
                    }

                    // UTF-8 с BOM, чтобы Excel правильно открыл кириллицу
                    System.IO.File.WriteAllText(dialog.FileName, sb.ToString(),
                        new System.Text.UTF8Encoding(true));
                }

                MessageBox.Show("Данные успешно экспортированы в Excel!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message, "Ошибка");
            }
        }

        private void ButtonChangePass_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage(currentUser));
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                NavigationService?.Navigate(new AuthPage());
        }
    }
}