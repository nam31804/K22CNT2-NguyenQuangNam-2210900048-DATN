using Microsoft.AspNetCore.Mvc;
using vpp_shop.Data;
using vpp_shop.Models;
using vpp_shop.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace vpp_shop.Controllers
{
    public class AuthController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public AuthController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // ================= ADMIN / STAFF =================
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == model.Email
                                       && a.Password == model.Password);

            if (admin != null)
            {
                // 🔒 ADMIN / STAFF BỊ KHOÁ
                if (admin.IsActive == false)
                {
                    ViewBag.Error = "Tài khoản quản trị đã bị khoá";
                    return View();
                }

                HttpContext.Session.SetInt32("ADMIN_ID", admin.Id);
                HttpContext.Session.SetString("ADMIN_NAME", admin.FullName ?? "Admin");
                HttpContext.Session.SetString("ADMIN_ROLE", admin.Role);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // ================= USER =================
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email
                                       && u.Password == model.Password);

            if (user != null)
            {
                // 🔒 USER BỊ KHOÁ
                if (user.IsActive == false)
                {
                    ViewBag.Error = "Tài khoản này đã bị khoá";
                    return View();
                }

                HttpContext.Session.SetInt32("USER_ID", user.Id);
                HttpContext.Session.SetString("USER_NAME", user.FullName);

                return RedirectToAction("Index", "Home");
            }

            // ================= SAI TÀI KHOẢN =================
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            return View();
        }


        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (exists)
            {
                ViewBag.Error = "Email đã tồn tại";
                return View();
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                IsActive = true,
                Password = model.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

    }
}
