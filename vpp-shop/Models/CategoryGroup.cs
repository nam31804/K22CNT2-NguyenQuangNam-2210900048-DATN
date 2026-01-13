using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vpp_shop.Models
{
    [Table("category_groups")]
    public class CategoryGroup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // 1 group có nhiều category
        public ICollection<Category> Categories { get; set; }
    }
}
