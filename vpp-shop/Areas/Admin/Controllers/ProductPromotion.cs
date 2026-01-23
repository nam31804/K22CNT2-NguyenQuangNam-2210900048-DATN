using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;

namespace vpp_shop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductPromotionController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public ProductPromotionController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            ViewBag.Products = _context.Products.ToList();

            var list = _context.ProductPromotions
                .Include(x => x.Product)
                .OrderBy(x => x.Position)
                .ThenByDescending(x => x.Id)
                .ToList();

            return View(list);
        }

        // ================= GET (SỬA – HIỆN NỘI DUNG CŨ) =================
        [HttpGet]
        public IActionResult GetPromotion(int id)
        {
            var item = _context.ProductPromotions
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    id = x.Id,
                    productId = x.ProductId,
                    title = x.Title ?? "",
                    content = x.Content ?? "",   // 🔥 FIX LỖI NỘI DUNG TRỐNG
                    position = x.Position
                })
                .FirstOrDefault();

            if (item == null) return NotFound();
            return Json(item);
        }

        // ================= CREATE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductPromotion model)
        {
            if (model.ProductId <= 0) return RedirectToAction("Index");

            // 🔥 BÀI MỚI LUÔN LÊN ĐẦU
            foreach (var p in _context.ProductPromotions)
            {
                p.Position += 1;
            }

            model.Title = model.Title ?? "";
            model.Content = model.Content ?? "";
            model.Position = 1;
            model.IsActive = true;
            model.CreatedAt = DateTime.Now;

            _context.ProductPromotions.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= EDIT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductPromotion model)
        {
            var old = _context.ProductPromotions.Find(model.Id);
            if (old == null) return NotFound();

            old.ProductId = model.ProductId;
            old.Title = model.Title ?? "";
            old.Content = model.Content ?? "";

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var item = _context.ProductPromotions.Find(id);
            if (item == null) return NotFound();

            _context.ProductPromotions.Remove(item);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= TOGGLE =================
        [HttpPost]
        public IActionResult Toggle(int id)
        {
            var item = _context.ProductPromotions.Find(id);
            if (item == null) return Json(false);

            item.IsActive = !item.IsActive;
            _context.SaveChanges();

            return Json(new { success = true, isActive = item.IsActive });
        }

        // ================= UPDATE POSITION (DỒN ĐÚNG) =================
        [HttpPost]
        public IActionResult UpdatePosition(int id, int position)
        {
            var current = _context.ProductPromotions.Find(id);
            if (current == null) return Json(false);

            if (position < 1) position = 1;

            int oldPos = current.Position;
            if (position == oldPos) return Json(true);

            if (position < oldPos)
            {
                // kéo lên → mấy thằng giữa +1
                var list = _context.ProductPromotions
                    .Where(x => x.Position >= position && x.Position < oldPos && x.Id != id)
                    .ToList();

                foreach (var item in list)
                    item.Position += 1;
            }
            else
            {
                // kéo xuống → mấy thằng giữa -1
                var list = _context.ProductPromotions
                    .Where(x => x.Position <= position && x.Position > oldPos && x.Id != id)
                    .ToList();

                foreach (var item in list)
                    item.Position -= 1;
            }

            current.Position = position;
            _context.SaveChanges();

            return Json(true);
        }
    }
}
