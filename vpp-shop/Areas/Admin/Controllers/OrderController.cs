using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public OrderController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

            return View(orders);
        }
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return Json(new { success = false });

            if (order.Status == "COMPLETED" || order.Status == "CANCELLED")
                return Json(new { success = false });

            order.Status = status;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                status = order.Status
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var items = _context.OrderItems.Where(x => x.OrderId == id);
            _context.OrderItems.RemoveRange(items);

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return Json(new { success = false });

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }

}
