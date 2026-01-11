using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public UserController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string keyword = "")
        {
            var q = _context.Users
                .Include(u => u.Orders)
                .Include(u => u.Wallet)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                q = q.Where(u =>
                    u.FullName.Contains(keyword) ||
                    u.Id.ToString() == keyword);
            }

            return View(await q.OrderByDescending(u => u.CreatedAt).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var u = await _context.Users
                .Include(x => x.Orders)
                .Include(x => x.Wallet)
                .Include(x => x.UserAddresses)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (u == null) return NotFound();

            var addr = u.UserAddresses.FirstOrDefault();

            return Json(new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                IsActive = u.IsActive ?? true,
                Balance = u.Wallet?.Balance ?? 0,
                OrderCount = u.Orders.Count,
                Address = addr == null ? null : new
                {
                    addr.Id,
                    addr.ReceiverName,
                    addr.Phone,
                    addr.Address,
                    addr.District,
                    addr.City
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAll(
            int userId,
            string fullName,
            string email,
            string phone,
            int addressId,
            string receiverName,
            string receiverPhone,
            string address,
            string district,
            string city)
        {
            if (string.IsNullOrWhiteSpace(fullName)
             || string.IsNullOrWhiteSpace(email)
             || string.IsNullOrWhiteSpace(phone)
             || string.IsNullOrWhiteSpace(receiverName)
             || string.IsNullOrWhiteSpace(receiverPhone)
             || string.IsNullOrWhiteSpace(address))
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(userId);
            var addr = await _context.UserAddresses.FindAsync(addressId);

            if (user == null || addr == null) return NotFound();

            user.FullName = fullName;
            user.Email = email;
            user.Phone = phone;

            addr.ReceiverName = receiverName;
            addr.Phone = receiverPhone;
            addr.Address = address;
            addr.District = district;
            addr.City = city;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();

            u.Password = "123456";
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();

            u.IsActive = !(u.IsActive ?? true);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
