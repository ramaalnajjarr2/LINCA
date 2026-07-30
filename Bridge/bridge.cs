using Microsoft.EntityFrameworkCore;
using LINCA_v1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LINCA.Models;
namespace LINCA_v1.Bridge
{
    public class bridge : IdentityDbContext<Users>
    {
        public bridge(DbContextOptions<bridge> options) : base(options)
        {
        }
        public DbSet<Marketprop> MarketsTable { get; set; }    
        public DbSet<AdminsProp> AdminsTable { get; set; }
        public DbSet<Customersprop> CustomersTable { get; set; }
        public DbSet<Sellersprop> SellersTable { get; set; }
        public DbSet<Productsprop> ProductsTable { get; set; }
        public DbSet<Order> OrderTable { get; set; }
        public DbSet<OrderItem> OrderItemTable { get; set; }
        public DbSet<Cartitems> CartItemsTable { get; set; }
        public DbSet<SellerRequest> SellerRequestTable { get; set; }
        public DbSet<Service>ServicetTable { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // VERY IMPORTANT
        }
    }
}

