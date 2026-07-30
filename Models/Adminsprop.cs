using System.ComponentModel.DataAnnotations;

namespace LINCA.Models
{
    public class AdminsProp
    {
        [Key]
        public int AdminId {  get; set; }
        public string AdminName { get; set; }
        public AdminsProp()
        {
            
        }
    }
}
