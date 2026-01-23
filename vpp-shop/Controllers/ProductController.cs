using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;

namespace vpp_shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public ProductController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // ===== TRANG SẢN PHẨM (DANH SÁCH + DANH MỤC + SORT + PHÂN TRANG) =====
        public async Task<IActionResult> Index(
            int? groupId,
            int? categoryId,
            string? sort,
            string? keyword,
            int page = 1
        )
        {
            // 1. Lấy tất cả CategoryGroup + Category con
            var groups = await _context.CategoryGroups
                .Include(g => g.Categories)
                .ToListAsync();

            // 2. Query sản phẩm
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.CategoryGroup)
                .AsQueryable();

            // 3. LỌC THEO DANH MỤC
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery
                    .Where(p => p.CategoryId == categoryId.Value);
            }
            else if (groupId.HasValue)
            {
                productsQuery = productsQuery
                    .Where(p => p.Category.CategoryGroupId == groupId.Value);
            }

            // 3.5. TÌM KIẾM
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productsQuery = productsQuery
                    .Where(p => p.Name.Contains(keyword));
            }

            // 4. SẮP XẾP
            if (!string.IsNullOrEmpty(sort))
            {
                productsQuery = sort switch
                {
                    "name_asc" => productsQuery.OrderBy(p => p.Name),
                    "name_desc" => productsQuery.OrderByDescending(p => p.Name),
                    "price_asc" => productsQuery.OrderBy(p => p.Price),
                    "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                    _ => productsQuery
                };
            }

            // ===== 🔥 PHÂN TRANG =====
            int pageSize = 15;

            int totalProduct = await productsQuery.CountAsync();
            int totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);

            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. TIÊU ĐỀ
            string title = "TẤT CẢ SẢN PHẨM";

            if (categoryId.HasValue)
            {
                title = groups
                    .SelectMany(g => g.Categories)
                    .FirstOrDefault(c => c.Id == categoryId.Value)?.Name
                    ?? title;
            }
            else if (groupId.HasValue)
            {
                var groupName = groups
                    .FirstOrDefault(g => g.Id == groupId.Value)?.Name;

                if (!string.IsNullOrEmpty(groupName))
                {
                    title = groupName.ToUpper() + " - VPP Twinkle";
                }
            }

            // 6. ĐỔ DỮ LIỆU SANG VIEW
            ViewBag.CategoryGroups = groups;
            ViewBag.ActiveGroupId = groupId;
            ViewBag.ActiveCategoryId = categoryId;
            ViewBag.CurrentTitle = title;
            ViewBag.Sort = sort;

            // 🔥 ViewBag phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPage = totalPage;

            return View(products);
        }

        // ===== TRANG CHI TIẾT SẢN PHẨM =====
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .ThenInclude(c => c.CategoryGroup)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.RelatedProducts = _context.Products
                .Where(p => p.Id != id)
                .OrderBy(x => Guid.NewGuid())
                .Take(5)
                .ToList();

            ViewBag.SameCategoryProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(5)
                .ToList();

            return View(product);
        }

        // ===== SEARCH SUGGEST =====
        [HttpGet]
        public IActionResult SearchSuggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var result = _context.Products
                .Where(p => p.Name.StartsWith(q))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    price = p.Price,
                    image = p.Image
                })
                .Take(5)
                .ToList();

            return Json(result);
        }
    }
}
