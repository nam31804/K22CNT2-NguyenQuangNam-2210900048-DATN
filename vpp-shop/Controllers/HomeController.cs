using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using vpp_shop.Data;
using vpp_shop.Models;
using Microsoft.EntityFrameworkCore;

namespace vpp_shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly VanPhongPhamDBContext _context;

        public HomeController(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 12
            var products = await _context.Products
                .OrderByDescending(p => p.Id)
                .Take(12)
                .ToListAsync();

            return View(products);
        }
    }
}