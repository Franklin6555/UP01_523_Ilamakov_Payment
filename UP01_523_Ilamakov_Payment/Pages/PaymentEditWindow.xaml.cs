using System;
using System.Linq;
using System.Windows;

namespace UP01_523_Ilamakov_Payment
{
    public partial class PaymentEditWindow : Window
    {
        private Payment payment;          // null = добавление, иначе редактирование
        public bool IsSaved { get; private set; } = false;

        public PaymentEditWindow(Payment paymentToEdit = null)
        {
            InitializeComponent();
            payment = paymentToEdit;

            LoadUsers();
            LoadCategories();

            if (payment == null)
            {
                TitleText.Text = "Добавление платежа";
                DatePickerDate.SelectedDate = DateTime.Today;
                TextBoxNum.Text = "1";
            }
            else
            {
                TitleText.Text = "Редактирование платежа";
                ComboBoxUser.SelectedValue = payment.UserID;
                ComboBoxCategory.SelectedValue = payment.CategoryID;
                TextBoxName.Text = payment.Name;
                DatePickerDate.SelectedDate = payment.Date;
                TextBoxNum.Text = payment.Num.ToString();
                TextBoxPrice.Text = payment.Price.ToString();
            }
        }

        private void LoadUsers()
        {
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                ComboBoxUser.ItemsSource = db.Users.ToList();
            }
        }

        private void LoadCategories()
        {
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                ComboBoxCategory.ItemsSource = db.Categories.ToList();
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверки
            if (ComboBoxUser.SelectedValue == null ||
                ComboBoxCategory.SelectedValue == null ||
                string.IsNullOrWhiteSpace(TextBoxName.Text) ||
                DatePickerDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(TextBoxNum.Text) ||
                string.IsNullOrWhiteSpace(TextBoxPrice.Text))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TextBoxNum.Text, out decimal num) || num <= 0)
            {
                MessageBox.Show("Количество должно быть положительным числом", "Ошибка");
                return;
            }

            if (!decimal.TryParse(TextBoxPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть числом ≥ 0", "Ошибка");
                return;
            }

            try
            {
                using (var db = new DB_Ilamakov_523_PaymentEntities())
                {
                    if (payment == null) // Добавление
                    {
                        int newId = db.Payments.Any() ? db.Payments.Max(p => p.ID) + 1 : 1;

                        var newPayment = new Payment
                        {
                            ID = newId,
                            UserID = (int)ComboBoxUser.SelectedValue,
                            CategoryID = (int)ComboBoxCategory.SelectedValue,
                            Name = TextBoxName.Text.Trim(),
                            Date = DatePickerDate.SelectedDate.Value,
                            Num = num,
                            Price = price
                        };

                        db.Payments.Add(newPayment);
                    }
                    else // Редактирование
                    {
                        var existing = db.Payments.FirstOrDefault(p => p.ID == payment.ID);
                        if (existing != null)
                        {
                            existing.UserID = (int)ComboBoxUser.SelectedValue;
                            existing.CategoryID = (int)ComboBoxCategory.SelectedValue;
                            existing.Name = TextBoxName.Text.Trim();
                            existing.Date = DatePickerDate.SelectedDate.Value;
                            existing.Num = num;
                            existing.Price = price;
                        }
                    }

                    db.SaveChanges();
                    IsSaved = true;
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