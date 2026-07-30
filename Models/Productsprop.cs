using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LINCA_v1.Models
{
    public class Productsprop
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? Description { get; set; }
        public string? imgurl { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public int MarketId { get; set; }

        [ForeignKey(nameof(MarketId))]
        public Marketprop Market { get; set; }
        // Foreign key (set in controller)
        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]   // ⭐ IMPORTANT
        public Users Seller { get; set; }

        public Productsprop()
        {
            
        }

    }

        

        
    }
