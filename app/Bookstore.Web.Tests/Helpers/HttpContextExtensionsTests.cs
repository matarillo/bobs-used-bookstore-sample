using System.Security.Claims;
using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Http;

namespace Bookstore.Web.Tests.Helpers;

// Nearly every cart-facing controller (ShoppingCart, Wishlist, Search, Checkout) identifies "the
// current cart" through this one extension. A regression here would silently scramble carts for
// every anonymous and logged-in visitor at once, which makes it worth pinning on its own rather
// than only indirectly through whichever controller happens to call it.
public class HttpContextExtensionsTests
{
    [Fact]
    public void GetShoppingCartCorrelationId_ReturnsTheExistingCookie_When_OneIsPresent()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("Cookie", "ShoppingCartId=existing-cart-id");

        var result = httpContext.GetShoppingCartCorrelationId();

        Assert.Equal("existing-cart-id", result);
    }

    [Fact]
    public void GetShoppingCartCorrelationId_GeneratesANewId_When_TheVisitorIsAnonymousAndHasNoCookie()
    {
        var httpContext = new DefaultHttpContext();

        var result = httpContext.GetShoppingCartCorrelationId();

        Assert.True(Guid.TryParse(result, out _));
    }

    // Ties an authenticated visitor's cart to their identity rather than a random id, so a
    // logged-in customer who has never had the cookie set still lands on a stable cart.
    [Fact]
    public void GetShoppingCartCorrelationId_UsesTheUsersSub_When_AuthenticatedAndHasNoCookie()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "sub-1") }, "TestAuth")),
        };

        var result = httpContext.GetShoppingCartCorrelationId();

        Assert.Equal("sub-1", result);
    }

    [Fact]
    public void GetShoppingCartCorrelationId_AlwaysRefreshesTheResponseCookie()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("Cookie", "ShoppingCartId=existing-cart-id");

        httpContext.GetShoppingCartCorrelationId();

        var setCookie = httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("ShoppingCartId=existing-cart-id", setCookie);
    }
}
