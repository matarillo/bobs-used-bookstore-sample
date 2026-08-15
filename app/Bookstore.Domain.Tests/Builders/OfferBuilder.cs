using Bookstore.Domain.Offers;

namespace Bookstore.Domain.Tests.Builders;

public class OfferBuilder
{
    private int id;
    private int customerId = 1;
    private string bookName = "test";
    private string author = "author";
    private string isbn = "12345678";
    private int bookTypeId = 2;
    private int conditionId = 3;
    private int genreId = 4;
    private int publisherId = 5;
    private decimal bookPrice = 5;

    public Offer Build()
    {
        return new Offer(customerId, bookName, author, isbn, bookTypeId, conditionId, genreId, publisherId, bookPrice)
        {
            Id = id
        };
    }

    public OfferBuilder Id(int value)
    {
        id = value;
        return this;
    }

    public OfferBuilder CustomerId(int value)
    {
        customerId = value;
        return this;
    }
}
