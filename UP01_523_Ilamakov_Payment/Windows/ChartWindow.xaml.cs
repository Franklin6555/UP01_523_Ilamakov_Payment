using System.Linq;
using System.Windows;
using LiveCharts;

namespace UP01_523_Ilamakov_Payment
{
    public partial class ChartWindow : Window
    {
        public ChartValues<decimal> Values { get; set; }
        public string[] Labels { get; set; }

        public ChartWindow()
        {
            InitializeComponent();
            LoadData();
            DataContext = this;
        }

        private void LoadData()
        {
            using (var db = new DB_Ilamakov_523_PaymentEntities())
            {
                var data = db.Payments
                    .GroupBy(p => p.Category.Name)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(p => p.Num * p.Price)
                    })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                Labels = data.Select(x => x.Category).ToArray();
                Values = new ChartValues<decimal>(data.Select(x => x.Total));
            }
        }
    }
}