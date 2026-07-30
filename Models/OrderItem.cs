using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LINCA_v1.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        // Either ProductId or ServiceId will be set
        public int? ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Productsprop? Product { get; set; }

        public int? ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public Service? Service { get; set; }

        [Required]
        public string SellerId { get; set; } = null!;

        public string? SellerName { get; set; }

        [Required]
        [Range(1, 999)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Optional: keep ONLY if you want per-item completion
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
    }
}