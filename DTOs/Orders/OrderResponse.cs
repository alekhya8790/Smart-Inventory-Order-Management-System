namespace SmartIOMS.DTOs.Orders
{
    public class OrderResponse
    {
        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public int UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Pincode { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime OrderedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public List<OrderItemResponse> Items { get; set; }
            = new List<OrderItemResponse>();
    }
}