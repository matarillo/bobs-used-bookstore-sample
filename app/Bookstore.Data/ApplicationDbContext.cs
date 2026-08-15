using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Address> Address { get; set; }

        public DbSet<Book> Book { get; set; }

        public DbSet<Customer> Customer { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<ShoppingCart> ShoppingCart { get; set; }

        public DbSet<ShoppingCartItem> ShoppingCartItem { get; set; }

        public DbSet<OrderItem> OrderItem { get; set; }

        public DbSet<Offer> Offer { get; set; }

        public DbSet<ReferenceDataItem> ReferenceData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureMoneyAndQuantity(modelBuilder);

            modelBuilder.Entity<Customer>().HasIndex(x => x.Sub).IsUnique();

            modelBuilder.Entity<Book>().HasOne(x => x.Publisher).WithMany().HasForeignKey(x => x.PublisherId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Book>().HasOne(x => x.BookType).WithMany().HasForeignKey(x => x.BookTypeId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Book>().HasOne(x => x.Genre).WithMany().HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Book>().HasOne(x => x.Condition).WithMany().HasForeignKey(x => x.ConditionId).OnDelete(DeleteBehavior.Restrict);

            // ISSUE-06: Book.SourceOfferId points back at the offer the book was bought as. There
            // is no navigation property: the book keeps the identifier only, and the cost it was
            // bought for is copied onto the book itself.
            modelBuilder.Entity<Book>().HasOne<Offer>().WithMany().HasForeignKey(x => x.SourceOfferId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Offer>().HasOne(x => x.Publisher).WithMany().HasForeignKey(x => x.PublisherId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Offer>().HasOne(x => x.BookType).WithMany().HasForeignKey(x => x.BookTypeId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Offer>().HasOne(x => x.Genre).WithMany().HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Offer>().HasOne(x => x.Condition).WithMany().HasForeignKey(x => x.ConditionId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<Order>().HasOne(x => x.Customer).WithMany().OnDelete(DeleteBehavior.Restrict);

            PopulateDatabase(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        // ISSUE-07: Money and Quantity are the domain's way of talking about amounts and counts;
        // the database's way is a decimal and an int. Each aggregate keeps an internal property
        // for the column and derives the value from it, so the column names and the read-side
        // queries are exactly what they were before the values were introduced.
        private static void ConfigureMoneyAndQuantity(ModelBuilder modelBuilder)
        {
            // ISSUE-05: the classification is a view over the four foreign keys, which stay mapped.
            modelBuilder.Entity<Book>().Ignore(x => x.Classification);
            modelBuilder.Entity<Offer>().Ignore(x => x.Classification);

            modelBuilder.Entity<Book>().Ignore(x => x.Price).Ignore(x => x.Quantity).Ignore(x => x.PurchaseCost);
            modelBuilder.Entity<Book>().Property(x => x.PriceAmount).HasColumnName("Price");
            modelBuilder.Entity<Book>().Property(x => x.StockQuantity).HasColumnName("Quantity");
            modelBuilder.Entity<Book>().Property(x => x.PurchaseCostAmount).HasColumnName("PurchaseCost");

            modelBuilder.Entity<OrderItem>().Ignore(x => x.Price).Ignore(x => x.Quantity).Ignore(x => x.Cost);
            modelBuilder.Entity<OrderItem>().Property(x => x.PriceAmount).HasColumnName("Price");
            modelBuilder.Entity<OrderItem>().Property(x => x.QuantityValue).HasColumnName("Quantity");
            modelBuilder.Entity<OrderItem>().Property(x => x.CostAmount).HasColumnName("Cost");

            modelBuilder.Entity<ShoppingCartItem>().Ignore(x => x.Quantity);
            modelBuilder.Entity<ShoppingCartItem>().Property(x => x.QuantityValue).HasColumnName("Quantity");

            modelBuilder.Entity<Offer>().Ignore(x => x.BookPrice);
            modelBuilder.Entity<Offer>().Property(x => x.BookPriceAmount).HasColumnName("BookPrice");
        }
    }
}