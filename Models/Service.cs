using System.ComponentModel.DataAnnotations;

namespace LINCA_v1.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        // seller
        [Required]
        public string ApplicationUserId { get; set; } = null!;
        public Users? Seller { get; set; }
    }
}
