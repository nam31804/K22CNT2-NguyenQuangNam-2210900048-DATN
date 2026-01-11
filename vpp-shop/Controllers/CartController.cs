using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;

namespace vpp_shop.Controllers
{
    public class CartController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public CartController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // ============================
        // ADD TO CART
        // ============================
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId.Value,
                    CreatedAt = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // ============================
        // VIEW CART
        // ============================
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            //
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

                ViewBag.WalletBalance = wallet?.Balance ?? 0;

            return View(cart);
        }

        // ============================
        // AJAX: TĂNG SỐ LƯỢNG
        // ============================
        [HttpPost]
        public async Task<IActionResult> Increase(int id)
        {
            var item = await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            item.Quantity += 1;
            await _context.SaveChangesAsync();

            return Json(new
            {
                quantity = item.Quantity,
                itemTotal = item.Quantity * item.Product.Price
            });
        }

        // ============================
        // AJAX: GIẢM SỐ LƯỢNG (KHÔNG < 1)
        // ============================
        [HttpPost]
        public async Task<IActionResult> Decrease(int id)
        {
            var item = await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            if (item.Quantity > 1)
            {
                item.Quantity -= 1;
                await _context.SaveChangesAsync();
            }

            return Json(new
            {
                quantity = item.Quantity,
                itemTotal = item.Quantity * item.Product.Price
            });
        }

        // ============================
        // AJAX: XÓA SẢN PHẨM
        // ============================
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] List<int> cartItemIds)
        {
            var userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            if (cartItemIds == null || !cartItemIds.Any())
                return Json(new { success = false, message = "Vui lòng chọn sản phẩm để thanh toán" });

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Json(new { success = false, message = "Giỏ hàng trống" });

            // 🔥 CHỈ LẤY ITEM ĐƯỢC CHỌN
            var selectedItems = cart.CartItems
                .Where(ci => cartItemIds.Contains(ci.Id))
                .ToList();

            if (!selectedItems.Any())
                return Json(new { success = false, message = "Không có sản phẩm hợp lệ" });

            decimal total = selectedItems.Sum(i => i.Product.Price * i.Quantity);

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null || wallet.Balance < total)
                return Json(new { success = false, message = "Ví không đủ tiền" });

            // 🔻 Trừ tiền ví
            wallet.Balance -= total;
            wallet.UpdatedAt = DateTime.Now;

            // 🔻 Tạo đơn hàng
            var order = new Order
            {
                UserId = userId.Value,
                ShippingName = "Test User",
                ShippingPhone = "000000000",
                ShippingAddress = "Test Address",
                TotalMoney = total,
                PaymentMethod = "WALLET",
                Status = "PAID",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 🔻 Lưu chi tiết đơn
            foreach (var item in selectedItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Price = item.Product.Price,
                    Quantity = item.Quantity
                });
            }

            // 🔻 CHỈ XÓA ITEM ĐÃ THANH TOÁN
            _context.CartItems.RemoveRange(selectedItems);

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        public IActionResult CartCountPartial()
        {
            return ViewComponent("CartCount");
        }
    }
}
