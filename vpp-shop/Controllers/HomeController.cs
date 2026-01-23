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

            // ===== SẢN PHẨM NỔI BẬT (RANDOM 10) =====
            var products = await _context.Products
                .OrderBy(x => Guid.NewGuid())
                .Take(10)
                .ToListAsync();

            return View(products);
        }
    }
}
