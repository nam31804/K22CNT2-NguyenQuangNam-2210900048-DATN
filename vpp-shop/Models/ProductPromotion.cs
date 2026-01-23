using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using vpp_shop.Models;
namespace vpp_shop.Models
{
    [Table("product_promotions")] // 🔥 QUAN TRỌNG
    public class ProductPromotion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("title")]
        public string Title { get; set; } = "";

        [Column("content")]
        public string Content { get; set; } = "";

        [Column("position")]
        public int Position { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}