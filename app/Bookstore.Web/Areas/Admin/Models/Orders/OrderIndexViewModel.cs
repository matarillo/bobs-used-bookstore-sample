using Bookstore.Domain;
using Bookstore.Web.ViewModel;
using Bookstore.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.Web.Areas.Admin.Models.Orders
{
    public class OrderIndexViewModel : PaginatedViewModel
    {
        public List<OrderIndexListItemViewModel> Items { get; set; } = new List<OrderIndexListItemViewModel>();

        public OrderFilters Filters { get; set; }

        public OrderIndexViewModel(PagedResult<Order> orderDtos, OrderFilters filters)
        {
            // ISSUE-25: read once so every row on the page is judged against the same instant.
            var now = DateTime.UtcNow;

            foreach (var order in orderDtos.Items)
            {
                Items.Add(new OrderIndexListItemViewModel
                {
                    Id = order.Id,
                    CustomerName = order.Customer.FullName,
                    OrderStatus = order.OrderStatus,
                    OrderDate = order.CreatedOn,
                    DeliveryDate = order.DeliveryDate,
                    Total = order.Total.Amount,
                    IsPastDue = order.IsPastDue(now)
                });
            }

            Filters = filters;

            SetPage(orderDtos);
        }
    }

    public class OrderIndexListItemViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = null!;

        public OrderStatus OrderStatus { get; set; }

        public DateTime DeliveryDate { get; set; }

        public DateTime OrderDate { get; internal set; }

        public decimal Total { get; internal set; }

        // ISSUE-25: Order.IsPastDue, so the list says the same thing the dashboard counts.
        public bool IsPastDue { get; internal set; }
    }
}