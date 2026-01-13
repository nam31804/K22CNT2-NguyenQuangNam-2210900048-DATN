using System;
using System.Collections.Generic;

namespace vpp_shop.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }
    // fk
    public int CategoryGroupId { get; set; }

    // 🔗 Navigation tới bảng cha
    public virtual CategoryGroup CategoryGroup { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
