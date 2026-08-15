using Bookstore.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ShoppingCartRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        async Task IShoppingCartRepository.AddAsync(ShoppingCart shoppingCart)
        {
            await dbContext.ShoppingCart.AddAsync(shoppingCart);
        }

        // Returns null when no matching cart exists; callers check for that (see
        // ShoppingCartService), even though the interface keeps the return type non-nullable
        // (see IOfferRepository.GetAsync(string, int) for the same shape). The null-forgiving
        // operator here just matches that existing, deliberate contract.
        async Task<ShoppingCart> IShoppingCartRepository.GetAsync(string correlationId)
        {
            return (await dbContext.ShoppingCart
                .Include(x => x.ShoppingCartItems)
                .ThenInclude(x => x.Book)
                .SingleOrDefaultAsync(x => x.CorrelationId == correlationId))!;
        }

    }
}
