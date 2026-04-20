using System.Linq;
using System.Windows;

namespace Pr17.Windows
{
    public partial class ProductDetailWindow : Window
    {
        private int _productId;
        private Products _product;

        public ProductDetailWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            LoadProductData();
        }

        private void LoadProductData()
        {
            _product = Core.Context.Products.Find(_productId);
            if (_product == null)
            {
                MessageBox.Show("Товар не найден");
                Close();
                return;
            }

            NameText.Text = _product.Name;
            PriceText.Text = $"Цена: {_product.Price:C}";
            if (_product.Discount > 0)
                DiscountText.Text = $"Скидка: {_product.Discount}%";
            else
                DiscountText.Visibility = Visibility.Collapsed;

            RatingText.Text = $"Рейтинг: {_product.Rating}/5";
            ManufacturerText.Text = $"Производитель: {_product.Manufacturers?.Name ?? "Не указан"}";
            TypeText.Text = $"Тип товара: {_product.ProductTypes?.Name ?? "Не указан"}";
            DescriptionText.Text = _product.Description ?? "Описание отсутствует";

            if (!string.IsNullOrEmpty(_product.ImagePath))
            {
                try
                {
                    ProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(_product.ImagePath, System.UriKind.RelativeOrAbsolute));
                }
                catch { }
            }

            AddToCartButton.IsEnabled = Core.CurrentUser != null && _product.IsActive;
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться");
                return;
            }

            var existing = Core.Context.Cart.FirstOrDefault(c => c.UserId == Core.CurrentUser.Id && c.ProductId == _productId);
            if (existing != null)
                existing.Quantity++;
            else
                Core.Context.Cart.Add(new Cart { UserId = Core.CurrentUser.Id, ProductId = _productId, Quantity = 1 });

            Core.Context.SaveChanges();
            MessageBox.Show("Товар добавлен в корзину");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}