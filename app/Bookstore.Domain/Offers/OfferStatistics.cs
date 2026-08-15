namespace Bookstore.Domain.Offers
{
    public class OfferStatistics
    {
        public int PendingOffers { get; set; }

        public int OffersThisMonth { get; set; }

        public int OffersTotal { get; set; }

        // The buying half of the monetary indicators (12 §2.4). How many offers are waiting was
        // measurable; what they would cost to accept was not.
        public decimal PendingOffersValue { get; set; }

        // What the store has actually paid customers, dated by Offer.PaidOn.
        public decimal PurchasesThisMonth { get; set; }

        public decimal PurchasesTotal { get; set; }
    }
}
