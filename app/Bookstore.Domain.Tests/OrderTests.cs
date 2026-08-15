using Bookstore.Domain.Orders;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    public class OrderTests
    {
        [Fact]
        public void SubTotal_MultipliesThePriceByTheQuantity_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 3);

            Assert.Equal(30m, order.SubTotal);
        }

        [Fact]
        public void SubTotal_SumsEveryOrderItem_When_TheOrderHasSeveralItems()
        {
            var firstBook = new BookBuilder().Id(1).Price(10m).Build();
            var secondBook = new BookBuilder().Id(2).Price(5m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(firstBook, 3);
            order.AddOrderItem(secondBook, 2);

            Assert.Equal(40m, order.SubTotal);
        }

        [Fact]
        public void Tax_IsTenPercentOfTheSubTotal_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 1);

            Assert.Equal(1m, order.Tax);
        }

        [Fact]
        public void Total_IsTheSubTotalPlusTax_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 1);

            Assert.Equal(order.SubTotal + order.Tax, order.Total);
            Assert.Equal(11m, order.Total);
        }

        [Fact]
        public void SubTotal_IsUnchanged_When_TheBookPriceIsChangedAfterTheOrderIsPlaced()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 1);

            book.Price = 25m;

            Assert.Equal(10m, order.SubTotal);
        }

        [Fact]
        public void AddOrderItem_RecordsThePriceOfTheBook_When_TheOrderItemIsAdded()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 1);

            Assert.Equal(10m, order.OrderItems.Single().Price);
        }

        [Fact]
        public void OrderStatus_IsPending_When_TheOrderIsCreated()
        {
            var order = new Order(1, 1);

            Assert.Equal(OrderStatus.Pending, order.OrderStatus);
        }

        [Fact]
        public void Accept_TransitionsToOrdered_When_TheOrderIsPending()
        {
            var order = new Order(1, 1);

            order.Accept();

            Assert.Equal(OrderStatus.Ordered, order.OrderStatus);
        }

        [Theory]
        [InlineData(OrderStatus.Ordered)]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public void Accept_Throws_When_TheOrderIsNotPending(OrderStatus status)
        {
            var order = OrderInState(status);

            Assert.Throws<DomainException>(() => order.Accept());
        }

        [Fact]
        public void Ship_TransitionsToShipped_When_TheOrderIsOrdered()
        {
            var order = OrderInState(OrderStatus.Ordered);

            order.Ship();

            Assert.Equal(OrderStatus.Shipped, order.OrderStatus);
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public void Ship_Throws_When_TheOrderIsNotOrdered(OrderStatus status)
        {
            var order = OrderInState(status);

            Assert.Throws<DomainException>(() => order.Ship());
        }

        [Fact]
        public void Deliver_TransitionsToDelivered_When_TheOrderIsShipped()
        {
            var order = OrderInState(OrderStatus.Shipped);

            order.Deliver();

            Assert.Equal(OrderStatus.Delivered, order.OrderStatus);
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Ordered)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public void Deliver_Throws_When_TheOrderIsNotShipped(OrderStatus status)
        {
            var order = OrderInState(status);

            Assert.Throws<DomainException>(() => order.Deliver());
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Ordered)]
        public void Cancel_TransitionsToCancelled_When_TheOrderIsPendingOrOrdered(OrderStatus status)
        {
            var order = OrderInState(status);

            order.Cancel();

            Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
        }

        [Theory]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public void Cancel_Throws_When_TheOrderIsShippedDeliveredOrAlreadyCancelled(OrderStatus status)
        {
            var order = OrderInState(status);

            Assert.Throws<DomainException>(() => order.Cancel());
        }

        [Fact]
        public void Cancel_RestoresTheWithdrawnStockToEachBook_When_TheOrderIsCancelled()
        {
            var firstBook = new BookBuilder().Id(1).Quantity(10).Build();
            var secondBook = new BookBuilder().Id(2).Quantity(10).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(firstBook, 3);
            order.AddOrderItem(secondBook, 2);

            // Mirrors what OrderService.CreateOrderAsync does when the order is placed.
            firstBook.ReduceStockLevel(3);
            secondBook.ReduceStockLevel(2);

            order.Cancel();

            Assert.Equal(10, firstBook.Quantity);
            Assert.Equal(10, secondBook.Quantity);
        }

        [Fact]
        public void Cancel_Throws_When_TheOrderIsAlreadyCancelled_AndDoesNotRestoreStockASecondTime()
        {
            var book = new BookBuilder().Id(1).Quantity(10).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 3);

            book.ReduceStockLevel(3);
            order.Cancel();

            Assert.Throws<DomainException>(() => order.Cancel());
            Assert.Equal(10, book.Quantity);
        }

        // Drives the order through the transitions needed to reach the given state, so each
        // test can start from an arbitrary point without relying on a raw status setter.
        private static Order OrderInState(OrderStatus status)
        {
            var order = new Order(1, 1);

            if (status == OrderStatus.Pending) return order;

            order.Accept();
            if (status == OrderStatus.Ordered) return order;

            if (status == OrderStatus.Cancelled)
            {
                order.Cancel();
                return order;
            }

            order.Ship();
            if (status == OrderStatus.Shipped) return order;

            order.Deliver();
            return order;
        }
    }
}
