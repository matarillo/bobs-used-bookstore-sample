using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Bookstore.Domain.Tests.Builders;

namespace Bookstore.Domain.Tests.Scenarios;

// The rest of this project mocks one repository call at a time with NSubstitute, which is right
// for pinning a single service's behaviour but cannot show whether several services actually hand
// state to one another correctly. The scenario this supports (OfferToOrderScenarioTests) needs a
// repository that remembers what an earlier step wrote, the same thing EF Core's change tracker
// gives production code — hence a small stateful fake instead of another set of stubs.
//
// Deliberately scoped to what that one scenario drives: the read/write paths a customer's offer
// takes from submission through to becoming an order line. Everything else throws, so a test that
// starts relying on an unimplemented path fails loudly instead of silently returning nothing.
//
// All repository interfaces declare their members "protected internal" — the same reason the
// existing *ServiceTests need NSubstitute rather than a plain stub. A class outside the granted
// assemblies cannot implement them at all, and even inside one, C# only allows a non-public
// interface member to be satisfied by an *explicit* interface implementation — an ordinarily
// named public method does not count. Bookstore.Data's repositories already follow this shape;
// the fakes below match it.
internal class FakeDatabase
{
    public List<Customer> Customers { get; } = new();
    public List<Book> Books { get; } = new();
    public List<Offer> Offers { get; } = new();
    public List<ShoppingCart> ShoppingCarts { get; } = new();
    public List<Order> Orders { get; } = new();
    public IReadOnlyList<ReferenceDataItem> ReferenceData => TestReferenceData.Items;

    private int nextId;
    public int NextId() => ++nextId;

    // Mimics EF Core's SaveChanges assigning identity-column values: every new entity written by
    // a service during this "unit of work" gets an id once the work is committed, not the moment
    // it is constructed.
    public void AssignIdsToNewEntities()
    {
        foreach (var customer in Customers) AssignId(customer);
        foreach (var book in Books) AssignId(book);
        foreach (var offer in Offers) AssignId(offer);
        foreach (var order in Orders)
        {
            AssignId(order);
            foreach (var item in order.OrderItems) AssignId(item);
        }
        foreach (var cart in ShoppingCarts)
        {
            foreach (var item in cart.ShoppingCartItems) AssignId(item);
        }
    }

    private void AssignId(Entity entity)
    {
        if (entity.IsNewEntity()) entity.Id = NextId();
    }
}

internal class FakeUnitOfWork : IUnitOfWork
{
    private readonly FakeDatabase db;

    public FakeUnitOfWork(FakeDatabase db) => this.db = db;

    public int CompletedCount { get; private set; }

    public Task CompleteAsync()
    {
        db.AssignIdsToNewEntities();
        CompletedCount++;
        return Task.CompletedTask;
    }
}

internal class FakeCustomerRepository : ICustomerRepository
{
    private readonly FakeDatabase db;

    public FakeCustomerRepository(FakeDatabase db) => this.db = db;

    Task<Customer> ICustomerRepository.GetAsync(int id) => Task.FromResult(db.Customers.SingleOrDefault(x => x.Id == id))!;

    Task<Customer> ICustomerRepository.GetAsync(string sub) => Task.FromResult(db.Customers.SingleOrDefault(x => x.Sub == sub))!;

    Task ICustomerRepository.AddAsync(Customer customer)
    {
        db.Customers.Add(customer);
        return Task.CompletedTask;
    }
}

internal class FakeReferenceDataRepository : IReferenceDataRepository
{
    private readonly FakeDatabase db;

    public FakeReferenceDataRepository(FakeDatabase db) => this.db = db;

    Task<IEnumerable<ReferenceDataItem>> IReferenceDataRepository.FullListAsync() =>
        Task.FromResult<IEnumerable<ReferenceDataItem>>(db.ReferenceData);

    Task<ReferenceDataItem> IReferenceDataRepository.GetAsync(int id) => Task.FromResult(db.ReferenceData.Single(x => x.Id == id));

    Task<PagedResult<ReferenceDataItem>> IReferenceDataRepository.ListAsync(ReferenceDataFilters filters, int pageIndex, int pageSize) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    Task IReferenceDataRepository.AddAsync(ReferenceDataItem item) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");
}

internal class FakeOfferRepository : IOfferRepository
{
    private readonly FakeDatabase db;

