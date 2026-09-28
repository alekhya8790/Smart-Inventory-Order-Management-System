namespace SmartIOMS.DTOs.Cart
{
    public class CartResponse
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public decimal TotalAmount { get; set; }

        public List<CartItemResponse> CartItems { get; set; }
            = new List<CartItemResponse>();
    }
}