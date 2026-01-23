using Microsoft.AspNetCore.Mvc;
using vpp_shop.Data;
using vpp_shop.Models.ViewModels;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public HomeController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("ADMIN_ID") == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var model = new AdminDashboardViewModel();

            // =========================
            // THỐNG KÊ SỐ LƯỢNG
            // =========================
            model.TotalUsers = _context.Users.Count();
            model.TotalAdmins = _context.Admins.Count();
            model.TotalOrders = _context.Orders.Count();

            model.PendingOrders = _context.Orders.Count(o => o.Status == "PENDING");
            model.PaidOrders = _context.Orders.Count(o => o.Status == "PAID");
            model.ShippingOrders = _context.Orders.Count(o => o.Status == "SHIPPING");
            model.CompletedOrders = _context.Orders.Count(o => o.Status == "COMPLETED");
            model.CancelledOrders = _context.Orders.Count(o => o.Status == "CANCELLED");

            // =========================
            // DOANH THU
            // =========================
            model.RevenueCompleted = _context.Orders
                .Where(o => o.Status == "COMPLETED")
                .Sum(o => (decimal?)o.TotalMoney) ?? 0;

            model.RevenueProcessing = _context.Orders
                .Where(o => o.Status != "CANCELLED" && o.Status != "COMPLETED")
                .Sum(o => (decimal?)o.TotalMoney) ?? 0;

            var now = DateTime.Now;

            // =========================
            // 📊 DOANH THU THEO NGÀY (TRONG THÁNG HIỆN TẠI)
            // =========================
            // =========================
            // 📊 DOANH THU THEO NGÀY (THÁNG HIỆN TẠI)
            // =========================
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var dailyData = _context.Orders
                .Where(o => o.Status == "COMPLETED"
                    && o.CreatedAt.HasValue
                    && o.CreatedAt >= startOfMonth
                    && o.CreatedAt < startOfNextMonth)
                .GroupBy(o => o.CreatedAt!.Value.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    Revenue = g.Sum(x => x.TotalMoney)
                })
                .ToList();

            // 👉 ĐỔ ĐẦY TẤT CẢ CÁC NGÀY TRONG THÁNG
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

            model.DailyLabels = new List<string>();
            model.DailyRevenue = new List<decimal>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var dataOfDay = dailyData.FirstOrDefault(x => x.Day == day);

                model.DailyLabels.Add($"Ngày {day}");
                model.DailyRevenue.Add(dataOfDay?.Revenue ?? 0);
            }


            // =========================
            // 📊 DOANH THU THEO THÁNG (TRONG NĂM)
            // =========================
            var startOfYear = new DateTime(now.Year, 1, 1);
            var startOfNextYear = startOfYear.AddYears(1);

            var monthlyData = _context.Orders
                .Where(o => o.Status == "COMPLETED"
                    && o.CreatedAt.HasValue
                    && o.CreatedAt >= startOfYear
                    && o.CreatedAt < startOfNextYear)
                .GroupBy(o => o.CreatedAt!.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.TotalMoney)
                })
                .ToList();

            // 👉 ĐỔ ĐẦY ĐỦ 12 THÁNG
            model.MonthlyLabels = new List<string>();
            model.MonthlyRevenue = new List<decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var dataOfMonth = monthlyData.FirstOrDefault(x => x.Month == month);

                model.MonthlyLabels.Add($"Tháng {month}");
                model.MonthlyRevenue.Add(dataOfMonth?.Revenue ?? 0);
            }


            return View(model);
        }

    }
}
