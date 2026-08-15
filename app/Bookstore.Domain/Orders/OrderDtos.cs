namespace Bookstore.Domain.Orders
{
    public record CreateOrderDto(string CustomerSub, string CorrelationId, int AddressId);

    public record CancelOrderDto(string CustomerSub, int OrderId);

    // ISSUE-11: alongside the new order, reports which cart items (if any) could not be
    // included because they were out of stock, so the caller can tell the customer.
    public record CreateOrderResult(int OrderId, IReadOnlyList<SkippedOrderItemDto> SkippedItems);

    public record SkippedOrderItemDto(int BookId, string BookName, int Quantity);
}