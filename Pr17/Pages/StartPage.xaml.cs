using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pr17.Windows;

namespace Pr17.Pages
{
    public partial class StartPage : Page
    {
        public class MasterInfo
        {
            public int Id { get; set; }
            public string FullName { get; set; }
        }

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

            var allServiceTypes = new List<ServiceTypes> { new ServiceTypes { Id = 0, Name = "Все" } };
            allServiceTypes.AddRange(serviceTypes);
            ServiceTypeFilter.ItemsSource = allServiceTypes;
            ServiceTypeFilter.DisplayMemberPath = "Name";
            ServiceTypeFilter.SelectedIndex = 0;

            var masters = Core.Context.Users
                .Where(u => u.Roles.Name == "Мастер")
                .Select(u => new MasterInfo
                {
                    Id = u.Id,
                    FullName = u.LastName + " " + u.FirstName
                })
                .ToList();

            var allMasters = new List<MasterInfo> { new MasterInfo { Id = 0, FullName = "Все" } };
            allMasters.AddRange(masters);
            MasterFilter.ItemsSource = allMasters;
            MasterFilter.DisplayMemberPath = "FullName";
            MasterFilter.SelectedIndex = 0;

            MastersListBox.ItemsSource = null;
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
            var selectedMaster = MasterFilter.SelectedItem as MasterInfo;

            int? typeId = (selectedType != null && selectedType.Id != 0) ? selectedType.Id : (int?)null;
            int? masterId = (selectedMaster != null && selectedMaster.Id != 0) ? selectedMaster.Id : (int?)null;

            var query = Core.Context.MasterServices.AsQueryable();
            if (typeId.HasValue)
                query = query.Where(ms => ms.ServiceTypeId == typeId.Value);
            if (masterId.HasValue)
                query = query.Where(ms => ms.MasterId == masterId.Value);

            var filteredMasters = query
                .Select(ms => ms.Users)
                .Where(u => u.Roles.Name == "Мастер" && u.IsActive)
                .Select(u => new MasterInfo
                {
                    Id = u.Id,
                    FullName = u.LastName + " " + u.FirstName
                })
                .Distinct()
                .ToList();

            MastersListBox.ItemsSource = filteredMasters;
        }

        private void ServiceTypesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = ServiceTypesListBox.SelectedItem as ServiceTypes;
            if (selectedType != null)
            {
                foreach (ServiceTypes item in ServiceTypeFilter.Items)
                {
                    if (item.Id == selectedType.Id)
                    {
                        ServiceTypeFilter.SelectedItem = item;
                        break;
                    }
                }
                LoadMastersForServiceType(selectedType.Id);
            }
            else
            {
                MastersListBox.ItemsSource = null;
            }
        }

        private void LoadMastersForServiceType(int serviceTypeId)
        {
            var selectedMasterFilter = MasterFilter.SelectedItem as MasterInfo;
            int? masterId = (selectedMasterFilter != null && selectedMasterFilter.Id != 0) ? selectedMasterFilter.Id : (int?)null;

            var query = Core.Context.MasterServices
                .Where(ms => ms.ServiceTypeId == serviceTypeId);

            if (masterId.HasValue)
                query = query.Where(ms => ms.MasterId == masterId.Value);

            var masters = query
                .Select(ms => ms.Users)
                .Where(u => u.Roles.Name == "Мастер" && u.IsActive)
                .Select(u => new MasterInfo
                {
                    Id = u.Id,
                    FullName = u.LastName + " " + u.FirstName
                })
                .Distinct()
                .ToList();

            MastersListBox.ItemsSource = masters;
        }

        private void MastersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMaster = MastersListBox.SelectedItem as MasterInfo;
            var selectedType = ServiceTypesListBox.SelectedItem as ServiceTypes;
            if (selectedMaster != null && selectedType != null)
            {
                NavigationService?.Navigate(new AppointmentSelectionPage(selectedType.Id, selectedMaster.Id));
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