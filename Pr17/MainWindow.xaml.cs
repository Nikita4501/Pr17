using System.Windows;
using Pr17.Pages;

namespace Pr17
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new StartPage());
        }
    }
}