using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LINCA_v1.Models
{
    public class Cartitems
    {
        [Key]
        public int CartItemId { get; set; }

        // Owner of the cart (Identity User)
        [Required]
        public string BuyerId { get; set; } = null!;

        // Market snapshot (NO FK)
        [Required]
        public int MarketId { get; set; }

        // Product snapshot (NO FK)
        [Required]
        public int ProductId { get; set; }

        // Snapshot fields
        [Required]
        public string ProductName { get; set; } = "";

        public string? ProductImage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal TotalPrice => Price * Quantity;
    }
}