using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pr17.Windows;

namespace Pr17.Pages
{
    public partial class ProductsPage : Page
    {
        private List<Products> _allProducts;

        public ProductsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _allProducts = Core.Context.Products.ToList();

            var productTypes = Core.Context.ProductTypes.ToList();
            var allProductTypes = new List<ProductTypes> { new ProductTypes { Id = 0, Name = "Все" } };
            allProductTypes.AddRange(productTypes);
            TypeFilter.ItemsSource = allProductTypes;
            TypeFilter.DisplayMemberPath = "Name";
            TypeFilter.SelectedIndex = 0;

            var manufacturers = Core.Context.Manufacturers.ToList();
            var allManufacturers = new List<Manufacturers> { new Manufacturers { Id = 0, Name = "Все" } };
            allManufacturers.AddRange(manufacturers);
            ManufacturerFilter.ItemsSource = allManufacturers;
            ManufacturerFilter.DisplayMemberPath = "Name";
            ManufacturerFilter.SelectedIndex = 0;

            SortCombo.SelectedIndex = 0;

            ApplyFilters(null, null);
        }

        private void ApplyFilters(object sender, System.EventArgs e)
        {
            var query = _allProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                query = query.Where(p => p.Name.Contains(SearchBox.Text));

            if (TypeFilter.SelectedItem is ProductTypes selectedType && selectedType.Id != 0)
                query = query.Where(p => p.ProductTypeId == selectedType.Id);

            if (ManufacturerFilter.SelectedItem is Manufacturers selectedMan && selectedMan.Id != 0)
                query = query.Where(p => p.ManufacturerId == selectedMan.Id);

            var sortItem = SortCombo.SelectedItem as ComboBoxItem;
            if (sortItem?.Tag.ToString() == "RatingAsc")
                query = query.OrderBy(p => p.Rating);
            else if (sortItem?.Tag.ToString() == "RatingDesc")
                query = query.OrderByDescending(p => p.Rating);

            var productViewModels = query.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Discount,
                p.Rating,
                p.ImagePath,
                IsLoggedIn = Core.CurrentUser != null
            }).ToList();

            ProductsItemsControl.ItemsSource = productViewModels;
        }

        private void ProductDetail_Click(object sender, RoutedEventArgs e)
        {
            var product = ((Button)sender).Tag;
            int id = (int)product.GetType().GetProperty("Id").GetValue(product);
            new ProductDetailWindow(id).ShowDialog();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Для добавления в корзину необходимо авторизоваться");
                return;
            }
            var product = ((Button)sender).Tag;
            int id = (int)product.GetType().GetProperty("Id").GetValue(product);
            var existing = Core.Context.Cart.FirstOrDefault(c => c.UserId == Core.CurrentUser.Id && c.ProductId == id);
            if (existing != null)
                existing.Quantity++;
            else
                Core.Context.Cart.Add(new Cart { UserId = Core.CurrentUser.Id, ProductId = id, Quantity = 1 });
            Core.Context.SaveChanges();
            MessageBox.Show("Товар добавлен в корзину");
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Для просмотра корзины необходимо авторизоваться");
                return;
            }
            NavigationService?.Navigate(new CartPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}