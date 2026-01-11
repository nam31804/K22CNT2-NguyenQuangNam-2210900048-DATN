using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public CategoryController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> ReloadTable()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return PartialView("_CategoryTable", categories);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest("Tên thể loại không được để trống");

            if (category.Id == 0)
            {
                category.CreatedAt = DateTime.Now;
                _context.Categories.Add(category);
            }
            else
            {
                var old = await _context.Categories.FindAsync(category.Id);
                if (old == null) return NotFound();

                old.Name = category.Name;
                old.Description = category.Description;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.Products.Any())
                return BadRequest("Không thể xoá vì thể loại đang có sản phẩm");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> Get(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return Json(new
            {
                category.Id,
                category.Name,
                category.Description,
                Products = category.Products.Select(p => new
                {
                    p.Id,
                    p.Name
                })
            });
        }
    }
}
