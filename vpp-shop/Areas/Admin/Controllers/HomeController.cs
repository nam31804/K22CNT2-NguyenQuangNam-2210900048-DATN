using Microsoft.AspNetCore.Mvc;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("ADMIN_ID") == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            return View();
        }
    }
}
