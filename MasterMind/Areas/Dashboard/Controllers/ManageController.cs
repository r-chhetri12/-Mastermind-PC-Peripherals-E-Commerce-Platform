using Microsoft.AspNetCore.Mvc;

namespace MasterMind.Areas.Dashboard.Controllers
{
    public class ManageController : Controller
    {
        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult DeleteAccount()
        {
            return View();
        }

        public IActionResult UpdateProfile()
        {
            return View();
        }

        public IActionResult SecuritySettings()
        {
            return View();
        }

    }
}
