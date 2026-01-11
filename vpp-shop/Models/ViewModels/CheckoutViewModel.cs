using System.Collections.Generic;
using vpp_shop.Models;

namespace vpp_shop.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new();

        public string ReceiverName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public decimal TotalMoney { get; set; }
        public List<UserAddress> SavedAddresses { get; set; } = new();
    }
}
