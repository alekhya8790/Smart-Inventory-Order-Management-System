namespace SmartIOMS.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        // Product snapshot at the time of purchase
        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal SubTotal { get; set; }

        // Navigation Properties

        public Order? Order { get; set; }

        public Product? Product { get; set; }
    }
}
