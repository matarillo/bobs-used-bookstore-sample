using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Orders
{
    public interface IOrderService
    {
        Task<IPaginatedList<Order>> GetOrdersAsync(OrderFilters filters, int pageIndex = 1, int pageSize = 10);

        Task<IEnumerable<Order>> GetOrdersAsync(string sub);

        Task<Order> GetOrderAsync(int id);

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

        public OrderService(IOrderRepository orderRepository,
            IShoppingCartRepository shoppingCartRepository,
            ICustomerRepository customerRepository)
        {
            this.orderRepository = orderRepository;
            this.shoppingCartRepository = shoppingCartRepository;
            this.customerRepository = customerRepository;
        }

        public async Task<IPaginatedList<Order>> GetOrdersAsync(OrderFilters filters, int pageIndex = 1, int pageSize = 10)
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

        public async Task<OrderStatistics> GetStatisticsAsync()
        {
            return (await orderRepository.GetStatisticsAsync()) ?? new OrderStatistics();
        }

        public async Task<CreateOrderResult> CreateOrderAsync(CreateOrderDto dto)
        {
            var shoppingCart = await shoppingCartRepository.GetAsync(dto.CorrelationId);

            var customer = await customerRepository.GetAsync(dto.CustomerSub);

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

            // Because each repository implements a unit of work, changes to the shopping cart and to stock levels
            // are captured by the unit of work and can be persisted by called SaveChangesAsync on _any_ repository.
            await orderRepository.SaveChangesAsync();

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

            await orderRepository.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(CancelOrderDto dto)
        {
            var order = await orderRepository.GetAsync(dto.OrderId, dto.CustomerSub);

            if (order == null) return;

            order.Cancel();

            await orderRepository.SaveChangesAsync();
        }
    }
}