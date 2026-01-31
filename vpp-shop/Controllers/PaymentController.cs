using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using vpp_shop.Data;
using vpp_shop.Models;

namespace vpp_shop.Controllers
{
    public class PaymentController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        private const string VNP_TMN_CODE = "0FHMCN78";
        private const string VNP_HASH_SECRET = "00CTOW0JDCZ8HGZ71ZGXTELKYYDG97I8";
        private const string VNP_URL = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public PaymentController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // =========================
        // CREATE VNPAY PAYMENT (FINAL)
        // =========================
        public IActionResult CreateVnpayPayment(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return BadRequest("ORDER_NOT_FOUND");

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ip == "::1") ip = "127.0.0.1";

            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", VNP_TMN_CODE },
                { "vnp_Amount", ((long)order.TotalMoney * 100).ToString() },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", order.Id.ToString() },
                { "vnp_OrderInfo", "Thanh toan don hang " + order.Id },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },

                // 🔥 BẮT BUỘC CHO QR
                { "vnp_BankCode", "VNPAYQR" },

                { "vnp_ReturnUrl", $"{Request.Scheme}://{Request.Host}/Payment/VnpayReturn" },
                { "vnp_IpAddr", ip },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss") }
            };

            // ✅ 1. HASH DATA (RAW – KHÔNG ENCODE)
            string hashData = string.Join("&", vnpParams.Select(p => $"{p.Key}={p.Value}"));
            string secureHash = HmacSHA512(VNP_HASH_SECRET, hashData);

            // ✅ 2. QUERY STRING (CÓ ENCODE)
            string query = string.Join("&", vnpParams.Select(p =>
                $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));

            query += $"&vnp_SecureHashType=HmacSHA512&vnp_SecureHash={secureHash}";

            return Redirect($"{VNP_URL}?{query}");
        }

        public IActionResult VnpayReturn()
        {
            ViewBag.Code = Request.Query["vnp_ResponseCode"];
            ViewBag.OrderId = Request.Query["vnp_TxnRef"];
            return View();
        }

        // =========================
        // HMAC SHA512 (UPPERCASE)
        // =========================
        private static string HmacSHA512(string key, string input)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            return BitConverter
                .ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(input)))
                .Replace("-", "")
                .ToUpper();
        }
    }
}
