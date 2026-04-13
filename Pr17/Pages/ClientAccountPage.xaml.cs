using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pr17.Pages
{
    public partial class ClientAccountPage : Page
    {
        public ClientAccountPage()
        {
            InitializeComponent();
            if (Core.CurrentUser == null || Core.CurrentUser.Roles.Name != "Клиент")
            {
                MessageBox.Show("Доступ запрещён");
                NavigationService?.GoBack();
                return;
            }
            LoadData();
        }

        private void LoadData()
        {
            int userId = Core.CurrentUser.Id;

            var appointments = Core.Context.Appointments
                .Where(a => a.ClientId == userId)
                .Select(a => new
                {
                    a.Date,
                    a.Time,
                    ServiceTypeName = a.ServiceTypes.Name,
                    MasterName = a.Users1.LastName + " " + a.Users1.FirstName,
                    a.Status
                }).ToList();
            AppointmentsGrid.ItemsSource = appointments;

            var orders = Core.Context.Orders
                .Where(o => o.ClientId == userId)
                .Select(o => new
                {
                    o.OrderDate,
                    o.DeliveryDate,
                    TotalAmount = o.OrderItems.Sum(oi => oi.Price * oi.Quantity),
                    o.Status
                }).ToList();
            OrdersGrid.ItemsSource = orders;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}