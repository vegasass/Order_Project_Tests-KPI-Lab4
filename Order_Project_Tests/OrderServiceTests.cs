using Moq;
using Order_Project.Models;
using Order_Project.Services;
using Order_Project.Services.Intefraces;

namespace Order_Project_Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IInventoryService> _inventoryMock;
        private readonly Mock<IPaymentService> _paymentMock;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _inventoryMock = new Mock<IInventoryService>();
            _paymentMock = new Mock<IPaymentService>();
            _notificationMock = new Mock<INotificationService>();

            _service = new OrderService(_inventoryMock.Object, _paymentMock.Object, _notificationMock.Object);
        }

        /// <summary>
        /// Перевірка: метод CreateOrder повертає правильний продукт та кількість.
        /// Тип: Assert.Equal
        /// </summary>
        [Fact]
        public void CreateOrder_ReturnsCorrectOrder()
        {
            _inventoryMock.Setup(x => x.CheckStock("Phone", 2)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Order order = _service.CreateOrder("Phone", 2);

            Assert.Equal("Phone", order.Product);
            Assert.Equal(2, order.Quantity);
        }

        /// <summary>
        /// Перевірка: метод CreateOrder кидає виняток, якщо оплата непроходить.
        /// Тип: Assert.Throws
        /// </summary>
        [Fact]
        public void CreateOrder_PaymentFails_ThrowInvalidOperationException()
        {
            _inventoryMock.Setup(x => x.CheckStock("Phone", 2)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(false);

            Assert.Throws<InvalidOperationException>(() => _service.CreateOrder("Phone", 2));
        }

        /// <summary>
        /// Перевірка: метод CreateOrder кидає виняток, якщо назва продукту пуста або кілкькість негативна
        /// Тип: [Theory] / [InlineData], Assert.Throws
        /// </summary>
        [Theory]
        [InlineData("Book", -1)]
        [InlineData("", 3)]
        public void CreateOrder_ValidateFails_ThrowArgumentException(string product, int qty)
        {
            Assert.Throws<ArgumentException>(() => _service.CreateOrder(product, qty));
        }

        /// <summary>
        /// Перевірка: після створення замовлення має з’явитися у списку _orders.
        /// Тип: Assert.Contains()
        /// </summary>
        /// 
        [Fact]
        public void CreateOrder_OrderAppearsInList()
        {
            _inventoryMock.Setup(x => x.CheckStock("Phone", 2)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Order createdOrder = _service.CreateOrder("Phone", 2);
            List<Order> orders = _service.GetOrders();

            Assert.Contains(createdOrder, orders);
        }

        /// <summary>
        /// Перевірка: якщо оновити кількість у замовленні,
        /// метод UpdateOrder повинен повернути true.
        /// Assert.NotEqual()
        /// </summary>
        [Fact]
        public void UpdateOrder_ChangedQuantityInOrder()
        {
            int originalNumber = 2;
            int updatedNumber = 3;

            _inventoryMock.Setup(x => x.CheckStock("Apple", originalNumber)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Order order = _service.CreateOrder("Apple", originalNumber);

            bool result = _service.UpdateOrder(order.Id, updatedNumber);

            Assert.NotEqual(originalNumber, order.Quantity);
            Assert.True(result);
        }


        /// <summary>
        /// Перевірка: RemoveOrder викликає IncreaseStock.
        /// Тип: Verify(Times.Once)
        /// </summary>
        [Fact]
        public void RemoveOrder_IncreasesStock_WhenRemoved()
        {
            _inventoryMock.Setup(x => x.CheckStock("Phone", 1)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Order order = _service.CreateOrder("Phone", 1);

            _service.RemoveOrder(order.Id);

            _inventoryMock.Verify(x => x.IncreaseStock("Phone", 1), Times.Once);
        }


        /// <summary>
        /// Перевірка: RemoveOrder видаляє єдиний ордер з листу та колекція становиться пустою
        /// Тип: Assert.Empty();
        /// </summary>
        [Fact]
        public void RemoveOrder_ListWithOneOrder_ListIsEmpty()
        {

            _inventoryMock.Setup(x => x.CheckStock("Phone", 2)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Order order = _service.CreateOrder("Phone", 2);

            bool result = _service.RemoveOrder(order.Id);

            List<Order> list = _service.GetOrders();

            Assert.True(result);


            Assert.Empty(list);

            _inventoryMock.Verify(x => x.IncreaseStock("Phone", 2), Times.Once);
        }

        /// <summary>
        /// Перевірка: при оновленні існуючого замовлення, перевіряється що воно існує та змінилось в колекції.
        /// </summary>
        [Fact]
        public void UpdateOrder_ItsNotNull_AndReturnsTrue()
        {
            _inventoryMock.Setup(x => x.CheckStock("Phone", 2)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Order created = _service.CreateOrder("Phone", 2);

            bool result = _service.UpdateOrder(created.Id, 10);

            Assert.True(result);

            Order order = _service.GetOrders().Find(o => o.Id == created.Id);

            Assert.NotNull(order);
            Assert.Equal(10, order.Quantity);
        }

         // <summary>
         /// Перевірка: якщо RemoveOrder викликається з неіснуючим ID,
         /// метод повертає false, а IncreaseStock НІКОЛИ не викликається.
         /// Тип: Verify(Times.Never)
         /// </summary>
         [Fact]
         public void RemoveOrder_WhenOrderIsNull_IncreaseStockNeverCalled()
         {
            bool result = _service.RemoveOrder(999);

            Assert.False(result);

            _inventoryMock.Verify(x => x.IncreaseStock(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            }


        /// <summary>
        /// Перевірка: метод CreateOrder кидає виняток, якщо не вистачає на складі.
        /// Тип: Assert.Throws
        /// </summary>
        [Fact]
        public void CreateOrder_NotEnoughStock_ThrowInvalidOperationException()
        {
            _inventoryMock.Setup(x => x.CheckStock("Phone", 2)).Returns(false);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Assert.Throws<InvalidOperationException>(() => _service.CreateOrder("Phone", 2));

            _inventoryMock.Verify(x => x.ReduceStock(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        /// <summary>
        /// Перевірка: ProcessPayment викликається НЕ більше одного разу
        /// при створенні одного замовлення.
        /// Тип: Verify(Times.AtMost(n))
        /// </summary>
        [Fact]
        public void CreateOrder_ProcessPayment_CalledAtMostOnce()
        {
            _inventoryMock.Setup(x => x.CheckStock("Monitor", 2)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            _service.CreateOrder("Monitor", 2);

            _paymentMock.Verify(x => x.ProcessPayment(It.IsAny<Order>()), Times.AtMost(1));
        }


        /// <summary>
        /// Перевірка: правильність відправлення повідомлення після створення замовлення.
        /// Is.Is предикат для перевірки аргументу SendConfirmation
        /// </summary>
        [Fact]
        public void CreateOrder_SendsCorrectNotification()
        {
            _inventoryMock.Setup(x => x.CheckStock("Phone", 3)).Returns(true);
            _paymentMock.Setup(x => x.ProcessPayment(It.IsAny<Order>())).Returns(true);

            Order order = _service.CreateOrder("Phone", 3);

            _notificationMock.Verify(
                x => x.SendConfirmation(
                It.Is<Order>(o =>
                        o.Id == order.Id &&
                        o.Product == "Phone" &&
                        o.Quantity == 3 &&
                        o.IsPaid == true
                    )
                ),
                Times.Once
            );
        }


    }

}
