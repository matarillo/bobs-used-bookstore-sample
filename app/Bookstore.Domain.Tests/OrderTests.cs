using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
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
            order.AddOrderItem(book, Quantity.Of(3));

            Assert.Equal(Money.Of(30m), order.SubTotal);
        }

        [Fact]
        public void SubTotal_SumsEveryOrderItem_When_TheOrderHasSeveralItems()
        {
            var firstBook = new BookBuilder().Id(1).Price(10m).Build();
            var secondBook = new BookBuilder().Id(2).Price(5m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(firstBook, Quantity.Of(3));
            order.AddOrderItem(secondBook, Quantity.Of(2));

            Assert.Equal(Money.Of(40m), order.SubTotal);
        }

        [Fact]
        public void Tax_IsTenPercentOfTheSubTotal_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(1));

            Assert.Equal(Money.Of(1m), order.Tax);
        }

        [Fact]
        public void Total_IsTheSubTotalPlusTax_When_Executed()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(1));

            Assert.Equal(order.SubTotal + order.Tax, order.Total);
            Assert.Equal(Money.Of(11m), order.Total);
        }

        [Fact]
        public void SubTotal_IsUnchanged_When_TheBookPriceIsChangedAfterTheOrderIsPlaced()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(1));

            book.Price = Money.Of(25m);

            Assert.Equal(Money.Of(10m), order.SubTotal);
        }

        [Fact]
        public void AddOrderItem_RecordsThePriceOfTheBook_When_TheOrderItemIsAdded()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(1));

            Assert.Equal(Money.Of(10m), order.OrderItems.Single().Price);
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
            order.AddOrderItem(firstBook, Quantity.Of(3));
            order.AddOrderItem(secondBook, Quantity.Of(2));

            // Mirrors what OrderService.CreateOrderAsync does when the order is placed.
            firstBook.ReduceStockLevel(Quantity.Of(3));
            secondBook.ReduceStockLevel(Quantity.Of(2));

            order.Cancel();

            Assert.Equal(Quantity.Of(10), firstBook.Quantity);
            Assert.Equal(Quantity.Of(10), secondBook.Quantity);
        }

        [Fact]
        public void Cancel_Throws_When_TheOrderIsAlreadyCancelled_AndDoesNotRestoreStockASecondTime()
        {
            var book = new BookBuilder().Id(1).Quantity(10).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(3));

            book.ReduceStockLevel(Quantity.Of(3));
            order.Cancel();

            Assert.Throws<DomainException>(() => order.Cancel());
            Assert.Equal(Quantity.Of(10), book.Quantity);
        }

        // The monetary indicators: the profit on a sale is the difference between what the store
        // sold the book for and what it paid for it, fixed at the moment of the sale.
        [Fact]
        public void AddOrderItem_RecordsWhatTheBookCostTheStore_When_TheBookCameFromAnOffer()
        {
            var offer = new OfferBuilder().Id(1).BookPrice(4m).Status(OfferStatus.Paid).Build();
            var book = Book.CreateFromOffer(offer, price: Money.Of(10m));

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(3));

            var orderItem = order.OrderItems.Single();

            Assert.Equal(Money.Of(4m), orderItem.Cost);
            Assert.Equal(18m, orderItem.GrossProfit);
        }

        // An unknown cost is not a cost of zero: a book the store entered by hand says nothing
        // about what it was worth, so no profit is claimed on it.
        [Fact]
        public void GrossProfit_IsUnknown_When_TheBookDidNotComeFromAnOffer()
        {
            var book = new BookBuilder().Id(1).Price(10m).Build();

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(3));

            var orderItem = order.OrderItems.Single();

            Assert.Null(orderItem.Cost);
            Assert.Null(orderItem.GrossProfit);
        }

        [Fact]
        public void GrossProfit_IsUnchanged_When_TheBookIsRestockedAtADifferentCost()
        {
            var offer = new OfferBuilder().Id(1).BookPrice(4m).Status(OfferStatus.Paid).Build();
            var book = Book.CreateFromOffer(offer, price: Money.Of(10m));

            var order = new Order(1, 1);
            order.AddOrderItem(book, Quantity.Of(1));

            book.Price = Money.Of(25m);

            Assert.Equal(6m, order.OrderItems.Single().GrossProfit);
        }

        // A cancelled order is not revenue: nothing was charged and the stock came back.
        [Theory]
        [InlineData(OrderStatus.Pending, true)]
        [InlineData(OrderStatus.Ordered, true)]
        [InlineData(OrderStatus.Shipped, true)]
        [InlineData(OrderStatus.Delivered, true)]
        [InlineData(OrderStatus.Cancelled, false)]
        public void CountsAsSale_ExcludesCancelledOrdersOnly(OrderStatus status, bool expectedResult)
        {
            var order = OrderInState(status);

            Assert.Equal(expectedResult, order.CountsAsSale);
        }

        // The read side sums these amounts with the query form of the rule; it must give the same
        // answer as the aggregate.
        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Ordered)]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public void SalesFilter_AgreesWithCountsAsSale(OrderStatus status)
        {
            var order = OrderInState(status);

            Assert.Equal(order.CountsAsSale, Order.SalesFilter().Compile()(order));
        }

        // Changed by ISSUE-25, having been pinned as "seven days after the local time". The
        // delivery date is now counted from UTC, the basis IsPastDue compares it against.
        [Fact]
        public void DeliveryDate_IsSevenDaysAfterTheCurrentUtcTime_When_TheOrderIsCreated()
        {
            var order = new Order(1, 1);

            Assert.Equal(DateTime.UtcNow.AddDays(7), order.DeliveryDate, TimeSpan.FromMinutes(1));
        }

        // ISSUE-25: an order that has passed its delivery date without reaching the customer.
        [Theory]
        [InlineData(OrderStatus.Pending, true)]
        [InlineData(OrderStatus.Ordered, true)]
        [InlineData(OrderStatus.Shipped, true)]
        [InlineData(OrderStatus.Delivered, false)]
        [InlineData(OrderStatus.Cancelled, false)]
        public void IsPastDue_ReportsAnUndeliveredOrderPastItsDeliveryDate(OrderStatus status, bool expectedResult)
        {
            var order = OrderInState(status);
            order.DeliveryDate = new DateTime(2026, 1, 1);

            Assert.Equal(expectedResult, order.IsPastDue(new DateTime(2026, 1, 2)));
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Ordered)]
        [InlineData(OrderStatus.Shipped)]
        public void IsPastDue_ReturnsFalse_When_TheDeliveryDateHasNotPassed(OrderStatus status)
        {
            var order = OrderInState(status);
            order.DeliveryDate = new DateTime(2026, 1, 2);

            Assert.False(order.IsPastDue(new DateTime(2026, 1, 1)));
        }

        // ISSUE-25: the read side asks the aggregate for the rule. This holds the query form of it
        // to the same answers as the aggregate itself, so a dashboard count cannot quietly come to
        // mean something other than what the order says.
        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Ordered)]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public void PastDueAsOf_AgreesWithIsPastDue(OrderStatus status)
        {
            var asOf = new DateTime(2026, 1, 2);
            var pastDue = Order.PastDueAsOf(asOf).Compile();

            foreach (var deliveryDate in new[] { new DateTime(2026, 1, 1), new DateTime(2026, 1, 3) })
            {
                var order = OrderInState(status);
                order.DeliveryDate = deliveryDate;

                Assert.Equal(order.IsPastDue(asOf), pastDue(order));
            }
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
