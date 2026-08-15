using System.Security.Claims;
using Bookstore.Web.Helpers;

namespace Bookstore.Web.Tests.Helpers;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetSub_ReturnsTheSubClaimValue_When_Present()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "sub-1") }));

        Assert.Equal("sub-1", principal.GetSub());
    }

    [Fact]
    public void GetSub_ReturnsNull_When_TheSubClaimIsMissing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Null(principal.GetSub());
    }
}
