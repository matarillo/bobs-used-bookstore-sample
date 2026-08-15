namespace Bookstore.Domain.Orders
{
    public class OrderFilters
    {
        public OrderStatus? OrderStatusFilter { get; set; }

        public DateTime? OrderDateFromFilter { get; set; }

        public DateTime? OrderDateToFilter { get; set; }

        // ISSUE-25: past due as defined by Order.PastDueAsOf.
        public bool? PastDueFilter { get; set; }
    }
}