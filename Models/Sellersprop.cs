namespace LINCA_v1.Models
{
    public class Sellersprop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Sellersprop(int id)
        {
            Id = id;
            Name = "Seller " + Name;
        }
    }
}
