namespace Bookstore.Domain.Books
{
    public record CreateBookDto(
        string Name,
        string Author,
        int BookTypeId,
        int ConditionId,
        int GenreId,
        int PublisherId,
        int? Year,
        string ISBN,
        string Summary,
        Money Price,
        Quantity Quantity,
        Stream CoverImage,
        string CoverImageFileName);

    // Stocking a paid offer. Name, author, ISBN and the four classifications are not asked for —
    // they come from the offer.
    public record CreateBookFromOfferDto(
        int OfferId,
        int? Year,
        string Summary,
        Money Price,
        Stream CoverImage,
        string CoverImageFileName);

    public record UpdateBookDto(
        int BookId,
        string Name,
        string Author,
        int BookTypeId,
        int ConditionId,
        int GenreId,
        int PublisherId,
        int? Year,
        string ISBN,
        string Summary,
        Money Price,
        Quantity Quantity,
        Stream CoverImage,
        string CoverImageFileName);
}
