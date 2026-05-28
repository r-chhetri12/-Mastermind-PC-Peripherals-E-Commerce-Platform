using iTextSharp.text.pdf;
using MasterMind.Data;
using MasterMind.Models;
using MasterMind.StaticData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace MasterMind.Controllers
{
    [Authorize] // Ensure only logged-in users can access the cart
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;


        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper Method: Get the current user's ID
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier); // Returns the unique UserId from ClaimsPrincipal
        }

        // View the Cart
        public IActionResult Cart()
        {
            var userId = GetUserId(); // Retrieve the logged-in user's ID
            var cartItems = _context.ShoppingCarts
                                    .Where(c => c.UserId == userId)
                                    .Include(c => c.Product)
                                    .ToList();

            return View(cartItems); // Pass the cart items to the view
        }
        //add view
   

        // Add a Product to the Cart
        public IActionResult AddToCart(int productId)
        {
            var userId = GetUserId(); // Retrieve the logged-in user's ID
            var cartItem = _context.ShoppingCarts
                                   .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                // If the item already exists in the cart, increase its quantity
                cartItem.Quantity++;
                TempData["CartMessage"] = "Product quantity updated in your cart.";
            }
            else
            {
                var product = _context.Products.Find(productId);
                if (product == null) return NotFound();

                // Add new item to cart
                var newCartItem = new ShoppingCart
                {
                    UserId = userId,
                    ProductId = product.Id,
                    Quantity = 1
                };

                _context.ShoppingCarts.Add(newCartItem);
                TempData["CartMessage"] = "Product added to your cart!";
            }

            _context.SaveChanges(); // Save changes to the database
            return RedirectToAction("Cart");
        }

        // Remove a Product from the Cart
        public IActionResult RemoveFromCart(int id)
        {
            var cartItem = _context.ShoppingCarts.Find(id);
            if (cartItem != null)
            {
                _context.ShoppingCarts.Remove(cartItem);
                _context.SaveChanges(); // Save changes to the database
                TempData["CartMessage"] = "Product removed from your cart!";
            }
            else
            {
                TempData["CartMessage"] = "Product not found in cart.";
            }
            return RedirectToAction("Cart");
        }

        // Clear All Items in the Cart
        public IActionResult ClearCart()
        {
            var userId = GetUserId(); // Retrieve the logged-in user's ID
            var cartItems = _context.ShoppingCarts.Where(c => c.UserId == userId).ToList();

            _context.ShoppingCarts.RemoveRange(cartItems);
            _context.SaveChanges(); // Save changes to the database

            return RedirectToAction("Cart");
        }

        // Checkout Logic
        public IActionResult Checkout()
        {
            var userId = GetUserId();
            var userProfile = _context.UserProfiles.FirstOrDefault(u => u.Id == userId);

            if (userProfile == null)
            {
                userProfile = new UserProfile { Id = userId };
                _context.UserProfiles.Add(userProfile);
                _context.SaveChanges();


            }

            var cartItems = _context.ShoppingCarts
                                    .Where(c => c.UserId == userId)
                                    .Include(c => c.Product)
                                    .ToList();

            if (!cartItems.Any()) return RedirectToAction("Cart");

            ViewBag.CartItems = cartItems;
            ViewBag.TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity);

            // ✅ Razorpay Order Creation (only in Checkout)
            var client = new RazorpayClient("rzp_test_LXtmp0k538yHVA", "RNDzCmSKVGJjF9CYXRGnqzCz");
            var options = new Dictionary<string, object>
    {
        { "amount", ViewBag.TotalAmount * 100 }, // Amount in paise
        { "currency", "INR" },
        { "receipt", "txn_" + Guid.NewGuid().ToString() }
    };

            var razorpayOrder = client.Order.Create(options);
            string razorpayOrderId = razorpayOrder["id"].ToString(); // ✅ Store order_id

            // ✅ Store order details temporarily (not yet confirmed)
            var order = new Models.Order
            {
                ApplicationUserId = userId,
                Name = userProfile.Name,
                PhoneNumber = userProfile.PhoneNumber,
                Address = userProfile.Address,
                City = userProfile.City,
                State = userProfile.State,
                Pincode = userProfile.Pincode,
                TotalAmount = ViewBag.TotalAmount,
                OrderDate = DateTime.Now,
                OrderId = razorpayOrderId
                  
            };

            _context.Orders.Add(order);
            _context.SaveChanges();
            // ✅ Add each item in the cart to OrderItems
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id, // ✅ Link with Order table
                    ProductId = cartItem.Product.Id,

                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                };
                _context.OrderItems.Add(orderItem);

                var product = _context.Products.FirstOrDefault(p => p.Id == cartItem.Product.Id);
                if (product != null && product.Quantity >= cartItem.Quantity)
                {
                    product.Quantity -= cartItem.Quantity;
                    _context.Products.Update(product);
                }
            }

            _context.SaveChanges();

            ViewBag.OrderId = razorpayOrderId; // ✅ Pass order_id to the view

            return View(userProfile);
        }

       


        // Place an Order
        [HttpPost]
        public IActionResult PlaceOrder(string Name, string PhoneNumber, string Address, string City, string State, string Pincode)
        {
            var userId = GetUserId();
            var userProfile = _context.UserProfiles.FirstOrDefault(u => u.Id == userId);

            if (userProfile == null) return RedirectToAction("Cart");

            // Update user profile details
            userProfile.Name = Name;
            userProfile.PhoneNumber = PhoneNumber;
            userProfile.Address = Address;
            userProfile.City = City;
            userProfile.State = State;
            

            _context.UserProfiles.Update(userProfile);
            _context.SaveChanges();

            var cartItems = _context.ShoppingCarts
                                    .Where(c => c.UserId == userId)
                                    .Include(c => c.Product)
                                    .ToList();

            if (!cartItems.Any()) return RedirectToAction("Cart");
            var order = new Models.Order
            {
                ApplicationUserId = userId,
                Name = userProfile.Name,
                PhoneNumber = userProfile.PhoneNumber,
                Address = userProfile.Address,
                City = userProfile.City,
                State = userProfile.State,
                Pincode = userProfile.Pincode,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                OrderDate = DateTime.Now,
               
            };

            _context.Orders.Add(order);
            _context.SaveChanges(); // ✅ Save the order to get OrderId

            // ✅ Add each item in the cart to OrderItems
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id, // ✅ Link with Order table
                    ProductId = cartItem.Product.Id,
                    
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                };
                _context.OrderItems.Add(orderItem);
            }

            _context.SaveChanges(); // ✅ Save order items
                                    //SendOrderConfirmationEmail(userProfile.Email, userProfile.Name, order);
            string email = userProfile.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                email = User.Identity.Name; // fallback if needed
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("📩 Sending confirmation to: " + email);
                SendOrderConfirmationEmail(email, userProfile.Name, order);
            }
            else
            {
                Console.WriteLine("❌ Cannot send email, no valid email address found.");
            }

            // ✅ The order was already created in Checkout, just return
            return RedirectToAction("Index", "Home");
        }

      


        // Display Order Success Page
        public IActionResult OrderSuccess()
        {

            return View();
        }

        [HttpPost]
        public IActionResult CompletePayment(string paymentId, string orderId, string signature)
        {
            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(signature))
            {
                return BadRequest("Invalid payment details received.");
            }

            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return BadRequest("Order not found.");

            // ✅ Ensure Payment Verification
            var attributes = new Dictionary<string, string>
    {
        { "razorpay_payment_id", paymentId },
        { "razorpay_order_id", orderId },
        { "razorpay_signature", signature }
    };

            try
            {
                Utils.verifyPaymentSignature(attributes); // ✅ Verify payment signature

                // ✅ Update order with payment ID and mark as paid
                order.TransactionId = paymentId;
                order.OrderId = orderId;
                _context.SaveChanges();

                // ✅ Clear user's shopping cart after payment
                var cartItems = _context.ShoppingCarts.Where(c => c.UserId == order.ApplicationUserId).ToList();
                _context.ShoppingCarts.RemoveRange(cartItems);
                _context.SaveChanges();

                return RedirectToAction(nameof(OrderSuccess));
            }
            catch (Exception ex)
            {
                return BadRequest("Payment verification failed: " + ex.Message);
            }
        }

        public IActionResult MyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var orders = _context.Orders
                .Where(o => o.ApplicationUserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
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
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int orderId, string reason)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.OrderStatus == "Pending");

            if (order == null)
            {
                return NotFound();
            }

            order.IsCancelled = true;
            order.CancellationReason = reason;
            order.OrderStatus = "Cancelled";

            // Refund using Razorpay
            var payment = _context.Orders.FirstOrDefault(p => p.OrderId == order.OrderId);
            if (payment != null)
            {
                var client = new Razorpay.Api.RazorpayClient("rzp_test_LXtmp0k538yHVA", "RNDzCmSKVGJjF9CYXRGnqzCz");
                var refundRequest = new Dictionary<string, object>
        {
            { "amount", (int)(order.TotalAmount * 100) } // Razorpay uses paise
        };
                Razorpay.Api.Refund refund = client.Payment.Fetch(payment.TransactionId).Refund(refundRequest);
            }

            _context.SaveChanges();
            return RedirectToAction("MyOrders");
        }
        public IActionResult Invoice(int orderId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ApplicationUser)
                .FirstOrDefault(o => o.Id == orderId && o.ApplicationUserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int id)
        {
            var cartItem = _context.ShoppingCarts.FirstOrDefault(i => i.Id == id);
            if (cartItem != null)
            {
                cartItem.Quantity++;
                _context.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int id)
        {
            var cartItem = _context.ShoppingCarts.FirstOrDefault(i => i.Id == id);
            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                _context.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        public ActionResult TestEmail()
        {
            SendOrderConfirmationEmail("mastermindd1210@gmail.com", "Test User", new Models.Order
            {
                Id = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 999
            });

            return Content("Test email attempted.");
        }

        private void SendOrderConfirmationEmail(string toEmail, string customerName, Models.Order order)
        {
            try
            {
                string fromEmail = "mastermindd1210@gmail.com"; // ✅ Admin email
                string fromAppPassword = "ucwzkjlpshyoeyde";  // ✅ App password from Gmail

                string subject = $"Order Confirmation - Order #{order.Id}";
                string body = $@"
            <h3>Hello {customerName},</h3>
            <p>Thank you for your order at Your Store!</p>
            <p><strong>Order ID:</strong> {order.Id}<br/>
            <strong>Order Date:</strong> {order.OrderDate}<br/>
            <strong>Total Amount:</strong> ₹{order.TotalAmount}</p>
            <p>We’ll notify you when your order ships.</p>
            <br/>
            <p>Regards,<br/>Your Store Admin</p>
        ";

                MailMessage mail = new MailMessage(fromEmail, toEmail, subject, body);
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, fromAppPassword)
                };

                smtp.Send(mail);
                Console.WriteLine("✅ Email sent from Admin to Customer: " + toEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Email sending failed: " + ex.Message);
            }
        }
    }

    }







//fcoewxgskobeizsc
//    chhetrirudra70@gmail.com