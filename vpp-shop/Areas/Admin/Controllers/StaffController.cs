using Microsoft.AspNetCore.Mvc;
using vpp_shop.Data;
using vpp_shop.Models;
using System.Linq;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffController : AdminBaseController
    {
        private readonly VanPhongPhamDBContext _context;

        public StaffController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // Trang chính
        public IActionResult Index()
        {
            return View();
        }

        // Load danh sách staff (KHÔNG ADMIN)
        public IActionResult List(string keyword)
        {
            var staffs = _context.Admins
                .Where(x => x.Role == "STAFF");

            if (!string.IsNullOrEmpty(keyword))
            {
                staffs = staffs.Where(x =>
                    x.Username.Contains(keyword) ||
                    x.FullName.Contains(keyword) ||
                    x.Id.ToString().Contains(keyword)
                );
            }

            return PartialView("_StaffTable",
                staffs.OrderByDescending(x => x.Id).ToList());
        }


        [HttpPost]
        public IActionResult Create(vpp_shop.Models.Admin admin)
        {
            if (string.IsNullOrEmpty(admin.Username) || string.IsNullOrEmpty(admin.Password))
                return BadRequest("Không được để trống");

            if (_context.Admins.Any(x => x.Username == admin.Username))
                return BadRequest("Username đã tồn tại");

            admin.Role = "STAFF";
            admin.IsActive = true;
            admin.CreatedAt = DateTime.Now;

            _context.Admins.Add(admin);
            _context.SaveChanges();

            return Ok();
        }

        // Khoá / Mở staff
        [HttpPost]
        public IActionResult Toggle(int id)
        {
            var staff = _context.Admins
                .FirstOrDefault(x => x.Id == id && x.Role == "STAFF");

            if (staff == null) return NotFound();

            staff.IsActive = !staff.IsActive;
            _context.SaveChanges();

            return Ok();
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var staff = _context.Admins
                .FirstOrDefault(x => x.Id == id && x.Role == "STAFF");

            if (staff == null) return NotFound();

            _context.Admins.Remove(staff);
            _context.SaveChanges();

            return Ok();
        }
    }
}