    public FakeOfferRepository(FakeDatabase db) => this.db = db;

    Task<Offer> IOfferRepository.GetAsync(int id) => Task.FromResult(db.Offers.SingleOrDefault(x => x.Id == id))!;

    // Scoped the same way the real repository is — only an offer that belongs to the given
    // subject is returned.
    Task<Offer> IOfferRepository.GetAsync(string sub, int id)
    {
        var customer = db.Customers.SingleOrDefault(x => x.Sub == sub);
        var offer = customer == null ? null : db.Offers.SingleOrDefault(x => x.Id == id && x.CustomerId == customer.Id);
        return Task.FromResult(offer)!;
    }

    Task IOfferRepository.AddAsync(Offer offer)
    {
        db.Offers.Add(offer);
        return Task.CompletedTask;
    }

    Task<PagedResult<Offer>> IOfferRepository.ListAsync(OfferFilters filters, int pageIndex, int pageSize) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    Task<IEnumerable<Offer>> IOfferRepository.ListAsync(string sub) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    public Task<OfferStatistics> GetStatisticsAsync() =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");
}

internal class FakeBookRepository : IBookRepository
{
    private readonly FakeDatabase db;

    public FakeBookRepository(FakeDatabase db) => this.db = db;

    Task<Book> IBookRepository.GetAsync(int id) => Task.FromResult(db.Books.SingleOrDefault(x => x.Id == id))!;

    Task IBookRepository.AddAsync(Book book)
    {
        db.Books.Add(book);
        return Task.CompletedTask;
    }

    Task IBookRepository.UpdateAsync(Book book) => Task.CompletedTask; // Already the tracked instance; nothing to copy.

    Task<PagedResult<Book>> IBookRepository.ListAsync(BookFilters filters, int pageIndex, int pageSize) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    Task<PagedResult<Book>> IBookRepository.ListAsync(string searchString, string sortBy, int pageIndex, int pageSize) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    public Task<BookStatistics> GetStatisticsAsync() =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");
}

internal class FakeShoppingCartRepository : IShoppingCartRepository
{
    private readonly FakeDatabase db;

    public FakeShoppingCartRepository(FakeDatabase db) => this.db = db;

    Task IShoppingCartRepository.AddAsync(ShoppingCart shoppingCart)
    {
        db.ShoppingCarts.Add(shoppingCart);
        return Task.CompletedTask;
    }

    // EF Core eagerly loads each item's Book navigation property alongside the cart; a fake
    // standing in for it has to do the same, or OrderService's later `item.Book.ReduceStockLevel`
    // finds nothing there.
    Task<ShoppingCart> IShoppingCartRepository.GetAsync(string correlationId)
    {
        var cart = db.ShoppingCarts.SingleOrDefault(x => x.CorrelationId == correlationId);

        if (cart != null)
        {
            foreach (var item in cart.ShoppingCartItems)
            {
                item.Book ??= db.Books.Single(b => b.Id == item.BookId);
            }
        }

        return Task.FromResult(cart)!;
    }
}

internal class FakeOrderRepository : IOrderRepository
{
    private readonly FakeDatabase db;

    public FakeOrderRepository(FakeDatabase db) => this.db = db;

    Task<Order> IOrderRepository.GetAsync(int id) => Task.FromResult(db.Orders.SingleOrDefault(x => x.Id == id))!;

    // Scoped to the owner, matching the real repository.
    Task<Order> IOrderRepository.GetAsync(int id, string sub)
    {
        var customer = db.Customers.SingleOrDefault(x => x.Sub == sub);
        var order = customer == null ? null : db.Orders.SingleOrDefault(x => x.Id == id && x.CustomerId == customer.Id);
        return Task.FromResult(order)!;
    }

    Task IOrderRepository.AddAsync(Order order)
    {
        db.Orders.Add(order);
        return Task.CompletedTask;
    }

    Task<IEnumerable<Book>> IOrderRepository.ListBestSellingBooksAsync(int count) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    Task<PagedResult<Order>> IOrderRepository.ListAsync(OrderFilters filters, int pageIndex, int pageSize) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    Task<IEnumerable<Order>> IOrderRepository.ListAsync(string sub) =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");

    Task<OrderStatistics> IOrderRepository.GetStatisticsAsync() =>
        throw new NotSupportedException("Not exercised by the scenario this fake supports.");
}
