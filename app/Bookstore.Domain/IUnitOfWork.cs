namespace Bookstore.Domain
{
    // ISSUE-23: the range of one atomic change, made into a thing you can name.
    //
    // Every repository used to carry its own "commit", and committing through any one of them
    // committed the changes made through all of them. Placing an order relies on that — the order,
    // the stock levels of several books and the shopping cart all have to move together
    // (ISSUE-18) — but nothing in the model said so. The code read "save the order repository"
    // and meant "save everything", and that only worked because of how the repositories happened
    // to be implemented.
    //
    // ISSUE-18 recommends keeping that strong consistency for this domain: a second-hand shop
    // usually holds one copy of a book, so selling the same copy twice is worse than the cost of
    // committing three aggregates at once. What was missing was saying where the boundary is.
    // A service now takes a unit of work and completes it, and the reader can see exactly which
    // changes are inside it.
    public interface IUnitOfWork
    {
        // Commits everything changed through any repository since the last completion.
        Task CompleteAsync();
    }
}
