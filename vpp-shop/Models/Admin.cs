using System;
using System.Collections.Generic;

namespace vpp_shop.Models;

public partial class Admin
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string Role { get; set; } = "STAFF"; // fix cứng

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
