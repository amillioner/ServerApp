using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerApp.Controllers
{
    public class OrderValuesController : Controller
    {
        private DataContext context;
        public OrderValuesController(DataContext ctx)
        {
            context = ctx;
        }
        [HttpGet]
        public IEnumerable<Order> GetOrders()
        {
            return this.context.Orders
                .Include(o => o.Products)
                .Include(o => o.Payment);
        }
        [HttpPost]
        public void MarkShipped(long id)
        {
            var order = context.Orders.Find(id);
            if (order != null)
            {
                order.Shipped = true;
                context.SaveChanges();
            }
        }
        [HttpPost]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderId = 0;
                order.Shipped = false;
                order.Payment.Total = GetPrice(order.Products);
                ProcessPayment(order.Payment);
                if (order.Payment.AuthCode != null)
                {
                    context.Add(order);
                    context.SaveChanges();
                    return Ok(new
                    {
                        orderId = order.OrderId,
                        authCode = order.Payment.AuthCode,
                        amount = order.Payment.Total,
                    });
                }
                else
                {
                    return BadRequest("Payment rejected");
                }
            }
            return null;
        }

        private void ProcessPayment(Payment payment)
        {
            payment.AuthCode = "12345";
        }

        private decimal GetPrice(IEnumerable<CartLine> lines)
        {
            var ids = lines.Select(s => s.ProductId);
            var prods = context.Products.Where(x => ids.Contains(x.ProductId));
            return prods.Select(x =>
                    lines.First(f => f.ProductId == x.ProductId).Quantity * x.Price).Sum();
        }
    }
}