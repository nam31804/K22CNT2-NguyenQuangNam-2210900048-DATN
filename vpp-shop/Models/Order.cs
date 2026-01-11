using System;
using System.Collections.Generic;

namespace vpp_shop.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ShippingName { get; set; } = null!;

    public string ShippingPhone { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public decimal TotalMoney { get; set; }

    public string? VoucherCode { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual User User { get; set; } = null!;

    public virtual Voucher? VoucherCodeNavigation { get; set; }
}
