namespace Bookstore.Domain.Orders
{
    public class OrderStatistics
    {
        public int PendingOrders { get; set; }

        // Counted by Order.PastDueAsOf.
        public int PastDueOrders { get; set; }

        public int OrdersThisMonth { get; set; }

        public int OrdersTotal { get; set; }

        // The selling half of the monetary indicators. Sales are net of tax — tax is collected
        // for someone else — and exclude cancelled orders (Order.CountsAsSale).
        public decimal SalesThisMonth { get; set; }

        public decimal SalesTotal { get; set; }

        // Sales less what the sold books cost, over the sales whose cost the domain knows: those
        // stocked from an offer. Books entered by hand have no recorded cost and are left out
        // rather than counted as pure profit.
        public decimal GrossProfitThisMonth { get; set; }

        public decimal GrossProfitTotal { get; set; }

        public decimal SalesWithKnownCostThisMonth { get; set; }

        public decimal SalesWithKnownCostTotal { get; set; }
    }
}
