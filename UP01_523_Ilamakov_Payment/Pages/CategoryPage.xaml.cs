using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UP01_523_Ilamakov_Payment.Pages
{
    public partial class CategoryPage : Page
    {
        public CategoryPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                DataGridCategories.ItemsSource = db.Categories
                    .OrderBy(c => c.ID)
                    .ToList();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new CategoryEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadCategories();
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridCategories.SelectedItem as Category;
            if (selected == null)
            {
                MessageBox.Show("Выберите категорию для редактирования", "Внимание");
                return;
            }

            var window = new CategoryEditWindow(selected);
            if (window.ShowDialog() == true)
            {
                LoadCategories();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DataGridCategories.SelectedItem as Category;
            if (selected == null)
            {
                MessageBox.Show("Выберите категорию для удаления", "Внимание");
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить категорию «{selected.Name}»?\n\n" +
                "Все платежи этой категории также будут удалены (каскадное удаление).",
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    var cat = db.Categories.FirstOrDefault(c => c.ID == selected.ID);
                    if (cat != null)
                    {
                        db.Categories.Remove(cat);   // платежи удалятся автоматически благодаря CASCADE
                        db.SaveChanges();
                        MessageBox.Show("Категория и связанные платежи удалены", "Успех");
                        LoadCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления:\n" + ex.Message);
            }
        }

        private void DataGridCategories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ButtonEdit_Click(sender, e);
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}