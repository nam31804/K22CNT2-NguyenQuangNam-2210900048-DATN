using Microsoft.AspNetCore.Mvc;
using vpp_shop.Data;
using vpp_shop.Models;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VoucherController : AdminBaseController
    {
        private readonly VanPhongPhamDBContext _context;

        public VoucherController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // =========================
        // 1. DANH SÁCH VOUCHER
        // =========================
        public IActionResult Index()
        {
            var vouchers = _context.Vouchers
                .OrderByDescending(v => v.CreatedAt)
                .ToList();

            return View(vouchers);
        }

        // =========================
        // 2. THÊM VOUCHER
        // =========================
        [HttpPost]
        public IActionResult Create(Voucher model)
        {
            // check trùng mã
            bool exists = _context.Vouchers.Any(v => v.Code == model.Code);
            if (exists)
            {
                TempData["error"] = "Mã voucher đã tồn tại";
                return RedirectToAction("Index");
            }

            model.CreatedAt = DateTime.Now;
            model.UsedCount = 0;
            model.IsActive = true;

            _context.Vouchers.Add(model);
            _context.SaveChanges();

            TempData["success"] = "Thêm voucher thành công";
            return RedirectToAction("Index");
        }

        // =========================
        // 3. SỬA VOUCHER
        // =========================
        [HttpPost]
        public IActionResult Update(Voucher model)
        {
            var voucher = _context.Vouchers.Find(model.Id);
            if (voucher == null) return NotFound();

            voucher.Code = model.Code;
            voucher.DiscountType = model.DiscountType;
            voucher.DiscountValue = model.DiscountValue;
            voucher.MinOrderValue = model.MinOrderValue;
            voucher.StartDate = model.StartDate;
            voucher.EndDate = model.EndDate;
            voucher.UsageLimit = model.UsageLimit;

            _context.SaveChanges();

            TempData["success"] = "Cập nhật voucher thành công";
            return RedirectToAction("Index");
        }

        // =========================
        // 4. KHOÁ / MỞ VOUCHER
        // =========================
        public IActionResult Toggle(int id)
        {
            var voucher = _context.Vouchers.Find(id);
            if (voucher == null) return NotFound();

            voucher.IsActive = !voucher.IsActive;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // =========================
        // 5. XOÁ VOUCHER
        // =========================
        public IActionResult Delete(int id)
        {
            var voucher = _context.Vouchers.Find(id);
            if (voucher == null) return NotFound();

            _context.Vouchers.Remove(voucher);
            _context.SaveChanges();

            TempData["success"] = "Đã xoá voucher";
            return RedirectToAction("Index");
        }
    }
}
