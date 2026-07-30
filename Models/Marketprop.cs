using System.ComponentModel.DataAnnotations;
namespace LINCA_v1.Models
{
    public class Marketprop
    {
        [Key]
    public int Id { get; set; }

        public Marketprop(int id)
        {
            Id = id;
        }
        public Marketprop()
        {
            
        }

        public string Name { get; set; }
        public string? Description { get; set; }
        public string? imgurl { get; set; }
        [Required]
        public String Ownerid { get; set; }
    }
}
