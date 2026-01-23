using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using vpp_shop.Data;

public class PromotionController : Controller
{
    private readonly VanPhongPhamDBContext _context;

    public PromotionController(VanPhongPhamDBContext context)
    {
        _context = context;
    }

    // TRANG ĐỌC BÀI QUẢNG CÁO
    public IActionResult Detail(int id)
    {
        var promo = _context.ProductPromotions
            .Include(p => p.Product)
            .FirstOrDefault(p => p.Id == id && p.IsActive);

        if (promo == null) return NotFound();

        return View(promo);
    }
}
