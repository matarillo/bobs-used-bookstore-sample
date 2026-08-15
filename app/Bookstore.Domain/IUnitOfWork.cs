namespace Bookstore.Domain
{
    // The range of one atomic change, made into a thing you can name.
    //
    // Placing an order changes three aggregates at once — the order, the stock levels of several
    // books, and the shopping cart the items came out of — and all of them have to move together.
    // That strong consistency is deliberate for this domain: a second-hand shop usually holds one
    // copy of a book, so selling the same copy twice is worse than the cost of committing three
    // aggregates at once.
    //
    // A service takes a unit of work and completes it, so the reader can see exactly which
    // changes are inside the boundary.
    public interface IUnitOfWork
    {
        // Commits everything changed through any repository since the last completion.
        Task CompleteAsync();
    }
}
