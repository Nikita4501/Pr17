using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;

namespace Pr17.Pages
{
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();
            if (Core.CurrentUser?.Roles?.Name != "Менеджер")
            {
                MessageBox.Show("Доступ запрещён");
                NavigationService?.GoBack();
                return;
            }
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadAppointments();
            LoadOrders();
            LoadProducts();
            LoadManufacturers();
            LoadProductTypes();
            LoadServiceTypes();
        }

        private void LoadAppointments(string searchText = null)
        {
            var query = Core.Context.Appointments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(a =>
                    (a.Users.LastName + " " + a.Users.FirstName + " " + a.Users.MiddleName).Contains(searchText) ||
                    a.Users.Phone.Contains(searchText));
            }

            var data = query.Select(a => new
            {
                a.Id,
                ClientName = a.Users.LastName + " " + a.Users.FirstName,
                MasterName = a.Users1.LastName + " " + a.Users1.FirstName,
                ServiceTypeName = a.ServiceTypes.Name,
                a.Date,
                a.Time,
                a.Status
            }).ToList();

            AppointmentsGrid.ItemsSource = data;
        }

        private void SearchClient_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAppointments(ClientSearchBox.Text);
        }

        private void CreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            string input = Interaction.InputBox("Введите ФИО или телефон клиента", "Поиск клиента");
            if (string.IsNullOrWhiteSpace(input)) return;

            var clients = Core.Context.Users
                .Where(u => u.Roles.Name == "Клиент" &&
                    ((u.LastName + " " + u.FirstName + " " + u.MiddleName).Contains(input) ||
                     u.Phone.Contains(input)))
                .ToList();

            Users client = null;
            if (clients.Count == 0)
            {
                MessageBox.Show("Клиент не найден");
                return;
            }
            else if (clients.Count == 1)
                client = clients[0];
            else
            {
                var win = new Window { Title = "Выберите клиента", Width = 300, Height = 200, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                var lb = new ListBox { ItemsSource = clients, DisplayMemberPath = "FullName" };
                lb.SelectionChanged += (s, ev) => win.DialogResult = true;
                win.Content = lb;
                if (win.ShowDialog() == true && lb.SelectedItem != null)
                    client = lb.SelectedItem as Users;
                else return;
            }

            var services = Core.Context.ServiceTypes.ToList();
            var serviceWin = new Window { Title = "Выберите услугу", Width = 250, Height = 200, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            var serviceLb = new ListBox { ItemsSource = services, DisplayMemberPath = "Name" };
            serviceLb.SelectionChanged += (s, ev) => serviceWin.DialogResult = true;
            serviceWin.Content = serviceLb;
            if (serviceWin.ShowDialog() != true || serviceLb.SelectedItem == null) return;
            var serviceType = serviceLb.SelectedItem as ServiceTypes;

            var masters = Core.Context.MasterServices
                .Where(ms => ms.ServiceTypeId == serviceType.Id)
                .Select(ms => ms.Users)
                .Where(u => u.Roles.Name == "Мастер")
                .ToList();
            if (!masters.Any())
            {
                MessageBox.Show("Нет мастеров для этой услуги");
                return;
            }
            Users master = null;
            if (masters.Count == 1)
                master = masters[0];
            else
            {
                var masterWin = new Window { Title = "Выберите мастера", Width = 250, Height = 200 };
                var masterLb = new ListBox { ItemsSource = masters, DisplayMemberPath = "FullName" };
                masterLb.SelectionChanged += (s, ev) => masterWin.DialogResult = true;
                masterWin.Content = masterLb;
                if (masterWin.ShowDialog() == true && masterLb.SelectedItem != null)
                    master = masterLb.SelectedItem as Users;
                else return;
            }

            var slot = SelectTimeSlot(master.Id);
            if (slot == null) return;

            var appointment = new Appointments
            {
                ClientId = client.Id,
                MasterId = master.Id,
                ServiceTypeId = serviceType.Id,
                Date = slot.Value.Date,
                Time = slot.Value.Time,
                Status = "Запланирована"
            };
            Core.Context.Appointments.Add(appointment);
            Core.Context.SaveChanges();
            LoadAppointments();
            MessageBox.Show("Запись успешно создана");
        }

        private void RescheduleAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsGrid.SelectedItem == null) return;
            dynamic sel = AppointmentsGrid.SelectedItem;
            int id = sel.Id;
            var app = Core.Context.Appointments.Find(id);
            if (app == null) return;

            var slot = SelectTimeSlot(app.MasterId);
            if (slot == null) return;

            app.Date = slot.Value.Date;
            app.Time = slot.Value.Time;
            Core.Context.SaveChanges();
            LoadAppointments();
            MessageBox.Show("Запись перенесена");
        }

        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsGrid.SelectedItem == null) return;
            dynamic sel = AppointmentsGrid.SelectedItem;
            int id = sel.Id;
            var app = Core.Context.Appointments.Find(id);
            app.Status = "Отменена";
            Core.Context.SaveChanges();
            LoadAppointments();
            MessageBox.Show("Запись отменена");
        }

        private (DateTime Date, TimeSpan Time)? SelectTimeSlot(int masterId)
        {
            var win = new Window { Title = "Выберите дату и время", Width = 300, Height = 200, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var dp = new DatePicker { SelectedDate = DateTime.Today, Margin = new Thickness(5) };
            Grid.SetRow(dp, 0);
            grid.Children.Add(dp);

            var cb = new ComboBox { Margin = new Thickness(5) };
            var slots = Enumerable.Range(9, 10).Select(h => new TimeSpan(h, 0, 0)).ToList();
            cb.ItemsSource = slots;
            cb.SelectedIndex = 0;
            Grid.SetRow(cb, 1);
            grid.Children.Add(cb);

            var btn = new Button { Content = "OK", Width = 80, Margin = new Thickness(5) };
            btn.Click += (s, e) => win.DialogResult = true;
            Grid.SetRow(btn, 2);
            grid.Children.Add(btn);

            win.Content = grid;
            if (win.ShowDialog() == true)
            {
                DateTime date = dp.SelectedDate ?? DateTime.Today;
                TimeSpan time = (TimeSpan)cb.SelectedItem;
                bool isFree = !Core.Context.Appointments.Any(a =>
                    a.MasterId == masterId && a.Date == date && a.Time == time && a.Status != "Отменена");
                if (!isFree)
                {
                    MessageBox.Show("Это время уже занято");
                    return null;
                }
                return (date, time);
            }
            return null;
        }

        private void LoadOrders()
        {
            var data = Core.Context.Orders.Select(o => new
            {
                o.Id,
                ClientName = o.Users.LastName + " " + o.Users.FirstName,
                o.OrderDate,
                o.DeliveryDate,
                TotalAmount = o.OrderItems.Sum(oi => oi.Price * oi.Quantity),
                o.Status
            }).ToList();
            OrdersGrid.ItemsSource = data;
        }

        private void MarkOrderDelivered_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem == null) return;
            dynamic sel = OrdersGrid.SelectedItem;
            int id = sel.Id;
            var order = Core.Context.Orders.Find(id);
            if (order.Status == "Выдан")
            {
                MessageBox.Show("Заказ уже выдан");
                return;
            }
            order.Status = "Выдан";
            Core.Context.SaveChanges();
            LoadOrders();
            MessageBox.Show("Заказ отмечен как выданный");
        }

        private void LoadProducts()
        {
            var data = Core.Context.Products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.Discount,
                p.IsActive
            }).ToList();
            ProductsGrid.ItemsSource = data;
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = new Products { Name = "Новый товар", Price = 0, IsActive = true };
            if (EditProductDialog(product, true))
            {
                Core.Context.Products.Add(product);
                Core.Context.SaveChanges();
                LoadProducts();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null) return;
            dynamic sel = ProductsGrid.SelectedItem;
            int id = sel.Id;
            var product = Core.Context.Products.Find(id);
            if (EditProductDialog(product, false))
            {
                Core.Context.SaveChanges();
                LoadProducts();
            }
        }

        private bool EditProductDialog(Products product, bool isNew)
        {
            string name = Interaction.InputBox("Название товара", "Редактирование", product.Name);
            if (string.IsNullOrWhiteSpace(name)) return false;
            string priceStr = Interaction.InputBox("Цена", "Редактирование", product.Price.ToString());
            if (!decimal.TryParse(priceStr, out decimal price)) return false;
            string discountStr = Interaction.InputBox("Скидка % (0-100)", "Редактирование", product.Discount.ToString());
            if (!int.TryParse(discountStr, out int discount) || discount < 0 || discount > 100) return false;

            product.Name = name;
            product.Price = price;
            product.Discount = discount;
            return true;
        }

        private void ToggleProductActive_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null) return;
            dynamic sel = ProductsGrid.SelectedItem;
            int id = sel.Id;
            var product = Core.Context.Products.Find(id);
            product.IsActive = !product.IsActive;
            Core.Context.SaveChanges();
            LoadProducts();
        }

        private void SetDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null) return;
            dynamic sel = ProductsGrid.SelectedItem;
            int id = sel.Id;
            var product = Core.Context.Products.Find(id);
            string input = Interaction.InputBox("Введите скидку в %", "Установка скидки", product.Discount.ToString());
            if (int.TryParse(input, out int discount) && discount >= 0 && discount <= 100)
            {
                product.Discount = discount;
                Core.Context.SaveChanges();
                LoadProducts();
            }
            else MessageBox.Show("Некорректное значение");
        }

        private void LoadManufacturers()
        {
            ManufacturersGrid.ItemsSource = Core.Context.Manufacturers.ToList();
        }

        private void AddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Название производителя", "Добавление");
            if (!string.IsNullOrWhiteSpace(name))
            {
                Core.Context.Manufacturers.Add(new Manufacturers { Name = name });
                Core.Context.SaveChanges();
                LoadManufacturers();
            }
        }

        private void EditManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (ManufacturersGrid.SelectedItem is Manufacturers man)
            {
                string name = Interaction.InputBox("Новое название", "Редактирование", man.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    man.Name = name;
                    Core.Context.SaveChanges();
                    LoadManufacturers();
                }
            }
        }

        private void LoadProductTypes()
        {
            ProductTypesGrid.ItemsSource = Core.Context.ProductTypes.ToList();
        }

        private void AddProductType_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Название типа товара", "Добавление");
            if (!string.IsNullOrWhiteSpace(name))
            {
                Core.Context.ProductTypes.Add(new ProductTypes { Name = name });
                Core.Context.SaveChanges();
                LoadProductTypes();
            }
        }

        private void EditProductType_Click(object sender, RoutedEventArgs e)
        {
            if (ProductTypesGrid.SelectedItem is ProductTypes pt)
            {
                string name = Interaction.InputBox("Новое название", "Редактирование", pt.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    pt.Name = name;
                    Core.Context.SaveChanges();
                    LoadProductTypes();
                }
            }
        }

        private void LoadServiceTypes()
        {
            ServiceTypesGrid.ItemsSource = Core.Context.ServiceTypes.ToList();
        }

        private void AddServiceType_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Название типа услуги", "Добавление");
            if (!string.IsNullOrWhiteSpace(name))
            {
                Core.Context.ServiceTypes.Add(new ServiceTypes { Name = name });
                Core.Context.SaveChanges();
                LoadServiceTypes();
            }
        }

        private void EditServiceType_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceTypesGrid.SelectedItem is ServiceTypes st)
            {
                string name = Interaction.InputBox("Новое название", "Редактирование", st.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    st.Name = name;
                    Core.Context.SaveChanges();
                    LoadServiceTypes();
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}