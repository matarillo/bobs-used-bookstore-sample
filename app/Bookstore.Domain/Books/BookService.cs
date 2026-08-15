using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.Domain.Orders;

namespace Bookstore.Domain.Books
{
    public interface IBookService
    {
        Task<Book> GetBookAsync(int id);

        Task<PagedResult<Book>> GetBooksAsync(BookFilters filters, int pageIndex, int pageSize);

        Task<PagedResult<Book>> GetBooksAsync(string searchString, string sortBy, int pageIndex, int pageSize);

        Task<IEnumerable<Book>> ListBestSellingBooksAsync(int count);

        Task<BookStatistics> GetStatisticsAsync();

        Task<BookResult> AddAsync(CreateBookDto createBookDto);

        // ISSUE-06: puts a paid offer on the shelf, recording it as the source of the stock.
        Task<BookResult> AddFromOfferAsync(CreateBookFromOfferDto createBookFromOfferDto);

        Task<BookResult> UpdateAsync(UpdateBookDto updateBookDto);
    }

    public class BookService : IBookService
    {
        private readonly IImageResizeService imageResizeService;
        private readonly IImageValidationService imageValidationService;
        private readonly IFileService fileService;
        private readonly IBookRepository bookRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOfferRepository offerRepository;
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IUnitOfWork unitOfWork;

        public BookService(IImageResizeService imageResizeService, IImageValidationService imageValidationService, IFileService fileService, IBookRepository bookRepository, IOrderRepository orderRepository, IOfferRepository offerRepository, IReferenceDataRepository referenceDataRepository, IUnitOfWork unitOfWork)
        {
            this.imageResizeService = imageResizeService;
            this.imageValidationService = imageValidationService;
            this.fileService = fileService;
            this.bookRepository = bookRepository;
            this.orderRepository = orderRepository;
            this.offerRepository = offerRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Book> GetBookAsync(int id)
        {
            return await bookRepository.GetAsync(id);
        }

        public async Task<PagedResult<Book>> GetBooksAsync(BookFilters filters, int pageIndex, int pageSize)
        {
            return await bookRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<PagedResult<Book>> GetBooksAsync(string searchString, string sortBy, int pageIndex, int pageSize)
        {
            return await bookRepository.ListAsync(searchString, sortBy, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Book>> ListBestSellingBooksAsync(int count)
        {
            return await orderRepository.ListBestSellingBooksAsync(count);
        }

        public async Task<BookStatistics> GetStatisticsAsync()
        {
            return (await bookRepository.GetStatisticsAsync()) ?? new BookStatistics();
        }

        public async Task<BookResult> AddAsync(CreateBookDto dto)
        {
            var book = new Book(
                dto.Name,
                dto.Author,
                dto.ISBN,
                await ClassifyAsync(dto.PublisherId, dto.BookTypeId, dto.GenreId, dto.ConditionId),
                dto.Price,
                dto.Quantity,
                dto.Year,
                dto.Summary);

            await bookRepository.AddAsync(book);

            return await SaveAsync(book, dto.CoverImage, dto.CoverImageFileName);
        }

        // ISSUE-06: the offer and the book it becomes are changed together, which the shared unit
        // of work behind the repositories allows (see CreateOrderAsync for the same pattern).
        public async Task<BookResult> AddFromOfferAsync(CreateBookFromOfferDto dto)
        {
            var offer = await offerRepository.GetAsync(dto.OfferId);

            // ISSUE-13: stocking is a strict update targeting one specific offer.
            if (offer == null)
            {
                throw new DomainException($"Offer {dto.OfferId} was not found.");
            }

            var book = Book.CreateFromOffer(offer, dto.Price, dto.Year, dto.Summary);

            await bookRepository.AddAsync(book);

            return await SaveAsync(book, dto.CoverImage, dto.CoverImageFileName);
        }

        public async Task<BookResult> UpdateAsync(UpdateBookDto dto)
        {
            var book = await bookRepository.GetAsync(dto.BookId);

            book.Name = dto.Name;
            book.Author = dto.Author;
            book.ISBN = dto.ISBN;
            book.Classification = await ClassifyAsync(dto.PublisherId, dto.BookTypeId, dto.GenreId, dto.ConditionId);
            book.Price = dto.Price;
            book.Quantity = dto.Quantity;
            book.Year = dto.Year;
            book.Summary = dto.Summary;
            book.UpdatedOn = DateTime.UtcNow;

            await bookRepository.UpdateAsync(book);

            return await SaveAsync(book, dto.CoverImage, dto.CoverImageFileName);
        }

        // ISSUE-05: the four identifiers arrive from four dropdowns, and nothing but the shape of
        // the form ever said the one in the genre position was a genre. Checked here, against the
        // reference data they were chosen from, before the book is built.
        private async Task<BookClassification> ClassifyAsync(int publisherId, int bookTypeId, int genreId, int conditionId)
        {
            var referenceData = await referenceDataRepository.FullListAsync();

            return BookClassification.Of(referenceData, publisherId, bookTypeId, genreId, conditionId);
        }

        private async Task<BookResult> SaveAsync(Book book, Stream? coverImage, string coverImageFileName)
        {
            var resizedCoverImage = await ResizeImageAsync(coverImage);

            // ISSUE-22: the safety check has to guard what actually ends up on the shelf. It used
            // to run against the image as uploaded, while the resized image — a different stream —
            // was what got saved; resizing was silently trusted not to introduce anything unsafe.
            var imageIsSafe = await imageValidationService.IsSafeAsync(resizedCoverImage);

            if (!imageIsSafe) return new BookResult(false, "The image failed the safety check. Please try another image.");

            await SaveImageAsync(book, resizedCoverImage, coverImageFileName);

            await unitOfWork.CompleteAsync();

            return new BookResult(true, null);
        }

        private async Task<Stream?> ResizeImageAsync(Stream? coverImage)
        {
            if (coverImage == null) return null;

            return await imageResizeService.ResizeImageAsync(coverImage);
        }

        private async Task SaveImageAsync(Book book, Stream? coverImage, string? coverImageFilename)
        {
            var imageUrl = await fileService.SaveAsync(coverImage, coverImageFilename);

            if (coverImage != null)
            {
                await fileService.DeleteAsync(book.CoverImageUrl);
                book.CoverImageUrl = imageUrl;
            }
        }
    }
}