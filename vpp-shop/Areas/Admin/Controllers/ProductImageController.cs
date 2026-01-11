using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductImageController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public ProductImageController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX: LIST SẢN PHẨM + ẢNH CHÍNH + ẢNH PHỤ
        // =========================
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.ProductImages)
                .ToListAsync();

            return View(products);
        }

        // =========================
        // ADD MULTIPLE SUB IMAGES
        // =========================
        [HttpPost]
        public async Task<IActionResult> AddMultiple(int productId, List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest();

            foreach (var image in images)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/products/sub",
                    fileName
                );

                using var stream = new FileStream(path, FileMode.Create);
                await image.CopyToAsync(stream);

                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = fileName
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // =========================
        // DELETE SUB IMAGE
        // =========================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var img = await _context.ProductImages.FindAsync(id);
            if (img == null) return NotFound();

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images/products/sub",
                img.ImageUrl
            );

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _context.ProductImages.Remove(img);
            await _context.SaveChangesAsync();

            return Ok();
        }
        public async Task<IActionResult> Table()
        {
            var products = await _context.Products
                .Include(p => p.ProductImages)
                .ToListAsync();

            return PartialView("_ProductImageTable", products);
        }

    }
}
