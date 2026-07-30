using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LINCA_v1.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Accepted = 1,
        Completed = 2,
        Rejected = 3
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MarketId { get; set; }

        //Optional navigation(nice to have)
         [ForeignKey(nameof(MarketId))]
        public Marketprop? Market { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public String? Phone1 { get; set; }
        public String? Phone2 { get; set; }
        public String? Address { get; set; }


        public string? Note { get; set; } // one note for whole order

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public string BuyerId { get; set; } = null!;

        [ForeignKey(nameof(BuyerId))]
        public Users? Buyer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<OrderItem> Items { get; set; } = new();
    }
}