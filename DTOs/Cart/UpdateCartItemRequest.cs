using System.ComponentModel.DataAnnotations;

namespace SmartIOMS.DTOs.Cart
{
    public class UpdateCartItemRequest
    {
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}