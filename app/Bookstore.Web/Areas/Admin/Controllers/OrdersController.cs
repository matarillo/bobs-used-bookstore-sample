using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Web.Areas.Admin.Models.Orders;
using Bookstore.Domain.Orders;

namespace Bookstore.Web.Areas.Admin.Controllers
{
    public class OrdersController : AdminAreaControllerBase
    {
        private readonly IOrderService orderService;

        public OrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        public async Task<IActionResult> Index(OrderFilters filters, int pageIndex = 1, int pageSize = 10)
        {
            var orders = await orderService.GetOrdersAsync(filters, pageIndex, pageSize);

            return View(new OrderIndexViewModel(orders, filters));
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await orderService.GetOrderAsync(id);

            return View(new OrderDetailsViewModel(order));
        }

        [HttpPost]
        public async Task<IActionResult> AcceptAsync(int id)
        {
            return await UpdateOrderStatus(id, () => orderService.AcceptOrderAsync(id), "The order has been accepted");
        }

        [HttpPost]
        public async Task<IActionResult> ShipAsync(int id)
        {
            return await UpdateOrderStatus(id, () => orderService.ShipOrderAsync(id), "The order has been shipped");
        }

        [HttpPost]
        public async Task<IActionResult> DeliverAsync(int id)
        {
            return await UpdateOrderStatus(id, () => orderService.DeliverOrderAsync(id), "The order has been marked as delivered");
        }

        private async Task<IActionResult> UpdateOrderStatus(int id, Func<Task> transition, string message)
        {
            await transition();

            TempData["Message"] = message;

            return RedirectToAction("Details", new { id });
        }
    }
}
