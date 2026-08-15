using Bookstore.Domain.Orders;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests
{
    public class OrderTests
    {
        [Fact]
        public void SubTotal_IgnoresTheQuantity_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 3);

            Assert.Equal(10m, order.SubTotal);
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
        public void SubTotal_Changes_When_TheBookPriceIsChangedAfterTheOrderIsPlaced()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, 1);

            book.Price = 25m;

            Assert.Equal(25m, order.SubTotal);
        }

        [Fact]
        public void OrderStatus_IsPending_When_TheOrderIsCreated()
        {
            var order = new Order(1, 1);

            Assert.Equal(OrderStatus.Pending, order.OrderStatus);
        }
    }
}
