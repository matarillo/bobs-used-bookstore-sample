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
        decimal Price,
        int Quantity,
        Stream CoverImage,
        string CoverImageFileName);

    // ISSUE-06: stocking a paid offer. Name, author, ISBN and the four classifications are not
    // asked for — they come from the offer.
    public record CreateBookFromOfferDto(
        int OfferId,
        int? Year,
        string Summary,
        decimal Price,
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
        decimal Price,
        int Quantity,
        Stream CoverImage,
        string CoverImageFileName);
}