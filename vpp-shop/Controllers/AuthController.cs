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

                // ❗ XOÁ SESSION CŨ (RẤT QUAN TRỌNG)
                HttpContext.Session.Remove("ADMIN_ID");
                HttpContext.Session.Remove("STAFF_ID");

                if (admin.Role == "ADMIN")
                {
                    HttpContext.Session.SetInt32("ADMIN_ID", admin.Id);
                }
                else if (admin.Role == "STAFF")
                {
                    HttpContext.Session.SetInt32("STAFF_ID", admin.Id);
                }

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

    
    // ================= QUÊN MẬT KHẨU =================
[HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(
            string email,
            string phone,
            string newPassword,
            string confirmPassword
        )
        {

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại";
                return View();
            }

            ViewBag.Email = email;

            if (string.IsNullOrEmpty(phone))
            {
                ViewBag.Step = "PHONE";
                return View();
            }

            if (user.Phone != phone)
            {
                ViewBag.Step = "PHONE";
                ViewBag.Error = "Số điện thoại không đúng";
                return View();
            }

            ViewBag.Phone = phone;

            if (string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Step = "RESET";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Step = "RESET";
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            user.Password = newPassword;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công";
            return RedirectToAction("Login");
        }
    }
    }
