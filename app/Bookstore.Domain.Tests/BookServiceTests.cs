using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.Domain.Orders;
using Bookstore.Domain.Tests.Builders;
using NSubstitute;

namespace Bookstore.Domain.Tests
{
    public class BookServiceTests
    {
        private readonly IImageResizeService imageResizeService = Substitute.For<IImageResizeService>();
        private readonly IImageValidationService imageValidationService = Substitute.For<IImageValidationService>();
        private readonly IFileService fileService = Substitute.For<IFileService>();
        private readonly IBookRepository bookRepository = Substitute.For<IBookRepository>();
        private readonly IOrderRepository orderRepository = Substitute.For<IOrderRepository>();
        private readonly IOfferRepository offerRepository = Substitute.For<IOfferRepository>();
        private readonly IReferenceDataRepository referenceDataRepository = Substitute.For<IReferenceDataRepository>();
        private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly BookService sut;

        public BookServiceTests()
        {
            referenceDataRepository.FullListAsync().Returns(TestReferenceData.Items);
            fileService.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>()).Returns("http://cover.dummy/saved.png");

            sut = new BookService(imageResizeService, imageValidationService, fileService,
                bookRepository, orderRepository, offerRepository, referenceDataRepository, unitOfWork);
        }

        // ISSUE-22, pinned before the fix: the safety check ran on the image as uploaded, while
        // what got saved was the resized image handed back by ResizeImageAsync. The two could
        // differ, and only the unchecked one ever reached the shelf.
        [Fact]
        public async Task AddAsync_ValidatesTheUploadedImage_NotTheResizedOne()
        {
            using var uploaded = new MemoryStream();
            using var resized = new MemoryStream();
            imageResizeService.ResizeImageAsync(uploaded).Returns(resized);
            imageValidationService.IsSafeAsync(Arg.Any<Stream>()).Returns(true);

            var dto = CreateBookDto(uploaded);

            await sut.AddAsync(dto);

            await imageValidationService.Received(1).IsSafeAsync(uploaded);
            await imageValidationService.DidNotReceive().IsSafeAsync(resized);
        }

        [Fact]
        public async Task AddAsync_SavesTheResizedImage_When_ItPassesValidation()
        {
            using var uploaded = new MemoryStream();
            using var resized = new MemoryStream();
            imageResizeService.ResizeImageAsync(uploaded).Returns(resized);
            imageValidationService.IsSafeAsync(Arg.Any<Stream>()).Returns(true);

            var dto = CreateBookDto(uploaded);

            var result = await sut.AddAsync(dto);

            Assert.True(result.IsSuccess);
            await fileService.Received(1).SaveAsync(resized, dto.CoverImageFileName);
        }

        [Fact]
        public async Task AddAsync_DoesNotSaveTheBookOrTheImage_When_ValidationFails()
        {
            using var uploaded = new MemoryStream();
            using var resized = new MemoryStream();
            imageResizeService.ResizeImageAsync(uploaded).Returns(resized);
            imageValidationService.IsSafeAsync(Arg.Any<Stream>()).Returns(false);

            var dto = CreateBookDto(uploaded);

            var result = await sut.AddAsync(dto);

            Assert.False(result.IsSuccess);
            await fileService.DidNotReceive().SaveAsync(Arg.Any<Stream>(), Arg.Any<string>());
            await unitOfWork.DidNotReceive().CompleteAsync();
        }

        private static CreateBookDto CreateBookDto(Stream coverImage) => new(
            "test", "author", TestReferenceData.BookTypeId, TestReferenceData.ConditionId,
            TestReferenceData.GenreId, TestReferenceData.PublisherId, 2000, "12345678",
            "summary", Money.Of(10m), Quantity.Of(1), coverImage, "cover.png");
    }
}
