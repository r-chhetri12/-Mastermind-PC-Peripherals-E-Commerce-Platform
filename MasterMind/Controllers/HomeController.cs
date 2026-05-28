using System;
using System.Diagnostics;
using System.Linq;
using MasterMind.Data;
using MasterMind.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace MasterMind.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;

            _context = context;
        }

        public IActionResult Index()
        {
           
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult Products()
        {
            var products = _context.Products.ToList();// Fetch all products

            IEnumerable<Category> categoryList = _context.Categories.ToList();

            ViewBag.Categories = categoryList;
            return View(products);

        }

        [HttpGet]
        public IActionResult FilterProducts(List<int> categoryIds, string sortOption)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryIds != null && categoryIds.Any())
            {
                products = products.Where(p => categoryIds.Contains(p.CategoryId));
            }

            switch (sortOption)
            {
                case "price-asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price-desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            return PartialView("_ProductGrid", products.ToList());
        }


        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        //
     



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

//string key = "rzp_test_LXtmp0k538yHVA";
//            string secret = "RNDzCmSKVGJjF9CYXRGnqzCz";
//@inject SignInManager<IdentityUser> SignInManager
//@{
//    var hasExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).Any();
//}
//< ul class= "nav nav-pills flex-column" >
//    < li class= "nav-item" >< a class= "nav-link @ManageNavPages.IndexNavClass(ViewContext)" id = "profile" asp - page = "./Index" > Profile </ a ></ li >
//    < li class= "nav-item" >< a class= "nav-link @ManageNavPages.EmailNavClass(ViewContext)" id = "email" asp - page = "./Email" > Email </ a ></ li >
//    < li class= "nav-item" >< a class= "nav-link @ManageNavPages.ChangePasswordNavClass(ViewContext)" id = "change-password" asp - page = "./ChangePassword" > Password </ a ></ li >
//    @if(hasExternalLogins)
//    {
//        < li id = "external-logins" class= "nav-item" >< a id = "external-login" class= "nav-link @ManageNavPages.ExternalLoginsNavClass(ViewContext)" asp - page = "./ExternalLogins" > External logins </ a ></ li >
//    }
//    < li class= "nav-item" >< a class= "nav-link @ManageNavPages.TwoFactorAuthenticationNavClass(ViewContext)" id = "two-factor" asp - page = "./TwoFactorAuthentication" > Two - factor authentication </ a ></ li >
//    < li class= "nav-item" >< a class= "nav-link @ManageNavPages.PersonalDataNavClass(ViewContext)" id = "personal-data" asp - page = "./PersonalData" > Personal data </ a ></ li >
//</ ul >


//@page
//@model ChangePasswordModel
//@{
//    ViewData["Title"] = "Change password";
//    ViewData["ActivePage"] = ManageNavPages.ChangePassword;
//}
//@{
//    Layout = "Areas/Identity/Pages/Account/Manage/ChangePassword.cshtml";
//}
//<h3>@ViewData["Title"]</h3>
//<partial name="_StatusMessage" for="StatusMessage" />
//<div class="row">
//    <div class="col-md-6">
//        <form id="change-password-form" method="post">
//            <div asp-validation-summary="All" class="text-danger"></div>
//            <div class="form-group">
//                <label asp-for="Input.OldPassword"></label>
//                <input asp-for="Input.OldPassword" class="form-control" autocomplete="current-password" aria-required="true" />
//                <span asp-validation-for="Input.OldPassword" class="text-danger"></span>
//            </div>
//            <div class="form-group">
//                <label asp-for="Input.NewPassword"></label>
//                <input asp-for="Input.NewPassword" class="form-control" autocomplete="new-password" aria-required="true" />
//                <span asp-validation-for="Input.NewPassword" class="text-danger"></span>
//            </div>
//            <div class="form-group">
//                <label asp-for="Input.ConfirmPassword"></label>
//                <input asp-for="Input.ConfirmPassword" class="form-control" autocomplete="new-password" aria-required="true" />
//                <span asp-validation-for="Input.ConfirmPassword" class="text-danger"></span>
//            </div>
//            <button type="submit" class="btn btn-primary">Update password</button>
//        </form>
//    </div>
//</div>

//@section Scripts {
//    <partial name="_ValidationScriptsPartial" />
//}@page
//@model ChangePasswordModel
//@{
//    ViewData["Title"] = "Change password";
//    ViewData["ActivePage"] = ManageNavPages.ChangePassword;
//}
//@{
//    Layout = "Areas/Identity/Pages/Account/Manage/ChangePassword.cshtml";
//}
//<h3>@ViewData["Title"]</h3>
//<partial name="_StatusMessage" for="StatusMessage" />
//<div class="row">
//    <div class="col-md-6">
//        <form id="change-password-form" method="post">
//            <div asp-validation-summary="All" class="text-danger"></div>
//            <div class="form-group">
//                <label asp-for="Input.OldPassword"></label>
//                <input asp-for="Input.OldPassword" class="form-control" autocomplete="current-password" aria-required="true" />
//                <span asp-validation-for="Input.OldPassword" class="text-danger"></span>
//            </div>
//            <div class="form-group">
//                <label asp-for="Input.NewPassword"></label>
//                <input asp-for="Input.NewPassword" class="form-control" autocomplete="new-password" aria-required="true" />
//                <span asp-validation-for="Input.NewPassword" class="text-danger"></span>
//            </div>
//            <div class="form-group">
//                <label asp-for="Input.ConfirmPassword"></label>
//                <input asp-for="Input.ConfirmPassword" class="form-control" autocomplete="new-password" aria-required="true" />
//                <span asp-validation-for="Input.ConfirmPassword" class="text-danger"></span>
//            </div>
//            <button type="submit" class="btn btn-primary">Update password</button>
//        </form>
//    </div>
//</div>

//@section Scripts {
//    <partial name="_ValidationScriptsPartial" />
//}