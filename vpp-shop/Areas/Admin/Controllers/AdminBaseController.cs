using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminBaseController : Controller
    {
        protected bool IsAdmin =>
            HttpContext.Session.GetInt32("ADMIN_ID") != null;

        protected bool IsStaff =>
            HttpContext.Session.GetInt32("STAFF_ID") != null;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // ❌ Chưa login (cả admin lẫn staff)
            if (!IsAdmin && !IsStaff)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Auth",
                    new { area = "" }
                );
                return;
            }

            // ✅ Admin hoặc Staff đều được vào Admin Area
            base.OnActionExecuting(context);
        }
    }
}
