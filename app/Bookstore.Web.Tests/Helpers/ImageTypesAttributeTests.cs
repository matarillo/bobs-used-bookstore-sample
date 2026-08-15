using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Http;

namespace Bookstore.Web.Tests.Helpers;

// Guards the one path a customer-supplied image enters the domain through (book covers, and
// ISSUE-20's offer covers). Bypassing the extension check here is bypassing
// POL-IMAGE-SAFETY/POL-IMAGE-RESIZE further downstream, so the extension list itself has to hold.
public class ImageTypesAttributeTests
{
    private readonly ImageTypesAttribute sut = new(new[] { ".png", ".jpg" });

    [Fact]
    public void IsValid_ReturnsTrue_When_TheValueIsNull()
    {
        Assert.True(sut.IsValid(null!));
    }

    [Theory]
    [InlineData("cover.png")]
    [InlineData("cover.PNG")]
    [InlineData("cover.jpg")]
    public void IsValid_ReturnsTrue_ForAnAllowedExtension(string fileName)
    {
        Assert.True(sut.IsValid(CreateFile(fileName)));
    }

    [Theory]
    [InlineData("cover.gif")]
    [InlineData("cover.exe")]
    [InlineData("cover")]
    public void IsValid_ReturnsFalse_ForADisallowedExtension(string fileName)
    {
        Assert.False(sut.IsValid(CreateFile(fileName)));
    }

    private static IFormFile CreateFile(string fileName) =>
        new FormFile(new MemoryStream(), 0, 0, "cover", fileName);
}
