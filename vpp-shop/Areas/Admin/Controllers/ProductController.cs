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
        // TẠO SLUG
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
            ViewBag.Categories = await _context.Categories.ToListAsync();
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // =========================
        // LẤY PRODUCT (JSON) – EDIT MODAL
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return Json(product);
        }

        // =========================
        // CREATE (POST) – MODAL
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (product.CategoryId == 0)
            {
                return BadRequest("Chưa chọn thể loại");
            }

            product.Slug = GenerateSlug(product.Name);

            if (imageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/products",
                    fileName
                );

                using var stream = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                product.Image = fileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // =========================
        // EDIT (POST) – MODAL
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile imageFile)
        {
            var oldProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (oldProduct == null) return NotFound();

            product.Slug = GenerateSlug(product.Name);

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

                using var stream = new FileStream(newPath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

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
        // DELETE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var orderItems = _context.OrderItems
                .Where(o => o.ProductId == id);
            _context.OrderItems.RemoveRange(orderItems);

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

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
