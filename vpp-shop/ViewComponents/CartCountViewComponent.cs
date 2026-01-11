using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_shop.Data;

public class CartCountViewComponent : ViewComponent
{
    private readonly VanPhongPhamDBContext _context;
    public CartCountViewComponent(VanPhongPhamDBContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = HttpContext.Session.GetInt32("USER_ID");
        int count = 0;

        if (userId != null)
        {
            count = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => (int?)ci.Quantity) ?? 0;
        }

        return View(count);
    }
}
