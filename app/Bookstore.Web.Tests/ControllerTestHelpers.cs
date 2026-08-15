using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace Bookstore.Web.Tests;

// Every controller under test reads the correlation-id cookie and/or the "sub" claim through
// HttpContext, and several write through TempData. A bare `new FooController(...)` throws on
// either before an action runs, so every test needs this wiring; centralising it here keeps each
// test about its own behaviour rather than about ASP.NET Core plumbing.
internal static class ControllerTestHelpers
{
    public const string Sub = "sub-1";

    public static T WithContext<T>(this T controller, string? sub = Sub, string? shoppingCartCookie = null)
        where T : Controller
    {
        var httpContext = new DefaultHttpContext();

        if (sub != null)
        {
            var identity = new ClaimsIdentity(new[] { new Claim("sub", sub) }, "TestAuth");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        if (shoppingCartCookie != null)
        {
            httpContext.Request.Headers.Append("Cookie", $"ShoppingCartId={shoppingCartCookie}");
        }

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());

        return controller;
    }
}
