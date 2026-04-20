using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pr17.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            if (Core.CurrentUser?.Roles?.Name != "Администратор")
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
            if (userData == null) return;
            int userId = (int)userData.GetType().GetProperty("Id").GetValue(userData);
            var user = Core.Context.Users.Find(userId);
            if (user == null) return;

            var roles = Core.Context.Roles.ToList();
            var win = new Window
            {
                Title = "Выберите новую роль",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };
            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var combo = new ComboBox { ItemsSource = roles, DisplayMemberPath = "Name", Margin = new Thickness(5) };
            combo.SelectedItem = roles.FirstOrDefault(r => r.Id == user.RoleId);
            Grid.SetRow(combo, 0);
            grid.Children.Add(combo);

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(btnPanel, 1);
            var okBtn = new Button { Content = "OK", Width = 80, Margin = new Thickness(5) };
            var cancelBtn = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };
            okBtn.Click += (s, ev) => { win.DialogResult = true; win.Close(); };
            cancelBtn.Click += (s, ev) => { win.DialogResult = false; win.Close(); };
            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            grid.Children.Add(btnPanel);

            win.Content = grid;
            if (win.ShowDialog() == true && combo.SelectedItem != null)
            {
                var selectedRole = (Roles)combo.SelectedItem;
                user.RoleId = selectedRole.Id;
                Core.Context.SaveChanges();
                LoadUsers();
                MessageBox.Show("Роль изменена");
            }
        }

        private void ToggleActive_Click(object sender, RoutedEventArgs e)
        {
            var userData = ((Button)sender).Tag;
            if (userData == null) return;
            int userId = (int)userData.GetType().GetProperty("Id").GetValue(userData);
            var user = Core.Context.Users.Find(userId);
            if (user == null) return;

            if (user.Id == Core.CurrentUser.Id)
            {
                MessageBox.Show("Нельзя заморозить самого себя");
                return;
            }

            user.IsActive = !user.IsActive;
            Core.Context.SaveChanges();
            LoadUsers();
            MessageBox.Show(user.IsActive ? "Пользователь разморожен" : "Пользователь заморожен");
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var userData = ((Button)sender).Tag;
            if (userData == null) return;
            int userId = (int)userData.GetType().GetProperty("Id").GetValue(userData);

            if (userId == Core.CurrentUser.Id)
            {
                MessageBox.Show("Нельзя удалить самого себя");
                return;
            }

            var user = Core.Context.Users.Find(userId);
            if (user == null) return;

            var result = MessageBox.Show($"Удалить пользователя {user.Login} и все связанные данные?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var masterServices = Core.Context.MasterServices.Where(ms => ms.MasterId == userId);
                Core.Context.MasterServices.RemoveRange(masterServices);

                var appointmentsAsMaster = Core.Context.Appointments.Where(a => a.MasterId == userId);
                Core.Context.Appointments.RemoveRange(appointmentsAsMaster);

                var appointmentsAsClient = Core.Context.Appointments.Where(a => a.ClientId == userId);
                Core.Context.Appointments.RemoveRange(appointmentsAsClient);

                var orders = Core.Context.Orders.Where(o => o.ClientId == userId).ToList();
                foreach (var order in orders)
                {
                    var orderItems = Core.Context.OrderItems.Where(oi => oi.OrderId == order.Id);
                    Core.Context.OrderItems.RemoveRange(orderItems);
                }
                Core.Context.Orders.RemoveRange(orders);

                var cartItems = Core.Context.Cart.Where(c => c.UserId == userId);
                Core.Context.Cart.RemoveRange(cartItems);

                Core.Context.Users.Remove(user);

                Core.Context.SaveChanges();
                LoadUsers();
                MessageBox.Show("Пользователь и все связанные данные удалены");
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new Window
            {
                Title = "Добавить пользователя",
                Width = 350,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid { Margin = new Thickness(10) };
            for (int i = 0; i < 16; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            AddLabelAndTextBox(grid, "Логин:", ref row, out TextBox loginBox);
            AddLabelAndPasswordBox(grid, "Пароль:", ref row, out PasswordBox passwordBox);
            AddLabelAndTextBox(grid, "Фамилия:", ref row, out TextBox lastNameBox);
            AddLabelAndTextBox(grid, "Имя:", ref row, out TextBox firstNameBox);
            AddLabelAndTextBox(grid, "Отчество:", ref row, out TextBox middleNameBox);
            AddLabelAndTextBox(grid, "Телефон:", ref row, out TextBox phoneBox);

            var roleLabel = new TextBlock { Text = "Роль:", Margin = new Thickness(0, 10, 0, 0) };
            Grid.SetRow(roleLabel, row++);
            grid.Children.Add(roleLabel);
            var roleCombo = new ComboBox
            {
                ItemsSource = Core.Context.Roles.ToList(),
                DisplayMemberPath = "Name",
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(roleCombo, row++);
            grid.Children.Add(roleCombo);
            roleCombo.SelectedIndex = 0;

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(btnPanel, row++);
            var okBtn = new Button { Content = "Добавить", Width = 80, Margin = new Thickness(5) };
            var cancelBtn = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };
            okBtn.Click += (s, ev) => { win.DialogResult = true; win.Close(); };
            cancelBtn.Click += (s, ev) => { win.DialogResult = false; win.Close(); };
            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            grid.Children.Add(btnPanel);

            var spacer = new TextBlock { Height = 10 };
            Grid.SetRow(spacer, row);
            grid.Children.Add(spacer);

            win.Content = grid;

            if (win.ShowDialog() == true)
            {
                string login = loginBox.Text.Trim();
                string password = passwordBox.Password;
                string lastName = lastNameBox.Text.Trim();
                string firstName = firstNameBox.Text.Trim();
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
                {
                    MessageBox.Show("Заполните обязательные поля: логин, пароль, фамилия, имя");
                    return;
                }
                if (Core.Context.Users.Any(u => u.Login == login))
                {
                    MessageBox.Show("Логин уже занят");
                    return;
                }
                if (roleCombo.SelectedItem == null)
                {
                    MessageBox.Show("Выберите роль");
                    return;
                }

                var newUser = new Users
                {
                    Login = login,
                    Password = password,
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = middleNameBox.Text.Trim(),
                    Phone = phoneBox.Text.Trim(),
                    RoleId = ((Roles)roleCombo.SelectedItem).Id,
                    IsActive = true
                };
                Core.Context.Users.Add(newUser);
                Core.Context.SaveChanges();
                LoadUsers();
                MessageBox.Show("Пользователь добавлен");
            }
        }

        private void AddLabelAndTextBox(Grid grid, string labelText, ref int row, out TextBox textBox)
        {
            var label = new TextBlock { Text = labelText, Margin = new Thickness(0, 5, 0, 0) };
            Grid.SetRow(label, row++);
            grid.Children.Add(label);
            textBox = new TextBox { Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(textBox, row++);
            grid.Children.Add(textBox);
        }

        private void AddLabelAndPasswordBox(Grid grid, string labelText, ref int row, out PasswordBox passwordBox)
        {
            var label = new TextBlock { Text = labelText, Margin = new Thickness(0, 5, 0, 0) };
            Grid.SetRow(label, row++);
            grid.Children.Add(label);
            passwordBox = new PasswordBox { Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(passwordBox, row++);
            grid.Children.Add(passwordBox);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}