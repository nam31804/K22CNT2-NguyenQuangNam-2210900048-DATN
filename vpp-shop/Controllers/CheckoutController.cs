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
                SavedAddresses = addresses,
                PaymentMethod = "COD" // mặc định
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

            // ===== LẤY CART ITEMS =====
            var items = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => itemIds.Contains(ci.Id))
                .ToListAsync();

            if (!items.Any())
                return RedirectToAction("Index", "Cart");

            decimal total = items.Sum(i => i.Product.Price * i.Quantity);

            // ===== XỬ LÝ ĐỊA CHỈ (QUAN TRỌNG) =====
            UserAddress shippingAddress;

            // 1️⃣ Chọn địa chỉ đã lưu
            if (model.SelectedAddressId.HasValue)
            {
                shippingAddress = await _context.UserAddresses
                    .FirstOrDefaultAsync(a =>
                        a.Id == model.SelectedAddressId &&
                        a.UserId == userId);

                if (shippingAddress == null)
                    return RedirectToAction("Index", "Cart");
            }
            else
            {
                // 2️⃣ Nhập địa chỉ mới
                shippingAddress = new UserAddress
                {
                    ReceiverName = model.ReceiverName,
                    Phone = model.Phone,
                    Address = model.Address,
                    City = model.City,
                    District = model.District
                };

                // 👉 CHỈ LƯU KHI TICK
                if (model.SaveAddress)
                {
                    shippingAddress.UserId = userId.Value;
                    shippingAddress.IsDefault = false;

                    _context.UserAddresses.Add(shippingAddress);
                    await _context.SaveChangesAsync();
                }
            }

            // ===== TẠO ORDER (LUÔN LẤY TỪ shippingAddress) =====
            var order = new Order
            {
                UserId = userId.Value,
                ShippingName = shippingAddress.ReceiverName,
                ShippingPhone = shippingAddress.Phone,
                ShippingAddress =
        $"{shippingAddress.Address}, {shippingAddress.District}, {shippingAddress.City}",

                TotalMoney = model.TotalMoney - model.DiscountAmount,
                VoucherCode = model.VoucherCode,
                DiscountAmount = model.DiscountAmount,

                PaymentMethod = model.PaymentMethod,
                Status = "PENDING",
                Note = model.Note,
                CreatedAt = DateTime.Now
            };


            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // ===== ORDER ITEMS =====
            foreach (var item in items)
            {
                // Nếu chưa set stock → coi như 0
                var stock = item.Product.Stock ?? 0;

                if (stock < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm {item.Product.Name} không đủ số lượng";
                    return RedirectToAction("Index", "Cart");
                }

                // 🔻 TRỪ TỒN KHO
                item.Product.Stock = stock - item.Quantity;

                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Price = item.Product.Price,
                    Quantity = item.Quantity
                });
            }

            // ===== XÓA GIỎ HÀNG =====
            _context.CartItems.RemoveRange(items);

            // ===== THANH TOÁN VÍ (NẾU CHỌN) =====
            if (model.PaymentMethod == "WALLET")
            {
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null || wallet.Balance < order.TotalMoney)
                {
                    TempData["Error"] = "Ví không đủ tiền";
                    return RedirectToAction("Index", "Cart");
                }

                wallet.Balance -= order.TotalMoney;
                wallet.UpdatedAt = DateTime.Now;
                order.Status = "PAID";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("History", "Orders");
        }
    }
    }
