using Bookstore.Domain;

namespace Bookstore.Data
{
    // The one place the shared "commit" lives. Every repository is built on the same scoped
    // DbContext, so committing here commits the changes made through all of them. Registered per
    // scope alongside the repositories, so a request has exactly one.
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
