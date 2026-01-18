using Microsoft.AspNetCore.Mvc;
using vpp_shop.Data;

namespace vpp_shop.Controllers
{
    [ApiController]
    [Route("Voucher")]
    public class VoucherController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public VoucherController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        [HttpPost("Apply")]
        public IActionResult Apply([FromBody] VoucherRequest req)
        {
            var voucher = _context.Vouchers.FirstOrDefault(v =>
                v.Code == req.Code &&
                v.StartDate <= DateTime.Now &&
                v.EndDate >= DateTime.Now &&
                (v.UsageLimit == null || v.UsedCount < v.UsageLimit)
            );

            if (voucher == null)
                return Json(new { success = false, message = "Mã không hợp lệ hoặc đã hết hạn" });

            if (voucher.MinOrderValue != null && req.Total < voucher.MinOrderValue)
                return Json(new
                {
                    success = false,
                    message = $"Đơn tối thiểu {voucher.MinOrderValue:N0} đ"
                });

            decimal discount = 0;

            if (voucher.DiscountType == "PERCENT")
                discount = req.Total * voucher.DiscountValue / 100;
            else
                discount = voucher.DiscountValue;

            if (discount > req.Total)
                discount = req.Total;

            return Json(new
            {
                success = true,
                discount,
                newTotal = req.Total - discount,
                code = voucher.Code
            });
        }
    }

    public class VoucherRequest
    {
        public string Code { get; set; }
        public decimal Total { get; set; }
    }
}
