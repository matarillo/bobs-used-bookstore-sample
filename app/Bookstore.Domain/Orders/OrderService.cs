using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Orders
{
    public interface IOrderService
    {
        Task<PagedResult<Order>> GetOrdersAsync(OrderFilters filters, int pageIndex = 1, int pageSize = 10);

        Task<IEnumerable<Order>> GetOrdersAsync(string sub);

        // Unscoped: for staff, who may look up any order (UC-ADMIN-05/06).
        Task<Order> GetOrderAsync(int id);

        // RULE-CUST-01: the customer-safe lookup — returns null unless the order belongs to the
        // given subject. Customer-facing routes must use this, not the unscoped overload above.
        // Extended to ISSUE-21's fix, which gave Offer the same pairing; Order's own unscoped
        // getter was already being used by two customer routes without this check.
        Task<Order> GetOrderAsync(string sub, int id);

        Task<OrderStatistics> GetStatisticsAsync();

        Task<CreateOrderResult> CreateOrderAsync(CreateOrderDto createOrderDto);

        Task AcceptOrderAsync(int orderId);

        Task ShipOrderAsync(int orderId);

        Task DeliverOrderAsync(int orderId);

        Task CancelOrderAsync(CancelOrderDto cancelOrderDto);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly IShoppingCartRepository shoppingCartRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IUnitOfWork unitOfWork;

        public OrderService(IOrderRepository orderRepository,
            IShoppingCartRepository shoppingCartRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork)
        {
            this.orderRepository = orderRepository;
            this.shoppingCartRepository = shoppingCartRepository;
            this.customerRepository = customerRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<Order>> GetOrdersAsync(OrderFilters filters, int pageIndex = 1, int pageSize = 10)
        {
            return await orderRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(string sub)
        {
            return await orderRepository.ListAsync(sub);
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            return await orderRepository.GetAsync(id);
        }

        public async Task<Order> GetOrderAsync(string sub, int id)
        {
            return await orderRepository.GetAsync(id, sub);
        }

        public async Task<OrderStatistics> GetStatisticsAsync()
        {
            return (await orderRepository.GetStatisticsAsync()) ?? new OrderStatistics();
        }

        public async Task<CreateOrderResult> CreateOrderAsync(CreateOrderDto dto)
        {
            var shoppingCart = await shoppingCartRepository.GetAsync(dto.CorrelationId);

            // ISSUE-13: placing an order is a strict update targeting one specific cart and one
            // specific customer; either missing is a failure rather than a silent no-op.
            if (shoppingCart == null)
            {
                throw new DomainException($"Shopping cart \"{dto.CorrelationId}\" was not found.");
            }

            var customer = await customerRepository.GetAsync(dto.CustomerSub);

            if (customer == null)
            {
                throw new DomainException($"Customer \"{dto.CustomerSub}\" was not found.");
            }

            // INV-ORDER-06, revised by ISSUE-11: throws if there is nothing in stock to order,
            // instead of silently placing an order with zero items. Items skipped because they
            // are out of stock are reported back so the caller can tell the customer.
            var itemsToOrder = shoppingCart.GetItemsForNewOrder();
            var skippedItems = shoppingCart.GetOutOfStockWantedItems();

            var order = new Order(customer.Id, dto.AddressId);

            await orderRepository.AddAsync(order);

            foreach (var item in itemsToOrder)
            {
                order.AddOrderItem(item.Book, item.Quantity);

                item.Book.ReduceStockLevel(item.Quantity);

                shoppingCart.RemoveShoppingCartItemById(item.Id);
            }

            // ISSUE-23/ISSUE-18: this is the boundary the design doc asked to be made visible.
            // Placing an order changes three aggregates — the new order, the stock level of each
            // book ordered, and the cart the items came out of — and all of them go in together
            // or not at all. That strong consistency is deliberate for a second-hand shop, where
            // there is usually one copy of a book and selling it twice is the worse outcome.
            await unitOfWork.CompleteAsync();

            var skippedItemDtos = skippedItems
                .Select(x => new SkippedOrderItemDto(x.BookId, x.Book.Name, x.Quantity))
                .ToList();

            return new CreateOrderResult(order.Id, skippedItemDtos);
        }

        public async Task AcceptOrderAsync(int orderId)
        {
            await TransitionAsync(orderId, order => order.Accept());
        }

        public async Task ShipOrderAsync(int orderId)
        {
            await TransitionAsync(orderId, order => order.Ship());
        }

        public async Task DeliverOrderAsync(int orderId)
        {
            await TransitionAsync(orderId, order => order.Deliver());
        }

        private async Task TransitionAsync(int orderId, Action<Order> transition)
        {
            var order = await orderRepository.GetAsync(orderId);

            transition(order);

            order.UpdatedOn = DateTime.UtcNow;

            await unitOfWork.CompleteAsync();
        }

        // ISSUE-13: deliberately kept tolerant, unlike the strict lookups elsewhere in this
        // class. Cancelling is idempotent by nature — the design doc calls it out by name as the
        // exception to "updates are strict" — so an order that is already gone or was never the
        // caller's is treated as already cancelled rather than as an error.
        public async Task CancelOrderAsync(CancelOrderDto dto)
        {
            var order = await orderRepository.GetAsync(dto.OrderId, dto.CustomerSub);

            if (order == null) return;

            order.Cancel();

            await unitOfWork.CompleteAsync();
        }
    }
}