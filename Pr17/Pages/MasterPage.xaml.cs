using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pr17.Windows;

namespace Pr17.Pages
{
    public partial class MasterPage : Page
    {
        public MasterPage()
        {
            InitializeComponent();
            if (Core.CurrentUser?.Roles.Name != "Мастер")
            {
                MessageBox.Show("Доступ запрещён");
                NavigationService?.GoBack();
                return;
            }
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var appointments = Core.Context.Appointments
                .Where(a => a.MasterId == Core.CurrentUser.Id)
                .Select(a => new
                {
                    a.Id,
                    a.Date,
                    a.Time,
                    ServiceTypeName = a.ServiceTypes.Name,
                    ClientName = a.Users.LastName + " " + a.Users.FirstName,
                    a.Status
                }).ToList();
            AppointmentsGrid.ItemsSource = appointments;
        }

        private void EditServicesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new MasterServicesWindow(Core.CurrentUser.Id);
            window.ShowDialog();
        }

        private void AppointmentsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AppointmentsGrid.SelectedItem != null)
            {
                int id = (int)AppointmentsGrid.SelectedItem.GetType().GetProperty("Id").GetValue(AppointmentsGrid.SelectedItem);
                NavigationService?.Navigate(new AppointmentDetailPage(id));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}