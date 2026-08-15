namespace Bookstore.Domain.Offers
{
    public class OfferStatistics
    {
        public int PendingOffers { get; set; }

        public int OffersThisMonth { get; set; }

        public int OffersTotal { get; set; }

        // The buying half of the monetary indicators: what the waiting offers would cost to
        // accept, not merely how many of them there are.
        public decimal PendingOffersValue { get; set; }

        // What the store has actually paid customers, dated by Offer.PaidOn.
        public decimal PurchasesThisMonth { get; set; }

        public decimal PurchasesTotal { get; set; }
    }
}
