using System;
using System.Windows;
using System.Windows.Controls;

namespace Pr17.Pages
{
    public partial class AppointmentDetailPage : Page
    {
        private int _appointmentId;

        public AppointmentDetailPage(int appointmentId)
        {
            InitializeComponent();
            _appointmentId = appointmentId;
            LoadData();
        }

        private void LoadData()
        {
            var appointment = Core.Context.Appointments.Find(_appointmentId);
            if (appointment == null) return;

            DateTimeText.Text = $"{appointment.Date.ToShortDateString()} {appointment.Time:hh\\:mm}";
            ServiceText.Text = $"Услуга: {appointment.ServiceTypes.Name}";
            ClientText.Text = $"Клиент: {appointment.Users.LastName} {appointment.Users.FirstName} {appointment.Users.MiddleName}";
            PhoneText.Text = $"Телефон: {appointment.Users.Phone}";
            CommentText.Text = $"Комментарий: {appointment.Comment ?? "Нет"}";

            CompleteButton.IsEnabled = appointment.Status != "Выполнена";
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            var appointment = Core.Context.Appointments.Find(_appointmentId);
            appointment.Status = "Выполнена";
            Core.Context.SaveChanges();
            MessageBox.Show("Запись отмечена выполненной");
            CompleteButton.IsEnabled = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}