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
        public void OrderStatus_CanBeSetToAnyValue_When_AssignedDirectly()
        {
            // INV-ORDER-02/03/04 are not enforced: a delivered order can be pushed back to
            // pending, with no notion of a valid or an invalid transition. Recorded here so the
            // change is visible when ISSUE-15 replaces the setter with guarded behaviours.
            var order = new Order(1, 1);

            order.OrderStatus = OrderStatus.Delivered;
            order.OrderStatus = OrderStatus.Pending;

            Assert.Equal(OrderStatus.Pending, order.OrderStatus);
        }
    }
}
