using System.Collections.Generic;
using vpp_shop.Models;
namespace vpp_shop.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new();

        // ===== ĐỊA CHỈ =====
        public int? SelectedAddressId { get; set; }
        public bool SaveAddress { get; set; } = true;

        public string? ReceiverName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }

        // ===== THANH TOÁN =====
        public string PaymentMethod { get; set; } = "COD";

        // ===== VOUCHER =====
        public string? VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Note { get; set; }
        public decimal TotalMoney { get; set; }

        public List<UserAddress> SavedAddresses { get; set; } = new();
    }
}