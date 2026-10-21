using System;
using System.Linq;
using System.Windows;

namespace UP01_523_Ilamakov_Payment
{
    public partial class CategoryEditWindow : Window
    {
        private Category category;

        public CategoryEditWindow(Category categoryToEdit = null)
        {
            InitializeComponent();
            category = categoryToEdit;

            if (category == null)
            {
                TitleText.Text = "Добавление категории";
            }
            else
            {
                TitleText.Text = "Редактирование категории";
                TextBoxName.Text = category.Name;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxName.Text))
            {
                MessageBox.Show("Введите название категории", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    if (category == null) // Добавление
                    {
                        int newId = db.Categories.Any() ? db.Categories.Max(c => c.ID) + 1 : 1;

                        var newCat = new Category
                        {
                            ID = newId,
                            Name = TextBoxName.Text.Trim()
                        };

                        db.Categories.Add(newCat);
                    }
                    else // Редактирование
                    {
                        var existing = db.Categories.FirstOrDefault(c => c.ID == category.ID);
                        if (existing != null)
                        {
                            existing.Name = TextBoxName.Text.Trim();
                        }
                    }

                    db.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения:\n" + ex.Message, "Ошибка");
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}