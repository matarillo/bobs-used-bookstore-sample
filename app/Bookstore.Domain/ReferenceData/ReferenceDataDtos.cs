namespace Bookstore.Domain.ReferenceData
{
    public record CreateReferenceDataItemDto(ReferenceDataType ReferenceDataType, string Text);

    // ISSUE-05: no type here. What an item is cannot be changed once books and offers point at it.
    public record UpdateReferenceDataItemDto(int Id, string Text);
}
