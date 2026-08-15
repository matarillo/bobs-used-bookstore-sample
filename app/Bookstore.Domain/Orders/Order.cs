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

        public DateTime DeliveryDate { get; set; } = DateTime.Now.AddDays(7);

        // INV-ORDER-02/03/04, revised by ISSUE-15: the status only moves through the behaviours
        // below, which each check the current state before transitioning.
        public OrderStatus OrderStatus { get; private set; } = OrderStatus.Pending;

        public decimal Tax => SubTotal * 0.1m;

        public decimal SubTotal => OrderItems.Sum(x => x.SubTotal);

        public decimal Total => SubTotal + Tax;

        public void AddOrderItem(Book book, int quantity)
        {
            orderItems.Add(new OrderItem(this, book, quantity));
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

        // RULE-ORDER-05, revised by ISSUE-15: INV-ORDER-04 keeps a shipped, delivered or already
        // cancelled order from being cancelled again.
        public void Cancel()
        {
            if (OrderStatus != OrderStatus.Pending && OrderStatus != OrderStatus.Ordered)
            {
                throw new DomainException($"Order {Id} cannot be cancelled from the \"{OrderStatus}\" state.");
            }

            OrderStatus = OrderStatus.Cancelled;
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