using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.Tests.Builders;
using NSubstitute;

namespace Bookstore.Domain.Tests
{
    public class OrderServiceTests
    {
        private readonly IOrderRepository orderRepository = Substitute.For<IOrderRepository>();
        private readonly IShoppingCartRepository shoppingCartRepository = Substitute.For<IShoppingCartRepository>();
        private readonly ICustomerRepository customerRepository = Substitute.For<ICustomerRepository>();
        private readonly OrderService sut;

        public OrderServiceTests()
        {
            sut = new OrderService(orderRepository, shoppingCartRepository, customerRepository);
        }

        [Fact]
        public async Task CreateOrderAsync_Throws_When_TheShoppingCartIsNotFound()
        {
            shoppingCartRepository.GetAsync("cart-1").Returns((ShoppingCart)null!);

            var dto = new CreateOrderDto("sub-1", "cart-1", 1);

            await Assert.ThrowsAsync<DomainException>(() => sut.CreateOrderAsync(dto));
        }

        [Fact]
        public async Task CreateOrderAsync_Throws_When_TheCustomerIsNotFound()
        {
            var book = new BookBuilder().Id(1).Quantity(5).Build();
            var cart = new ShoppingCartBuilder().WithShoppingCartItem(book, 1).Build();

            shoppingCartRepository.GetAsync("cart-1").Returns(cart);
            customerRepository.GetAsync("sub-1").Returns((Customer)null!);

            var dto = new CreateOrderDto("sub-1", "cart-1", 1);

            await Assert.ThrowsAsync<DomainException>(() => sut.CreateOrderAsync(dto));
        }

        [Fact]
        public async Task CreateOrderAsync_AddsAnOrderItemAndReducesStock_ForEachInStockItem()
        {
            var firstBook = new BookBuilder().Id(1).Quantity(5).Build();
            var secondBook = new BookBuilder().Id(2).Quantity(5).Build();
            var cart = new ShoppingCartBuilder()
                .WithShoppingCartItem(firstBook, 2)
                .WithShoppingCartItem(secondBook, 3)
                .Build();
            var customer = new Customer("sub-1") { Id = 42 };

            shoppingCartRepository.GetAsync("cart-1").Returns(cart);
            customerRepository.GetAsync("sub-1").Returns(customer);

            Order? addedOrder = null;
            await orderRepository.AddAsync(Arg.Do<Order>(o => addedOrder = o));

            var dto = new CreateOrderDto("sub-1", "cart-1", 7);

            await sut.CreateOrderAsync(dto);

            Assert.NotNull(addedOrder);
            Assert.Equal(42, addedOrder!.CustomerId);
            Assert.Equal(7, addedOrder.AddressId);
            Assert.Equal(2, addedOrder.OrderItems.Count());
            Assert.Equal(Quantity.Of(3), firstBook.Quantity);
            Assert.Equal(Quantity.Of(2), secondBook.Quantity);
            Assert.Empty(cart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems));
            await orderRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrderAsync_SkipsOutOfStockItems_ButLeavesThemInTheCart()
        {
            var inStockBook = new BookBuilder().Id(1).Quantity(5).Build();
            var outOfStockBook = new BookBuilder().Id(2).Quantity(0).Build();
            var cart = new ShoppingCartBuilder()
                .WithShoppingCartItem(inStockBook, 1)
                .WithShoppingCartItem(outOfStockBook, 1)
                .Build();
            var customer = new Customer("sub-1") { Id = 1 };

            shoppingCartRepository.GetAsync("cart-1").Returns(cart);
            customerRepository.GetAsync("sub-1").Returns(customer);

            var dto = new CreateOrderDto("sub-1", "cart-1", 1);

            var result = await sut.CreateOrderAsync(dto);

            var skipped = Assert.Single(result.SkippedItems);
            Assert.Equal(outOfStockBook.Id, skipped.BookId);
            Assert.Contains(cart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems), x => x.BookId == outOfStockBook.Id);
        }

        [Fact]
        public async Task CreateOrderAsync_Throws_When_NothingInTheCartIsInStock()
        {
            var outOfStockBook = new BookBuilder().Id(1).Quantity(0).Build();
            var cart = new ShoppingCartBuilder().WithShoppingCartItem(outOfStockBook, 1).Build();
            var customer = new Customer("sub-1") { Id = 1 };

            shoppingCartRepository.GetAsync("cart-1").Returns(cart);
            customerRepository.GetAsync("sub-1").Returns(customer);

            var dto = new CreateOrderDto("sub-1", "cart-1", 1);

            await Assert.ThrowsAsync<DomainException>(() => sut.CreateOrderAsync(dto));
        }

        [Fact]
        public async Task AcceptOrderAsync_TransitionsTheOrderAndSaves()
        {
            var order = new Order(1, 1) { Id = 5 };
            orderRepository.GetAsync(5).Returns(order);

            await sut.AcceptOrderAsync(5);

            Assert.Equal(OrderStatus.Ordered, order.OrderStatus);
            await orderRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ShipOrderAsync_TransitionsTheOrderAndSaves()
        {
            var order = new Order(1, 1) { Id = 5 };
            order.Accept();
            orderRepository.GetAsync(5).Returns(order);

            await sut.ShipOrderAsync(5);

            Assert.Equal(OrderStatus.Shipped, order.OrderStatus);
            await orderRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeliverOrderAsync_TransitionsTheOrderAndSaves()
        {
            var order = new Order(1, 1) { Id = 5 };
            order.Accept();
            order.Ship();
            orderRepository.GetAsync(5).Returns(order);

            await sut.DeliverOrderAsync(5);

            Assert.Equal(OrderStatus.Delivered, order.OrderStatus);
            await orderRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CancelOrderAsync_DoesNothing_When_TheOrderIsNotFound()
        {
            orderRepository.GetAsync(5, "sub-1").Returns((Order)null!);

            var dto = new CancelOrderDto("sub-1", 5);

            await sut.CancelOrderAsync(dto);

            await orderRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task CancelOrderAsync_CancelsTheOrderAndSaves_When_TheOrderIsFound()
        {
            var order = new Order(1, 1) { Id = 5 };
            orderRepository.GetAsync(5, "sub-1").Returns(order);

            var dto = new CancelOrderDto("sub-1", 5);

            await sut.CancelOrderAsync(dto);

            Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
            await orderRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GetOrderAsync_ScopedToTheOwner_DelegatesToTheRepository()
        {
            var order = new Order(1, 1) { Id = 5 };
            orderRepository.GetAsync(5, "sub-1").Returns(order);

            var result = await sut.GetOrderAsync("sub-1", 5);

            Assert.Same(order, result);
        }

        [Fact]
        public async Task GetStatisticsAsync_ReturnsEmptyStatistics_When_TheRepositoryHasNone()
        {
            orderRepository.GetStatisticsAsync().Returns((OrderStatistics)null!);

            var result = await sut.GetStatisticsAsync();

            Assert.Equal(0, result.OrdersTotal);
            Assert.Equal(0, result.PendingOrders);
        }
    }
}
