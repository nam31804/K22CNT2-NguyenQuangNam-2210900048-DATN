using System;
using System.Collections.Generic;

namespace vpp_shop.Models;

public partial class UserAddress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? City { get; set; }

    public string? District { get; set; }

    public bool? IsDefault { get; set; }

    public virtual User User { get; set; } = null!;
}
