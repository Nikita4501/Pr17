using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pr17.Pages
{
    public partial class AppointmentSelectionPage : Page
    {
        private int _serviceTypeId;
        private int _masterId;

        public AppointmentSelectionPage(int serviceTypeId, int masterId)
        {
            InitializeComponent();
            _serviceTypeId = serviceTypeId;
            _masterId = masterId;
            DatePicker.SelectedDate = DateTime.Today;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSlots(DatePicker.SelectedDate ?? DateTime.Today);
        }

        private void LoadSlots(DateTime date)
        {
            // Генерируем временные слоты с 9:00 до 18:00 с шагом 1 час
            var slots = Enumerable.Range(9, 9).Select(hour => new
            {
                Date = date,
                Time = new TimeSpan(hour, 0, 0),
                IsFree = !Core.Context.Appointments.Any(a =>
                    a.MasterId == _masterId &&
                    a.Date == date &&
                    a.Time == new TimeSpan(hour, 0, 0) &&
                    a.Status != "Отменена")
            }).ToList();

            SlotsGrid.ItemsSource = slots;
        }

        private void SlotsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SlotsGrid.SelectedItem != null)
            {
                dynamic selected = SlotsGrid.SelectedItem;
                if (selected.IsFree)
                {
                    NavigationService?.Navigate(new AppointmentConfirmationPage(_serviceTypeId, _masterId, selected.Date, selected.Time));
                }
                else
                {
                    MessageBox.Show("Это время уже занято");
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}