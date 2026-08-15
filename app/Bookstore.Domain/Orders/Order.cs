using System.Linq.Expressions;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Orders
{
    public class Order : Entity
    {
        public Order(int customerId, int addressId)
        {
            CustomerId = customerId;
            AddressId = addressId;
        }

        private readonly List<OrderItem> orderItems = new List<OrderItem>();

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int AddressId { get; set; }
        public Address Address { get; set; }

        public IEnumerable<OrderItem> OrderItems => orderItems;

        // RULE-ORDER-01, revised by ISSUE-25: seven days out, counted from UTC. The date used to
        // be taken from the local time of the machine while the only thing that read it — the
        // past-due count — compared it against UTC, so the two ends of that comparison did not
        // share a basis (ISSUE-09).
        public DateTime DeliveryDate { get; set; } = DateTime.UtcNow.AddDays(7);

        // INV-ORDER-02/03/04, revised by ISSUE-15: the status only moves through the behaviours
        // below, which each check the current state before transitioning.
        public OrderStatus OrderStatus { get; private set; } = OrderStatus.Pending;

        // The rate the store charges. Named because ISSUE-07 is about amounts saying what they
        // are: a tenth buried in an expression says nothing about what it is a tenth of.
        public const decimal TaxRate = 0.1m;

        public Money Tax => SubTotal.Times(TaxRate);

        public Money SubTotal => OrderItems.Sum(x => x.SubTotal);

        public Money Total => SubTotal + Tax;

        public void AddOrderItem(Book book, Quantity quantity)
        {
            // ISSUE-07: Quantity rules out a negative count; a line of an order also has to be
            // for at least one copy, which is the aggregate's rule rather than the value's.
            if (quantity.IsNone)
            {
                throw new DomainException($"An order line for \"{book.Name}\" must be for at least one copy.");
            }

            orderItems.Add(new OrderItem(this, book, quantity));
        }

        // The monetary indicators (12 §2.4) count an order as revenue unless it was cancelled: a
        // cancelled order was never charged and its stock came back. Nothing else in the
        // lifecycle changes whether it is a sale — an order is an agreement from the moment it is
        // placed.
        public bool CountsAsSale => OrderStatus != OrderStatus.Cancelled;

        // The same rule for the read side, which sums these amounts across every order.
        public static Expression<Func<Order, bool>> SalesFilter()
        {
            return order => order.OrderStatus != OrderStatus.Cancelled;
        }

        // ISSUE-25: "past due" used to exist only as a line on the dashboard, with its rule
        // written into the query that counted it. The order decides what it means: the delivery
        // date promised by RULE-ORDER-01 has passed and the order has still not reached the
        // customer. A cancelled order is not past due — it is never going to be delivered, and
        // nothing about it is waiting to be chased.
        //
        // "Now" is passed in rather than read here, so the rule is testable and so the caller
        // decides the basis (UTC, as DeliveryDate is).
        public bool IsPastDue(DateTime asOfUtc)
        {
            return DeliveryDate < asOfUtc
                && OrderStatus != OrderStatus.Delivered
                && OrderStatus != OrderStatus.Cancelled;
        }

        // The same rule, in the form the read side needs to hand to the database. OrderTests pins
        // this to IsPastDue so the two cannot drift apart.
        public static Expression<Func<Order, bool>> PastDueAsOf(DateTime asOfUtc)
        {
            return order => order.DeliveryDate < asOfUtc
                && order.OrderStatus != OrderStatus.Delivered
                && order.OrderStatus != OrderStatus.Cancelled;
        }

        // The store accepts a pending order.
        public void Accept()
        {
            RequireStatus(OrderStatus.Pending, "be accepted");

            OrderStatus = OrderStatus.Ordered;
        }

        // The store ships an accepted order.
        public void Ship()
        {
            RequireStatus(OrderStatus.Ordered, "be shipped");

            OrderStatus = OrderStatus.Shipped;
        }

        // The store records that a shipped order has reached the customer.
        public void Deliver()
        {
            RequireStatus(OrderStatus.Shipped, "be marked as delivered");

            OrderStatus = OrderStatus.Delivered;
        }

        // RULE-ORDER-05, revised by ISSUE-15/16: INV-ORDER-04 keeps a shipped, delivered or
        // already cancelled order from being cancelled again, and the stock withdrawn for each
        // item is returned to its book as part of cancelling.
        public void Cancel()
        {
            if (OrderStatus != OrderStatus.Pending && OrderStatus != OrderStatus.Ordered)
            {
                throw new DomainException($"Order {Id} cannot be cancelled from the \"{OrderStatus}\" state.");
            }

            OrderStatus = OrderStatus.Cancelled;

            foreach (var orderItem in orderItems)
            {
                orderItem.Book.RestoreStockLevel(orderItem.Quantity);
            }
        }

        private void RequireStatus(OrderStatus required, string action)
        {
            if (OrderStatus != required)
            {
                throw new DomainException($"Order {Id} cannot {action} from the \"{OrderStatus}\" state.");
            }
        }
    }
}