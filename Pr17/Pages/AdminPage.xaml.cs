using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pr17.Windows;

namespace Pr17.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            if (Core.CurrentUser?.Roles.Name != "Администратор")
            {
                MessageBox.Show("Доступ запрещён");
                NavigationService?.GoBack();
                return;
            }
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = Core.Context.Users.Select(u => new
            {
                u.Id,
                u.Login,
                FullName = u.LastName + " " + u.FirstName + " " + u.MiddleName,
                RoleName = u.Roles.Name,
                u.IsActive
            }).ToList();
            UsersGrid.ItemsSource = users;
        }

        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var userData = ((Button)sender).Tag;
            int userId = (int)userData.GetType().GetProperty("Id").GetValue(userData);
            var user = Core.Context.Users.Find(userId);
        }

        private void ToggleActive_Click(object sender, RoutedEventArgs e)
        {
            var userData = ((Button)sender).Tag;
            int userId = (int)userData.GetType().GetProperty("Id").GetValue(userData);
            var user = Core.Context.Users.Find(userId);
            user.IsActive = !user.IsActive;
            Core.Context.SaveChanges();
            LoadUsers();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var userData = ((Button)sender).Tag;
            int userId = (int)userData.GetType().GetProperty("Id").GetValue(userData);
            var user = Core.Context.Users.Find(userId);
            Core.Context.Users.Remove(user);
            Core.Context.SaveChanges();
            LoadUsers();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}