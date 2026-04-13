using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pr17.Windows
{
    public partial class OrderCheckoutWindow : Window
    {
        public OrderCheckoutWindow()
        {
            InitializeComponent();
            // Установка диапазона дат
            DeliveryDatePicker.DisplayDateStart = DateTime.Today;
            DeliveryDatePicker.DisplayDateEnd = DateTime.Today.AddDays(7);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не авторизован");
                return;
            }

            // Проверка наличия товаров в корзине
            var cartItems = Core.Context.Cart.Where(c => c.UserId == Core.CurrentUser.Id).ToList();
            if (!cartItems.Any())
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            // Проверка выбранной даты
            if (DeliveryDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату получения");
                return;
            }

            DateTime deliveryDate = DeliveryDatePicker.SelectedDate.Value.Date;
            if (deliveryDate < DateTime.Today || deliveryDate > DateTime.Today.AddDays(7))
            {
                MessageBox.Show("Дата получения должна быть в пределах 7 дней от сегодняшнего дня");
                return;
            }

            // Получение способа оплаты
            string paymentMethod = ((ComboBoxItem)PaymentMethodCombo.SelectedItem).Content.ToString();

            // Создание заказа
            var order = new Orders
            {
                ClientId = Core.CurrentUser.Id,
                OrderDate = DateTime.Now,
                DeliveryDate = deliveryDate,
                PaymentMethod = paymentMethod,
                Status = "Новый"
            };
            Core.Context.Orders.Add(order);
            Core.Context.SaveChanges();

            // Перенос товаров из корзины в заказ
            foreach (var item in cartItems)
            {
                Core.Context.OrderItems.Add(new OrderItems
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Products.Price // цена на момент заказа
                });
            }

            // Очистка корзины
            Core.Context.Cart.RemoveRange(cartItems);
            Core.Context.SaveChanges();

            MessageBox.Show($"Заказ №{order.Id} успешно оформлен. Дата получения: {deliveryDate.ToShortDateString()}");
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}