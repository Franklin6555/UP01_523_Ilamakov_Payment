using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Ilamakov_Payment.Pages;
using ClosedXML.Excel;

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
                    Filter = "Excel файлы (*.xlsx)|*.xlsx",
                    FileName = $"Платежи_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };

                if (dialog.ShowDialog() != true) return;

                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var data = db.Payments
                        .Include("Category")
                        .Include("User")
                        .OrderByDescending(p => p.Date)
                        .ToList();

                    using (var workbook = new ClosedXML.Excel.XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Платежи");

                        // Заголовки
                        worksheet.Cell(1, 1).Value = "ID";
                        worksheet.Cell(1, 2).Value = "Дата";
                        worksheet.Cell(1, 3).Value = "Пользователь";
                        worksheet.Cell(1, 4).Value = "Название";
                        worksheet.Cell(1, 5).Value = "Категория";
                        worksheet.Cell(1, 6).Value = "Количество";
                        worksheet.Cell(1, 7).Value = "Цена";
                        worksheet.Cell(1, 8).Value = "Сумма";

                        // Стиль заголовков
                        var headerRange = worksheet.Range(1, 1, 1, 8);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0198AF");
                        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                        // Данные
                        int row = 2;
                        foreach (var p in data)
                        {
                            worksheet.Cell(row, 1).Value = p.ID;
                            worksheet.Cell(row, 2).Value = p.Date;
                            worksheet.Cell(row, 2).Style.DateFormat.Format = "dd.MM.yyyy";
                            worksheet.Cell(row, 3).Value = p.User?.FIO;
                            worksheet.Cell(row, 4).Value = p.Name;
                            worksheet.Cell(row, 5).Value = p.Category?.Name;
                            worksheet.Cell(row, 6).Value = p.Num;
                            worksheet.Cell(row, 7).Value = p.Price;
                            worksheet.Cell(row, 8).Value = p.Num * p.Price;
                            row++;
                        }

                        // Автоширина столбцов
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(dialog.FileName);
                    }
                }

                MessageBox.Show("Данные успешно экспортированы в Excel (.xlsx)!", "Успех",
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