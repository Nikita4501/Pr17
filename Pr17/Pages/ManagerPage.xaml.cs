using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pr17.Pages
{
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();
            if (Core.CurrentUser?.Roles.Name != "Менеджер")
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
            var query = Core.Context.Appointments
                .Include("Users")
                .Include("Users1")
                .Include("ServiceTypes")
                .AsQueryable();

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
            var client = SelectClient();
            if (client == null) return;

            var serviceType = SelectServiceType();
            if (serviceType == null) return;

            var master = SelectMasterForService(serviceType.Id);
            if (master == null) return;

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
            MessageBox.Show("Запись создана");
        }

        private void RescheduleAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            dynamic selected = AppointmentsGrid.SelectedItem;
            int id = selected.Id;
            var appointment = Core.Context.Appointments.Find(id);
            if (appointment == null) return;

            var slot = SelectTimeSlot(appointment.MasterId);
            if (slot == null) return;

            appointment.Date = slot.Value.Date;
            appointment.Time = slot.Value.Time;
            Core.Context.SaveChanges();
            LoadAppointments();
            MessageBox.Show("Запись перенесена");
        }

        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (AppointmentsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            dynamic selected = AppointmentsGrid.SelectedItem;
            int id = selected.Id;
            var appointment = Core.Context.Appointments.Find(id);
            appointment.Status = "Отменена";
            Core.Context.SaveChanges();
            LoadAppointments();
            MessageBox.Show("Запись отменена");
        }

        private void LoadOrders()
        {
            var data = Core.Context.Orders
                .Include("Users")
                .Include("OrderItems")
                .Select(o => new
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
            if (OrdersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            dynamic selected = OrdersGrid.SelectedItem;
            int id = selected.Id;
            var order = Core.Context.Orders.Find(id);
            if (order.Status != "Новый" && order.Status != "В обработке")
            {
                MessageBox.Show("Заказ уже выдан или отменён");
                return;
            }

            order.Status = "Выдан";
            Core.Context.SaveChanges();
            LoadOrders();
            MessageBox.Show("Заказ отмечен как выданный");
        }

        private void LoadProducts()
        {
            var data = Core.Context.Products
                .Select(p => new
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
            var product = new Products
            {
                Name = "Новый товар",
                Price = 0,
                IsActive = true
            };
            if (EditProductDialog(product))
            {
                Core.Context.Products.Add(product);
                Core.Context.SaveChanges();
                LoadProducts();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null) return;
            dynamic selected = ProductsGrid.SelectedItem;
            int id = selected.Id;
            var product = Core.Context.Products.Find(id);
            if (EditProductDialog(product))
            {
                Core.Context.SaveChanges();
                LoadProducts();
            }
        }

        private void ToggleProductActive_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null) return;
            dynamic selected = ProductsGrid.SelectedItem;
            int id = selected.Id;
            var product = Core.Context.Products.Find(id);
            product.IsActive = !product.IsActive;
            Core.Context.SaveChanges();
            LoadProducts();
        }

        private void SetDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null) return;
            dynamic selected = ProductsGrid.SelectedItem;
            int id = selected.Id;
            var product = Core.Context.Products.Find(id);

            string input = Interaction.InputBox("Введите скидку в %", "Установка скидки", product.Discount.ToString());
            if (int.TryParse(input, out int discount) && discount >= 0 && discount <= 100)
            {
                product.Discount = discount;
                Core.Context.SaveChanges();
                LoadProducts();
            }
            else
            {
                MessageBox.Show("Некорректное значение");
            }
        }

        private bool EditProductDialog(Products product)
        {
            string name = Interaction.InputBox("Название", "Редактирование товара", product.Name);
            if (string.IsNullOrWhiteSpace(name)) return false;
            string priceStr = Interaction.InputBox("Цена", "Редактирование товара", product.Price.ToString());
            if (!decimal.TryParse(priceStr, out decimal price)) return false;

            product.Name = name;
            product.Price = price;
            return true;
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
                string name = Microsoft.VisualBasic.Interaction.InputBox("Новое название", "Редактирование", man.Name);
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
            string name = Microsoft.VisualBasic.Interaction.InputBox("Название типа товара", "Добавление");
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
                string name = Microsoft.VisualBasic.Interaction.InputBox("Новое название", "Редактирование", pt.Name);
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
            string name = Microsoft.VisualBasic.Interaction.InputBox("Название типа услуги", "Добавление");
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
                string name = Microsoft.VisualBasic.Interaction.InputBox("Новое название", "Редактирование", st.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    st.Name = name;
                    Core.Context.SaveChanges();
                    LoadServiceTypes();
                }
            }
        }

        private Users SelectClient()
        {
            string input = Interaction.InputBox("Введите ФИО или телефон клиента", "Поиск клиента");
            if (string.IsNullOrWhiteSpace(input)) return null;

            var clients = Core.Context.Users
                .Where(u => u.Roles.Name == "Клиент" &&
                    ((u.LastName + " " + u.FirstName + " " + u.MiddleName).Contains(input) ||
                     u.Phone.Contains(input)))
                .ToList();

            if (clients.Count == 0)
            {
                MessageBox.Show("Клиент не найден");
                return null;
            }

            if (clients.Count == 1) return clients[0];

            var selectionWindow = new Window
            {
                Title = "Выберите клиента",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var listBox = new ListBox
            {
                ItemsSource = clients,
                DisplayMemberPath = "FullName"
            };
            listBox.SelectionChanged += (s, e) => selectionWindow.DialogResult = true;
            selectionWindow.Content = listBox;
            if (selectionWindow.ShowDialog() == true)
                return listBox.SelectedItem as Users;
            return null;
        }

        private ServiceTypes SelectServiceType()
        {
            var types = Core.Context.ServiceTypes.ToList();
            var selectionWindow = new Window
            {
                Title = "Выберите услугу",
                Width = 250,
                Height = 200
            };
            var listBox = new ListBox { ItemsSource = types, DisplayMemberPath = "Name" };
            listBox.SelectionChanged += (s, e) => selectionWindow.DialogResult = true;
            selectionWindow.Content = listBox;
            if (selectionWindow.ShowDialog() == true)
                return listBox.SelectedItem as ServiceTypes;
            return null;
        }

        private Users SelectMasterForService(int serviceTypeId)
        {
            var masters = Core.Context.MasterServices
                .Where(ms => ms.ServiceTypeId == serviceTypeId)
                .Select(ms => ms.Users)
                .Where(u => u.Roles.Name == "Мастер")
                .ToList();

            if (masters.Count == 0)
            {
                MessageBox.Show("Нет мастеров для этой услуги");
                return null;
            }

            var selectionWindow = new Window
            {
                Title = "Выберите мастера",
                Width = 250,
                Height = 200
            };
            var listBox = new ListBox
            {
                ItemsSource = masters,
                DisplayMemberPath = "FullName"
            };
            listBox.SelectionChanged += (s, e) => selectionWindow.DialogResult = true;
            selectionWindow.Content = listBox;
            if (selectionWindow.ShowDialog() == true)
                return listBox.SelectedItem as Users;
            return null;
        }

        private (DateTime Date, TimeSpan Time)? SelectTimeSlot(int masterId)
        {
            var window = new Window
            {
                Title = "Выберите дату и время",
                Width = 300,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var datePicker = new DatePicker { SelectedDate = DateTime.Today, Margin = new Thickness(5) };
            Grid.SetRow(datePicker, 0);
            grid.Children.Add(datePicker);

            var comboBox = new ComboBox { Margin = new Thickness(5) };
            var slots = Enumerable.Range(9, 10).Select(h => new TimeSpan(h, 0, 0)).ToList();
            comboBox.ItemsSource = slots;
            comboBox.SelectedIndex = 0;
            Grid.SetRow(comboBox, 1);
            grid.Children.Add(comboBox);

            var button = new Button { Content = "OK", Margin = new Thickness(5), Width = 80 };
            button.Click += (s, e) => window.DialogResult = true;
            Grid.SetRow(button, 2);
            grid.Children.Add(button);

            window.Content = grid;
            if (window.ShowDialog() == true)
            {
                DateTime date = datePicker.SelectedDate ?? DateTime.Today;
                TimeSpan time = (TimeSpan)comboBox.SelectedItem;
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}