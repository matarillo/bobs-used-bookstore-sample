using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Http;

namespace Bookstore.Web.Tests.Helpers;

public class MaxFileSizeAttributeTests
{
    private readonly MaxFileSizeAttribute sut = new(1024);

    [Fact]
    public void IsValid_ReturnsTrue_When_TheValueIsNull()
    {
        Assert.True(sut.IsValid(null!));
    }

    [Fact]
    public void IsValid_ReturnsTrue_When_TheFileIsExactlyAtTheLimit()
    {
        Assert.True(sut.IsValid(CreateFile(1024)));
    }

    [Fact]
    public void IsValid_ReturnsFalse_When_TheFileExceedsTheLimit()
    {
        Assert.False(sut.IsValid(CreateFile(1025)));
    }

    private static IFormFile CreateFile(long length) =>
        new FormFile(new MemoryStream(), 0, length, "cover", "cover.png");
}
