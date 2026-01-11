using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;
using vpp_shop.Models.ViewModels;

namespace vpp_shop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public CheckoutController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // =========================
        // GET: HIỆN TRANG CHECKOUT
        // =========================
        public async Task<IActionResult> Index(string itemIds)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var ids = itemIds.Split(',').Select(int.Parse).ToList();

            var items = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ids.Contains(ci.Id))
                .ToListAsync();

            if (!items.Any())
                return RedirectToAction("Index", "Cart");

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var total = items.Sum(i => i.Product.Price * i.Quantity);

            var model = new CheckoutViewModel
            {
                Items = items,
                ReceiverName = address?.ReceiverName ?? "",
                Phone = address?.Phone ?? "",
                Address = address?.Address ?? "",
                TotalMoney = total,
                SavedAddresses = addresses
            };

            return View(model);
        }

        // =========================
        // POST: XÁC NHẬN ĐẶT HÀNG
        // =========================
        [HttpPost]
        public async Task<IActionResult> Confirm(
            CheckoutViewModel model,
            List<int> itemIds)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // 1️⃣ LƯU / UPDATE ĐỊA CHỈ
            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == true);


            if (address == null)
            {
                address = new UserAddress
                {
                    UserId = userId.Value,
                    ReceiverName = model.ReceiverName,
                    Phone = model.Phone,
                    Address = model.Address,
                    IsDefault = true
                };
                _context.UserAddresses.Add(address);
            }
            else
            {
                address.ReceiverName = model.ReceiverName;
                address.Phone = model.Phone;
                address.Address = model.Address;
            }

            // 2️⃣ LẤY CART ITEM ĐƯỢC CHỌN
            var items = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => itemIds.Contains(ci.Id))
                .ToListAsync();

            if (!items.Any())
                return RedirectToAction("Index", "Cart");

            decimal total = items.Sum(i => i.Product.Price * i.Quantity);

            // 3️⃣ KIỂM TRA VÍ
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null || wallet.Balance < total)
            {
                TempData["Error"] = "Ví không đủ tiền";
                return RedirectToAction("Index");
            }

            // 4️⃣ TRỪ TIỀN
            wallet.Balance -= total;
            wallet.UpdatedAt = DateTime.Now;

            // 5️⃣ TẠO ORDER
            var order = new Order
            {
                UserId = userId.Value,
                ShippingName = model.ReceiverName,
                ShippingPhone = model.Phone,
                ShippingAddress = model.Address,
                TotalMoney = total,
                PaymentMethod = "WALLET",
                Status = "PAID",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 6️⃣ ORDER ITEMS
            foreach (var item in items)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Price = item.Product.Price,
                    Quantity = item.Quantity
                });
            }

            // 7️⃣ XÓA CART ITEM ĐÃ MUA
            _context.CartItems.RemoveRange(items);

            await _context.SaveChangesAsync();

            // 8️⃣ CHUYỂN VỀ LỊCH SỬ ĐƠN HÀNG
            return RedirectToAction("History", "Orders");
        }
    }
}
