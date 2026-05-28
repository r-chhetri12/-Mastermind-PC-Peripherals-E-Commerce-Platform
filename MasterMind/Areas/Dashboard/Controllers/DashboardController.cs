using Microsoft.AspNetCore.Mvc;
using MasterMind.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MasterMind.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get total revenue from shipped orders
            decimal totalRevenue = _context.Orders
                .Where(o => o.OrderStatus == "Shipped" )
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // Get total quantity of products sold
            int totalProductsSold = _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderStatus == "Shipped" || oi.Order.OrderStatus == "Out for Delivery")
                .Sum(oi => (int?)oi.Quantity) ?? 0;

            // Get latest 5 ordered products
            var latestOrderedProducts = _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderStatus == "Shipped" || oi.Order.OrderStatus == "Out for Delivery")
                .OrderByDescending(oi => oi.Order.OrderDate)
                .Select(oi => new
                {
                    OrderId = oi.Order.Id,
                    ProductName = oi.Product.Name,
                    OrderDate = oi.Order.OrderDate,
                    Price = oi.Price
                })
                .Take(10)
                .ToList();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalProductsSold = totalProductsSold;
            ViewBag.LatestOrderedProducts = latestOrderedProducts;

            return View();
        }
    }
}
