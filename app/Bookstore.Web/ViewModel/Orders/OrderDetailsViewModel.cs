using Bookstore.Domain;
using Bookstore.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.Web.ViewModel.Orders
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }

        public string OrderStatus { get; set; }

        public DateTime DeliveryDate { get; set; }

        public decimal Total { get; set; }

        public List<OrderDetailsItemViewModel> OrderItems { get; set; } = new List<OrderDetailsItemViewModel>();

        public OrderDetailsViewModel(Order order)
        {
            OrderId = order.Id;
            DeliveryDate = order.DeliveryDate;
            OrderStatus = order.OrderStatus.GetDescription();
            Total = order.Total.Amount;

            OrderItems = order.OrderItems.Select(x => new OrderDetailsItemViewModel
            {
                BookId = x.BookId,
                BookName = x.Book.Name,
                ImageUrl = x.Book.CoverImageUrl!,
                Price = x.Price.Amount,
                Quantity = x.Quantity.Value
            }).ToList();
        }
    }

    public class OrderDetailsItemViewModel
    {
        public int BookId { get; set; }

        public string ImageUrl { get; set; } = null!;

        public string BookName { get; set; } = null!;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal SubTotal => Price * Quantity;
    }
}