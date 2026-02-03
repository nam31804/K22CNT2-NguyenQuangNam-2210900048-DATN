using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;

public class UserController : Controller
{
    private readonly VanPhongPhamDBContext _context;

    public UserController(VanPhongPhamDBContext context)
    {
        _context = context;
    }

    // ================== LẤY USER ID TỪ SESSION ==================
    private int GetUserId()
    {
        return HttpContext.Session.GetInt32("USER_ID") ?? 0;
    }

    // ================== TRANG TỔNG QUAN USER ==================
    public IActionResult Index()
    {
        int userId = GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Auth");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

        // TỰ ĐỘNG TẠO VÍ NẾU CHƯA CÓ
        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                UpdatedAt = DateTime.Now
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
        }

        ViewBag.Balance = wallet.Balance ?? 0;

        return View(user);
    }

    // ================== THÔNG TIN CÁ NHÂN ==================
    public IActionResult Profile()
    {
        int userId = GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Auth");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        var address = _context.UserAddresses
            .FirstOrDefault(a => a.UserId == userId && a.IsDefault == true);

        ViewBag.DefaultAddress = address;

        return View(user);
    }

    [HttpPost]
    public IActionResult UpdateProfile(User model)
    {
        int userId = GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Auth");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.FullName = model.FullName;
            user.Phone = model.Phone;
            _context.SaveChanges();
        }

        return RedirectToAction("Profile");
    }


    // ================== ĐỔI MẬT KHẨU ==================
    public IActionResult ChangePassword()
    {
        if (GetUserId() == 0)
            return RedirectToAction("Login", "Auth");

        return View();
    }

    [HttpPost]
    public IActionResult ChangePassword(string oldPassword, string newPassword)
    {
        int userId = GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Auth");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        if (user.Password != oldPassword)
        {
            ViewBag.Error = "Mật khẩu cũ không đúng";
            return View();
        }

        user.Password = newPassword;
        _context.SaveChanges();

        ViewBag.Success = "Đổi mật khẩu thành công";
        return View();
    }

    // ================== ĐƠN HÀNG ==================
    public IActionResult Orders(string type, int page = 1)
    {
        int userId = GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Auth");

        int pageSize = 15;

        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId);

        if (type == "pending")
            query = query.Where(o => o.Status != "COMPLETED" && o.Status != "CANCELLED");
        else if (type == "completed")
            query = query.Where(o => o.Status == "COMPLETED");
        else if (type == "cancelled")
            query = query.Where(o => o.Status == "CANCELLED");

        int totalOrders = query.Count();

        var orders = query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
        ViewBag.Type = type;

        return View(orders);
    }


    // ================== VÍ ==================
    public IActionResult Wallet()
    {
        int userId = GetUserId();
        if (userId == 0)
            return RedirectToAction("Login", "Auth");

        var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                UpdatedAt = DateTime.Now
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
        }

        var transactions = _context.WalletTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        ViewBag.Transactions = transactions;
        return View(wallet);
    }
    [HttpGet]
    public IActionResult GetOrderDetail(int id)
    {
        int userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var order = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == id && o.UserId == userId);

        if (order == null)
            return NotFound();

        return Json(new
        {
            order.Id,
            order.ShippingName,
            order.ShippingPhone,
            order.ShippingAddress,
            order.PaymentMethod,
            order.Status,
            order.TotalMoney,
            Items = order.OrderItems.Select(i => new
            {
                i.Product.Name,
                i.Product.Image,
                i.Quantity,
                i.Price
            })
        });
    }
    [HttpPost]
    public IActionResult CancelOrderAjax(int orderId)
    {
        int userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        using var tran = _context.Database.BeginTransaction();
        try
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return Json(new { success = false });

            if (order.Status != "PENDING" && order.Status != "PAID")
                return Json(new { success = false });

            // ✅ HOÀN KHO
            foreach (var item in order.OrderItems)
            {
                var product = _context.Products.First(p => p.Id == item.ProductId);
                product.Stock += item.Quantity;
            }

            // ✅ HOÀN TIỀN VÍ
            if (order.PaymentMethod == "WALLET")
            {
                var wallet = _context.Wallets.First(w => w.UserId == userId);
                wallet.Balance += order.TotalMoney;
                wallet.UpdatedAt = DateTime.Now;

                _context.WalletTransactions.Add(new WalletTransaction
                {
                    UserId = userId,
                    Type = "REFUND",
                    Amount = order.TotalMoney,
                    Description = $"Hoàn tiền huỷ đơn #{order.Id}"
                });
            }

            order.Status = "CANCELLED";

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = "CANCELLED",
                Note = "Khách hàng huỷ đơn"
            });

            _context.SaveChanges();
            tran.Commit();

            return Json(new { success = true });
        }
        catch
        {
            tran.Rollback();
            return Json(new { success = false });
        }
    }

}
