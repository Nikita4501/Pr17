using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pr17.Pages
{
    public partial class AppointmentConfirmationPage : Page
    {
        private int _serviceTypeId;
        private int _masterId;
        private DateTime _date;
        private TimeSpan _time;

        public AppointmentConfirmationPage(int serviceTypeId, int masterId, DateTime date, TimeSpan time)
        {
            InitializeComponent();
            _serviceTypeId = serviceTypeId;
            _masterId = masterId;
            _date = date;
            _time = time;

            var service = Core.Context.ServiceTypes.Find(serviceTypeId);
            var master = Core.Context.Users.Find(masterId);

            ServiceText.Text = $"Услуга: {service.Name}";
            MasterText.Text = $"Мастер: {master.LastName} {master.FirstName}";
            DateTimeText.Text = $"Дата и время: {date.ToShortDateString()} {time:hh\\:mm}";

            PaymentCombo.ItemsSource = new[] { "Наличные", "Карта", "Онлайн" };
            PaymentCombo.SelectedIndex = 0;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо войти в систему");
                return;
            }

            var result = MessageBox.Show("Подтвердить запись?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var appointment = new Appointments
                {
                    ClientId = Core.CurrentUser.Id,
                    MasterId = _masterId,
                    ServiceTypeId = _serviceTypeId,
                    Date = _date,
                    Time = _time,
                    Comment = CommentBox.Text,
                    PaymentMethod = PaymentCombo.SelectedItem.ToString(),
                    Status = "Запланирована"
                };

                Core.Context.Appointments.Add(appointment);
                Core.Context.SaveChanges();

                MessageBox.Show("Вы успешно записаны!");
                NavigationService?.Navigate(new ClientAccountPage());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}