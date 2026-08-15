using Bookstore.Domain;

namespace Bookstore.Data
{
    // ISSUE-23: the one place the shared "commit" now lives. Every repository is built on the
    // same scoped DbContext, which is what made committing through any of them commit all of
    // them; that is no longer an accident of the implementation but the whole purpose of this
    // class. Registered per scope alongside the repositories, so a request has exactly one.
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CompleteAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
