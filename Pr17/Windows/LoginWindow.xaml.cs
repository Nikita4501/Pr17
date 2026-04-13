using System.Linq;
using System.Windows;

namespace Pr17.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Введите логин и пароль";
                return;
            }

            var user = Core.Context.Users
                .FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user == null)
            {
                ErrorText.Text = "Неверный логин или пароль";
                return;
            }

            if (!user.IsActive)
            {
                ErrorText.Text = "Пользователь заблокирован";
                return;
            }

            Core.CurrentUser = user;
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