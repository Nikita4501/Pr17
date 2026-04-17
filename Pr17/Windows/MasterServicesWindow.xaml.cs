using System.Linq;
using System.Windows;

namespace Pr17.Windows
{
    public partial class MasterServicesWindow : Window
    {
        private int _masterId;

        public MasterServicesWindow(int masterId)
        {
            InitializeComponent();
            _masterId = masterId;
            LoadData();
        }

        private void LoadData()
        {
            var allServices = Core.Context.ServiceTypes.ToList();
            var masterServices = Core.Context.MasterServices
                .Where(ms => ms.MasterId == _masterId)
                .Select(ms => ms.ServiceTypeId)
                .ToList();

            ServicesListBox.ItemsSource = allServices;
            foreach (var service in allServices)
            {
                if (masterServices.Contains(service.Id))
                    ServicesListBox.SelectedItems.Add(service);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var existing = Core.Context.MasterServices.Where(ms => ms.MasterId == _masterId);
            Core.Context.MasterServices.RemoveRange(existing);

            foreach (ServiceTypes service in ServicesListBox.SelectedItems)
            {
                Core.Context.MasterServices.Add(new MasterServices
                {
                    MasterId = _masterId,
                    ServiceTypeId = service.Id
                });
            }

            Core.Context.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}