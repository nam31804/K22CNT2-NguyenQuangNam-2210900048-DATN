using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace vpp_shop.Models;

public partial class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public string? Image { get; set; }

    public string? Slug { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ValidateNever]   // 👈 DÒNG QUYẾT ĐỊNH
    public virtual Category Category { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ValidateNever]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ValidateNever]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
