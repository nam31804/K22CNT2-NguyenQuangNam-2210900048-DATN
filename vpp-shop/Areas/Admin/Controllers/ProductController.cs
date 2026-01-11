using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public ProductController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // =========================
        // HÀM TẠO SLUG (TỰ ĐỘNG)
        // =========================
        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var uc = Char.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            var slug = sb.ToString().Normalize(NormalizationForm.FormC);
            slug = slug.ToLower();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

            return slug;
        }

        // =========================
        // DANH SÁCH
        // =========================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            // BẮT BUỘC CHỌN THỂ LOẠI
            if (product.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn thể loại");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            // TẠO SLUG TỰ ĐỘNG
            product.Slug = GenerateSlug(product.Name);

            // UPLOAD ẢNH
            if (imageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/products",
                    fileName
                );

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.Image = fileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (product.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn thể loại");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            var oldProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (oldProduct == null) return NotFound();

            // TẠO LẠI SLUG NẾU ĐỔI TÊN
            product.Slug = GenerateSlug(product.Name);

            // XỬ LÝ ẢNH
            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(oldProduct.Image))
                {
                    var oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images/products",
                        oldProduct.Image
                    );
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var newPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/products",
                    fileName
                );

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.Image = fileName;
            }
            else
            {
                product.Image = oldProduct.Image;
            }

            _context.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // =========================
        // DELETE (POST) – XOÁ TEST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // XOÁ order_items TRƯỚC (TEST)
            var orderItems = _context.OrderItems
                .Where(o => o.ProductId == id);
            _context.OrderItems.RemoveRange(orderItems);

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // XOÁ ẢNH
            if (!string.IsNullOrEmpty(product.Image))
            {
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/products",
                    product.Image
                );
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
