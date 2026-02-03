using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;

namespace vpp_shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public HomeController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ===== BÀI VIẾT / KHUYẾN MÃI =====
            ViewBag.Promotions = await _context.ProductPromotions
                .Include(x => x.Product)
                .Where(x => x.IsActive)
                .OrderBy(x => x.Position)
                .Take(4)
                .ToListAsync();
            // ===== 5 SẢN PHẨM BÁN CHẠY =====
        var bestSellers = (
            from p in _context.Products
            join oi in _context.OrderItems on p.Id equals oi.ProductId into poi
            from oi in poi.DefaultIfEmpty()
            join o in _context.Orders
                .Where(x => x.Status == "COMPLETED" || x.Status == "PAID")
                on oi.OrderId equals o.Id into ooi
            from o in ooi.DefaultIfEmpty()
            group oi by p into g
            select new
            {
                Product = g.Key,
                TotalSold = g.Sum(x => x == null ? 0 : x.Quantity)
            }
        )
        .OrderByDescending(x => x.TotalSold)
        .Take(5)
        .Select(x => x.Product)
        .ToList();

        ViewBag.BestSellers = bestSellers;
            // ===== SẢN PHẨM NỔI BẬT (RANDOM 10) =====
            var products = await _context.Products
            .Where(x => x.Stock > 0) // ✅ LOẠI HẾT HÀNG
            .OrderBy(x => Guid.NewGuid())
            .Take(10)
            .ToListAsync();

            return View(products);

        }
    }
}
