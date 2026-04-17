using System.Linq;
using System.Windows;

namespace Pr17.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string lastName = LastNameBox.Text.Trim();
            string firstName = FirstNameBox.Text.Trim();
            string middleName = MiddleNameBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
            {
                ErrorText.Text = "Заполните обязательные поля: логин, пароль, фамилия, имя";
                return;
            }

            if (Core.Context.Users.Any(u => u.Login == login))
            {
                ErrorText.Text = "Пользователь с таким логином уже существует";
                return;
            }

            var clientRole = Core.Context.Roles.FirstOrDefault(r => r.Name == "Клиент");
            if (clientRole == null)
            {
                ErrorText.Text = "Ошибка: роль 'Клиент' не найдена";
                return;
            }

            var newUser = new Users
            {
                Login = login,
                Password = password,
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                Phone = phone,
                RoleId = clientRole.Id,
                IsActive = true
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();

            MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}