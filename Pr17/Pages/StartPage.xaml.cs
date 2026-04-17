using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pr17.Windows;

namespace Pr17.Pages
{
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
            LoadData();
            UpdateLoginUI();
        }

        private void LoadData()
        {
            var serviceTypes = Core.Context.ServiceTypes.ToList();
            ServiceTypesListBox.ItemsSource = serviceTypes;
            ServiceTypeFilter.ItemsSource = serviceTypes;
            ServiceTypeFilter.DisplayMemberPath = "Name";

            var masters = Core.Context.Users
                .Where(u => u.Roles.Name == "Мастер")
                .Select(u => new { u.Id, FullName = u.LastName + " " + u.FirstName, u.Specialization })
                .ToList();
            MasterFilter.ItemsSource = masters;
            MasterFilter.DisplayMemberPath = "FullName";
        }

        private void UpdateLoginUI()
        {
            if (Core.CurrentUser != null)
            {
                LoginButton.Visibility = Visibility.Collapsed;
                RegisterButton.Visibility = Visibility.Collapsed;
                AccountButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Visible;
            }
            else
            {
                LoginButton.Visibility = Visibility.Visible;
                RegisterButton.Visibility = Visibility.Visible;
                AccountButton.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyFilters(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = ServiceTypeFilter.SelectedItem as ServiceTypes;
            var selectedMaster = MasterFilter.SelectedItem;

            int? masterId = selectedMaster != null
                ? (int?)selectedMaster.GetType().GetProperty("Id").GetValue(selectedMaster)
                : null;

            var mastersQuery = Core.Context.MasterServices.AsQueryable();
            if (selectedType != null)
                mastersQuery = mastersQuery.Where(ms => ms.ServiceTypeId == selectedType.Id);
            if (masterId.HasValue)
                mastersQuery = mastersQuery.Where(ms => ms.MasterId == masterId.Value);

            var filteredMasters = mastersQuery
                .Select(ms => ms.Users)
                .Where(u => u.Roles.Name == "Мастер")
                .Select(u => new { u.Id, FullName = u.LastName + " " + u.FirstName, u.Specialization })
                .Distinct()
                .ToList();

            MastersListBox.ItemsSource = filteredMasters;
        }

        private void ServiceTypesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = ServiceTypesListBox.SelectedItem as ServiceTypes;
            if (selectedType != null)
            {
                var masters = Core.Context.MasterServices
                    .Where(ms => ms.ServiceTypeId == selectedType.Id)
                    .Select(ms => ms.Users)
                    .Where(u => u.Roles.Name == "Мастер")
                    .Select(u => new { u.Id, FullName = u.LastName + " " + u.FirstName, u.Specialization })
                    .ToList();
                MastersListBox.ItemsSource = masters;
            }
        }

        private void MastersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMaster = MastersListBox.SelectedItem;
            var selectedType = ServiceTypesListBox.SelectedItem as ServiceTypes;
            if (selectedMaster != null && selectedType != null)
            {
                int masterId = (int)selectedMaster.GetType().GetProperty("Id").GetValue(selectedMaster);
                NavigationService?.Navigate(new AppointmentSelectionPage(selectedType.Id, masterId));
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                UpdateLoginUI();
                RedirectByRole();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
            UpdateLoginUI();
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            RedirectByRole();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Core.CurrentUser = null;
            UpdateLoginUI();
            NavigationService?.Navigate(new StartPage());
        }

        private void RedirectByRole()
        {
            if (Core.CurrentUser == null) return;
            string roleName = Core.CurrentUser.Roles?.Name;
            switch (roleName)
            {
                case "Клиент":
                    NavigationService?.Navigate(new ClientAccountPage());
                    break;
                case "Мастер":
                    NavigationService?.Navigate(new MasterPage());
                    break;
                case "Менеджер":
                    NavigationService?.Navigate(new ManagerPage());
                    break;
                case "Администратор":
                    NavigationService?.Navigate(new AdminPage());
                    break;
            }
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProductsPage());
        }
    }
}