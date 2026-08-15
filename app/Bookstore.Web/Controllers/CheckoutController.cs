using Bookstore.Domain;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Orders;
using Bookstore.Web.Helpers;
using Bookstore.Web.ViewModel.Checkout;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IAddressService addressService;
        private readonly IShoppingCartService shoppingCartService;
        private readonly IOrderService orderService;

        public CheckoutController(IShoppingCartService shoppingCartService,
                                  IOrderService orderService,
                                  IAddressService addressService)
        {
            this.shoppingCartService = shoppingCartService;
            this.orderService = orderService;
            this.addressService = addressService;
        }

        public async Task<IActionResult> Index()
        {
            var shoppingCart = await shoppingCartService.GetShoppingCartAsync(HttpContext.GetShoppingCartCorrelationId());
            var addresses = await addressService.GetAddressesAsync(User.GetSub());

            return View(new CheckoutIndexViewModel(shoppingCart, addresses));
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutIndexViewModel model)
        {
            var dto = new CreateOrderDto(User.GetSub(), HttpContext.GetShoppingCartCorrelationId(), model.SelectedAddressId);

            CreateOrderResult result;

            try
            {
                result = await orderService.CreateOrderAsync(dto);
            }
            catch (DomainException ex)
            {
                // ISSUE-11: an empty cart, an out-of-stock cart, or a shortage caught by
                // ISSUE-03's stock check all surface here as a customer-facing message instead
                // of falling through to the generic error page.
                ModelState.AddModelError(string.Empty, ex.Message);

                var shoppingCart = await shoppingCartService.GetShoppingCartAsync(HttpContext.GetShoppingCartCorrelationId());
                var addresses = await addressService.GetAddressesAsync(User.GetSub());

                return View(new CheckoutIndexViewModel(shoppingCart, addresses) { SelectedAddressId = model.SelectedAddressId });
            }

            if (result.SkippedItems.Count > 0)
            {
                var skippedBookNames = string.Join(", ", result.SkippedItems.Select(x => x.BookName));

                this.SetNotification($"These items were out of stock and were not included in your order: {skippedBookNames}. They are still in your cart.");
            }

            return RedirectToAction("Finished", new { orderId = result.OrderId });
        }

        public async Task<IActionResult> Finished(int orderId)
        {
            // RULE-CUST-01, ISSUE-21: only the owner may see this order.
            var order = await orderService.GetOrderAsync(User.GetSub(), orderId);

            if (order == null) return NotFound();

            return View(new CheckoutFinishedViewModel(order));
        }
    }
}
