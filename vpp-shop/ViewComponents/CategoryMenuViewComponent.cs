using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;
using vpp_shop.Models;

namespace vpp_shop.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly VanPhongPhamDBContext _context;

        public CategoryMenuViewComponent(VanPhongPhamDBContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var groups = await _context.CategoryGroups
                .Include(g => g.Categories)
                .ToListAsync();

            return View(groups);
        }
    }
}
