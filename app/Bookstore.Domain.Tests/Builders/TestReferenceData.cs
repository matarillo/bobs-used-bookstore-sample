using Bookstore.Domain.ReferenceData;

namespace Bookstore.Domain.Tests.Builders;

// A classification can only be made by looking the items up, so the builders need something to
// look them up in. One item of each of the four types.
public static class TestReferenceData
{
    public const int PublisherId = 1;
    public const int BookTypeId = 2;
    public const int GenreId = 3;
    public const int ConditionId = 4;

    public static readonly IReadOnlyList<ReferenceDataItem> Items = new[]
    {
        new ReferenceDataItem(ReferenceDataType.Publisher, "publisher") { Id = PublisherId },
        new ReferenceDataItem(ReferenceDataType.BookType, "book type") { Id = BookTypeId },
        new ReferenceDataItem(ReferenceDataType.Genre, "genre") { Id = GenreId },
        new ReferenceDataItem(ReferenceDataType.Condition, "condition") { Id = ConditionId }
    };

    public static BookClassification Classification(
        int publisherId = PublisherId,
        int bookTypeId = BookTypeId,
        int genreId = GenreId,
        int conditionId = ConditionId) =>
        BookClassification.Of(Items, publisherId, bookTypeId, genreId, conditionId);
}
