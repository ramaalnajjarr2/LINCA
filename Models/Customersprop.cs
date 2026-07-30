namespace LINCA_v1.Models
{
    public class Customersprop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Customersprop(int id)
        {
            Id = id;
            Name = "Customer " + Name;
        }
    }
}
