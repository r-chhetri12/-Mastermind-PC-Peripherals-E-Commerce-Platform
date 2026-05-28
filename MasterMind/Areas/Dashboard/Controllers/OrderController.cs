using MasterMind.Data;
using MasterMind.StaticData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class OrderController : Controller
    {

        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = SD.Role_Admin)] // Ensure only admins can access this
        public IActionResult AdminOrders()
        {
            var orders = _context.Orders
                .Include(o => o.ApplicationUser) // Include user details if needed
                .OrderByDescending(o => o.OrderDate) // Show latest orders first
                .ToList();

            return View(orders);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Include product details
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }




        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult ShipOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Shipped"; // ✅ Update order status
            _context.SaveChanges();

            return RedirectToAction("AdminOrders"); // ✅ Redirect back to the order list
        }
        [HttpGet]
        public IActionResult GetDailySales()
        {
            var dailySales = _context.Orders
         .Where(o => o.OrderStatus == "Shipped" || o.OrderStatus == "Out for Delivery")
         .AsEnumerable() // Forces LINQ to switch from SQL to in-memory (C#)
         .GroupBy(o => o.OrderDate.Date)
         .Select(g => new
         {
             Date = g.Key.ToString("yyyy-MM-dd"), // now valid since it's in memory
             TotalSales = g.Sum(x => x.TotalAmount)
         })
         .OrderBy(x => x.Date)
         .ToList();


            return Json(dailySales);
        }
        [HttpGet]
        public IActionResult GetDailyAndMonthlyOrderCount()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var dailyCount = _context.Orders.Count(o => o.OrderDate.Date == today && o.OrderStatus == "Shipped" || o.OrderStatus == "Out for Delivery");
            var monthlyCount = _context.Orders.Count(o => o.OrderDate.Date >= firstDayOfMonth && o.OrderStatus == "Shipped" || o.OrderStatus == "Out for Delivery");

            return Json(new { dailyOrders = dailyCount, monthlyOrders = monthlyCount });
        }
        // In Controllers/OrderController.cs
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult MarkOutForDelivery(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                return NotFound();

            order.OrderStatus = "Out for Delivery";

            order.TrackingId = "TRK" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

            // ✅ Set estimated delivery date
            order.EstimatedDeliveryDate = DateTime.Now.AddDays(3);

            _context.SaveChanges();

            TempData["success"] = $"Order #{order.Id} marked as Out for Delivery.";
            return RedirectToAction("AdminOrders"); // or your actual view

            // or wherever your list is
        }





    }
}
