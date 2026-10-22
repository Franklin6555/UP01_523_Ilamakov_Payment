using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Ilamakov_Payment.Pages;

namespace UP01_523_Ilamakov_Payment.Pages
{
    public partial class UserPage : Page
    {
        private User currentUser;

        public UserPage(User user)
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
                    .Where(p => p.UserID == currentUser.ID)
                    .AsQueryable();

                // Поиск
                if (!string.IsNullOrWhiteSpace(TextBoxSearch.Text))
                {
                    string search = TextBoxSearch.Text.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(search));
                }

                // Фильтр по категории
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
                        p.Date,
                        p.Name,
                        p.Category,
                        p.Num,
                        p.Price,
                        Total = p.Num * p.Price
                    })
                    .ToList();

                DataGridPayments.ItemsSource = list;
            }
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadPayments();
        }

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

        private void ButtonChangePass_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage(currentUser));
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService?.Navigate(new AuthPage());
            }
        }
    }
}