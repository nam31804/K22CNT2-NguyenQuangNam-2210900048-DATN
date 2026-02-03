using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vpp_shop.Models
{
    public class VoucherUsage
    {
        [Key]
        public int Id { get; set; }

        // ===== FK tới vouchers =====
        [Required]
        public int VoucherId { get; set; }

        [ForeignKey("VoucherId")]
        public Voucher Voucher { get; set; }

        // ===== FK tới users =====
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        // ===== FK tới orders =====
        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.Now;
    }
}
