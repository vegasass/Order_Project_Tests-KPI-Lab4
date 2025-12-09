using Order_Project.Models;
using Order_Project.Services.Intefraces;

namespace Order_Project.Services
{
    public class OrderService
    {
        private readonly IInventoryService _inventory;
        private readonly IPaymentService _payment;
        private readonly INotificationService _notification;
        private readonly List<Order> _orders = new List<Order>();

        public OrderService(IInventoryService inventory, IPaymentService payment, INotificationService notification)
        {
            _inventory = inventory;
            _payment = payment;
            _notification = notification;
        }

        public Order CreateOrder(string product, int quantity)
        {
            ValidateInput(product, quantity);

            if (!_inventory.CheckStock(product, quantity))
                throw new InvalidOperationException("Not enough stock.");

            var order = new Order { Id = _orders.Count + 1, Product = product, Quantity = quantity };

            ProcessOrder(order);
            FinalizeOrder(order);

            return order;
        }

        private void ValidateInput(string product, int quantity)
        {
            if (string.IsNullOrEmpty(product))
                throw new ArgumentException("Product name required.");
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive.");
        }

        private void ProcessOrder(Order order)
        {
            _inventory.ReduceStock(order.Product, order.Quantity);
            order.IsPaid = _payment.ProcessPayment(order);
        }

        private void FinalizeOrder(Order order)
        {
            if (order.IsPaid)
            {
                _orders.Add(order);
                _notification.SendConfirmation(order);
            }
            else
            {
                _inventory.IncreaseStock(order.Product, order.Quantity);
                throw new InvalidOperationException("Payment failed.");
            }
        }

        public bool UpdateOrder(int orderId, int newQuantity)
        {
            var order = _orders.Find(o => o.Id == orderId);
            if (order == null)
                return false;

            if (newQuantity <= 0)
                return false;

            order.Quantity = newQuantity;
            return true;
        }

        public bool RemoveOrder(int orderId)
        {
            var order = _orders.Find(o => o.Id == orderId);
            if (order == null)
                return false;

            _inventory.IncreaseStock(order.Product, order.Quantity);
            _orders.Remove(order);
            return true;
        }

        public List<Order> GetOrders() => _orders;
    }
}

