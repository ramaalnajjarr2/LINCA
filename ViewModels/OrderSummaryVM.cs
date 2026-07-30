namespace LINCA_v1.ViewModels   
{
    public class OrderSummaryVM
    {
        public int MarketId { get; set; }
        public string MarketName { get; set; } = "";

        // seller + buyer display
        public string SellerName { get; set; } = "";
        public string BuyerName { get; set; } = "";

        public string BuyerId { get; set; } = "";

        // invoice / delivery info
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Address { get; set; }

        public string? Note { get; set; }
        public decimal TotalPrice { get; set; }

        public List<OrderSummaryItemVM> Items { get; set; } = new();
    }

    public class OrderSummaryItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ImageUrl { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}