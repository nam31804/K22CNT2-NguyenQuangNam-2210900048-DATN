using System;
using System.Collections.Generic;
namespace vpp_shop.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal RevenueCompleted { get; set; }
        public decimal RevenueProcessing { get; set; }
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();
        public List<string> DailyLabels { get; set; } = new();
        public List<decimal> DailyRevenue { get; set; } = new();
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }
        public int PaidOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}
