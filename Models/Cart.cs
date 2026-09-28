namespace SmartIOMS.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public User? User { get; set; }

        // Navigation Property
        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();
    }
}