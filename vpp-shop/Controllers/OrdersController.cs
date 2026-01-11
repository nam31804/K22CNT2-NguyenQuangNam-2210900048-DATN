using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;

public class OrdersController : Controller
{
    private readonly VanPhongPhamDBContext _context;

    public OrdersController(VanPhongPhamDBContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> History()
    {
        int? userId = HttpContext.Session.GetInt32("USER_ID");

        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var orders = await _context.Orders
    .Where(o => o.UserId == userId)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
    .OrderByDescending(o => o.CreatedAt)
    .ToListAsync();
        return View(orders);
    }
}
