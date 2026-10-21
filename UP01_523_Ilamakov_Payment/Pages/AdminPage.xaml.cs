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
            MessageBox.Show("Позже");
        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Позже");
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