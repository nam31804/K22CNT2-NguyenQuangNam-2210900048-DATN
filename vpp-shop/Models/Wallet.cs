using System;
using System.Collections.Generic;

namespace vpp_shop.Models;

public partial class Wallet
{
    public int UserId { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
