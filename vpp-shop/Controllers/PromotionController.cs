using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;

public class PromotionController : Controller
{
    private readonly VanPhongPhamDBContext _context;

    public PromotionController(VanPhongPhamDBContext context)
    {
        _context = context;
    }

    public IActionResult Index(int page = 1)
    {
        int pageSize = 5; // 🔥 chỉ 5 khuyến mãi mỗi trang

        var query = _context.ProductPromotions
            .Include(p => p.Product)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt);

        ViewBag.RelatedPromotions = _context.ProductPromotions
            .Include(p => p.Product)
            .Where(p => p.IsActive)
            .OrderBy(x => Guid.NewGuid())
            .Take(5)
            .ToList();

        ViewBag.SuggestProducts = _context.Products
            .OrderBy(x => Guid.NewGuid())
            .Take(6)
            .ToList();
        int totalItems = query.Count();
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        ViewBag.CurrentPage = page;
        var promotions = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return View(promotions);
    }


    // 🔹 ĐỌC BÀI VIẾT
    // 🔹 ĐỌC BÀI VIẾT
    public IActionResult Detail(int id)
    {
        var promo = _context.ProductPromotions
            .Include(p => p.Product)
            .FirstOrDefault(p => p.Id == id && p.IsActive);

        if (promo == null)
            return NotFound();

        /* ===== CỘT TRÁI: DANH MỤC (GIỐNG PRODUCT) ===== */
        var groups = _context.CategoryGroups
            .Include(g => g.Categories)
            .ToList();

        ViewBag.CategoryGroups = groups;
        ViewBag.ActiveGroupId = null;
        ViewBag.ActiveCategoryId = promo.Product.CategoryId;

        /* ===== CỘT PHẢI: CÓ THỂ BẠN THÍCH ===== */
        ViewBag.RelatedProducts = _context.Products
            .Where(p => p.Id != promo.Product.Id)
            .OrderBy(x => Guid.NewGuid())
            .Take(5)
            .ToList();

        /* ===== DƯỚI ĐÁY: SẢN PHẨM CÙNG LOẠI ===== */
        ViewBag.SameCategoryProducts = _context.Products
            .Where(p => p.CategoryId == promo.Product.CategoryId
                        && p.Id != promo.Product.Id)
            .Take(5)
            .ToList();

        return View(promo);
    }



}
