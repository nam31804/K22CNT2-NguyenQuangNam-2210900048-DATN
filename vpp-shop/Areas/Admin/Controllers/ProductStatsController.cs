using Microsoft.AspNetCore.Mvc;
using vpp_shop.Data;
using vpp_shop.Models.ViewModels;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductStatsController : AdminBaseController
    {
        private readonly VanPhongPhamDBContext _context;

        public ProductStatsController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = (
                from p in _context.Products
                join oi in _context.OrderItems on p.Id equals oi.ProductId into poi
                from oi in poi.DefaultIfEmpty()
                join o in _context.Orders
                    // ✅ CHỈ TÍNH ĐƠN ĐÃ THANH TOÁN HOẶC HOÀN THÀNH
                    .Where(x => x.Status == "COMPLETED" || x.Status == "PAID")
                    on oi.OrderId equals o.Id into ooi
                from o in ooi.DefaultIfEmpty()
                group oi by new { p.Id, p.Name, p.Image } into g
                select new ProductRankingVM
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,

                    // chuẩn hoá đường dẫn ảnh
                    Image = string.IsNullOrEmpty(g.Key.Image)
                        ? "/images/no-image.png"
                        : "/images/products/" + g.Key.Image,

                    TotalSold = g.Sum(x => x == null ? 0 : x.Quantity)
                }
            )
            .OrderByDescending(x => x.TotalSold)
            .ToList();

            var model = new DashboardProductVM
            {
                Top3 = products.Take(3).ToList(),
                Top4To10 = products.Skip(3).Take(7).ToList(),

                LowOrNoSales = products
                    .Where(x => x.TotalSold < 3)
                    .Select(x => x.Name)
                    .ToList(),

                BanChay = products.Count(x => x.TotalSold >= 5),
                BanIt = products.Count(x => x.TotalSold >= 1 && x.TotalSold < 5),
                KhongBan = products.Count(x => x.TotalSold == 0)
            };

            return View(model);
        }
    }
}
