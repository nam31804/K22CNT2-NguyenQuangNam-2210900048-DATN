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

    // ======================
    // LỊCH SỬ ĐƠN HÀNG
    // ======================
    public async Task<IActionResult> History()
    {
        int? userId = HttpContext.Session.GetInt32("USER_ID");

        if (userId == null)
            return RedirectToAction("Login", "Auth");

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // ======================
    // HUỶ ĐƠN HÀNG
    // ======================
    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        int? userId = HttpContext.Session.GetInt32("USER_ID");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
            return NotFound();

        // ❌ KHÔNG CHO HUỶ NẾU ĐÃ GIAO
        if (order.Status == "DELIVERED")
        {
            TempData["Error"] = "Đơn hàng đã giao, không thể huỷ";
            return RedirectToAction("History");
        }

        // ❌ KHÔNG HUỶ LẠI ĐƠN ĐÃ HUỶ
        if (order.Status == "CANCELLED")
        {
            TempData["Error"] = "Đơn hàng đã được huỷ trước đó";
            return RedirectToAction("History");
        }

        // ======================
        // HOÀN LẠI TỒN KHO
        // ======================
        foreach (var item in order.OrderItems)
        {
            if (item.Product.Stock != null)
            {
                item.Product.Stock += item.Quantity;
            }
        }

        // ======================
        // HOÀN TIỀN
        // ======================
        if (order.PaymentMethod == "WALLET")
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == order.UserId);

            if (wallet != null)
            {
                wallet.Balance += order.TotalMoney;
                wallet.UpdatedAt = DateTime.Now;
            }
        }

        // VNPAY (sandbox / giả lập hoàn tiền)
        if (order.PaymentMethod == "VNPAY")
        {
            // 👉 Thực tế sẽ gọi API refund
            // 👉 Đồ án: chỉ cần ghi chú + đổi trạng thái
            order.Note = (order.Note ?? "") + " | Đã hoàn tiền VNPAY (sandbox)";
        }

        // ======================
        // CẬP NHẬT TRẠNG THÁI
        // ======================
        order.Status = "CANCELLED";

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã huỷ đơn hàng, hoàn tiền và hoàn lại tồn kho";
        return RedirectToAction("History");
    }
}
