using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using vpp_shop.Data;

namespace vpp_shop.Controllers
{
    public class PaymentController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        // ===== VNPAY SANDBOX CONFIG =====
        private const string VNP_TMN_CODE = "1P0DTBJ8";
        private const string VNP_HASH_SECRET = "BDR8JMEAJ984VY94DIVX5WSN1V5LHXQ8";
        private const string VNP_URL = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        // ⚠️ ĐỔI LINK NÀY NẾU RESTART NGROK
        private const string RETURN_URL =
            "https://bb002d6c03c0.ngrok-free.app/payment/vnpay-return";

        public PaymentController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // ==================================================
        // 1️⃣ TẠO LINK THANH TOÁN VNPAY
        // ==================================================
        public IActionResult CreateVnpayPayment(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                return Content("ORDER_NOT_FOUND");

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", VNP_TMN_CODE },
                { "vnp_Amount", ((long)(order.TotalMoney * 100)).ToString() }, // x100
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", order.Id.ToString() },
                { "vnp_OrderInfo", $"Thanh toan don hang {order.Id}" }, // KHÔNG DẤU
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },
                { "vnp_IpAddr", ip },
                { "vnp_ReturnUrl", RETURN_URL },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            // ===== 1️⃣ CHUỖI KÝ (RAW – KHÔNG URL ENCODE) =====
            string hashData = string.Join("&",
                vnpParams.Select(x => $"{x.Key}={x.Value}")
            );

            string secureHash = HmacSHA512(VNP_HASH_SECRET, hashData);

            // ===== 2️⃣ QUERY STRING (URL ENCODE RFC3986) =====
            string query = string.Join("&",
                vnpParams.Select(x =>
                    $"{UrlEncodeRFC3986(x.Key)}={UrlEncodeRFC3986(x.Value)}")
            );

            query += $"&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";

            return Redirect($"{VNP_URL}?{query}");
        }

        // ==================================================
        // 2️⃣ RETURN URL – HIỂN THỊ KẾT QUẢ
        // ==================================================
        [HttpGet("payment/vnpay-return")]
        public IActionResult VnpayReturn()
        {
            ViewBag.ResponseCode = Request.Query["vnp_ResponseCode"].ToString();
            ViewBag.OrderId = Request.Query["vnp_TxnRef"].ToString();

            return View("VnpayResult");
        }

        // ==================================================
        // 3️⃣ HMAC SHA512 (TẠO CHỮ KÝ)
        // ==================================================
        private static string HmacSHA512(string key, string input)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            return BitConverter
                .ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(input)))
                .Replace("-", "")
                .ToUpper();
        }

        // ==================================================
        // 4️⃣ URL ENCODE RFC3986 (CHÌA KHÓA HẾT LỖI)
        // ==================================================
        private static string UrlEncodeRFC3986(string value)
        {
            return Uri.EscapeDataString(value)
                .Replace("%7E", "~")
                .Replace("+", "%20");
        }
    }
}
