namespace Bookstore.Web.Areas.Admin.Models.Dashboard
{
    public class DashboardIndexViewModel
    {
        public int PendingOrders { get; set; }

        public int PastDueOrders { get; set; }

        public int OrdersThisMonth { get; set; }

        public int OrdersTotal { get; set; }

        // The monetary indicators (12 §2.4). Sales are net of tax; gross profit covers only the
        // sales whose cost the domain knows, which is why the sales it covers are shown with it.
        public decimal SalesThisMonth { get; set; }

        public decimal SalesTotal { get; set; }

        public decimal GrossProfitThisMonth { get; set; }

        public decimal SalesWithKnownCostThisMonth { get; set; }

        public int PendingOffers { get; set; }

        public int PastDueOffers { get; set; }

        public int OffersThisMonth { get; set; }

        public int OffersTotal { get; set; }

        public decimal PendingOffersValue { get; set; }

        public decimal PurchasesThisMonth { get; set; }

        public decimal PurchasesTotal { get; set; }

        public int OutOfStock { get; set; }

        public int LowStock { get; set; }

        public int StockTotal { get; set; }
    }
}
