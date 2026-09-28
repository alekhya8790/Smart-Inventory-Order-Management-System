namespace SmartIOMS.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public int UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string ShippingAddress { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Pincode { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "CashOnDelivery";

        public string PaymentStatus { get; set; } = "Pending";

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CancelledAt { get; set; }

        // Navigation Properties

        public User? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}