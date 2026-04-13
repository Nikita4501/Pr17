using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pr17.Windows;

namespace Pr17.Pages
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться");
                NavigationService?.GoBack();
                return;
            }
            LoadCart();
        }

        private void LoadCart()
        {
            var cartItems = Core.Context.Cart
                .Where(c => c.UserId == Core.CurrentUser.Id)
                .Select(c => new
                {
                    c.Id,
                    c.ProductId,
                    ProductName = c.Products.Name,
                    c.Products.Price,
                    c.Quantity
                }).ToList();
            CartItemsControl.ItemsSource = cartItems;
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).Tag;
            int cartId = (int)item.GetType().GetProperty("Id").GetValue(item);
            var cartItem = Core.Context.Cart.Find(cartId);
            cartItem.Quantity++;
            Core.Context.SaveChanges();
            LoadCart();
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).Tag;
            int cartId = (int)item.GetType().GetProperty("Id").GetValue(item);
            var cartItem = Core.Context.Cart.Find(cartId);
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                Core.Context.SaveChanges();
            }
            else
            {
                Core.Context.Cart.Remove(cartItem);
                Core.Context.SaveChanges();
            }
            LoadCart();
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).Tag;
            int cartId = (int)item.GetType().GetProperty("Id").GetValue(item);
            var cartItem = Core.Context.Cart.Find(cartId);
            Core.Context.Cart.Remove(cartItem);
            Core.Context.SaveChanges();
            LoadCart();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            new OrderCheckoutWindow().ShowDialog();
            LoadCart();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}