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

        // ======================
        // DANH SÁCH ĐƠN
        // ======================
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // ======================
        // CẬP NHẬT TRẠNG THÁI
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return Json(new { success = false });

            // ❌ Không cho sửa đơn đã kết thúc
            if (order.Status == "COMPLETED" || order.Status == "CANCELLED")
                return Json(new { success = false });

            // ======================
            // HUỶ ĐƠN (ADMIN)
            // ======================
            if (status == "CANCELLED")
            {
                // 1️⃣ Hoàn kho
                foreach (var item in order.OrderItems)
                {
                    if (item.Product.Stock != null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }

                // 2️⃣ Hoàn tiền ví
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

                // 3️⃣ VNPAY → ghi chú (sandbox)
                if (order.PaymentMethod == "VNPAY")
                {
                    order.Note = (order.Note ?? "") + " | Admin huỷ - cần hoàn tiền VNPAY";
                }

                order.Status = "CANCELLED";
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    status = order.Status
                });
            }

            // ======================
            // UPDATE TRẠNG THÁI BÌNH THƯỜNG
            // ======================
            order.Status = status;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                status = order.Status
            });
        }

        // ======================
        // XOÁ ĐƠN (XOÁ CỨNG)
        // ======================
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
