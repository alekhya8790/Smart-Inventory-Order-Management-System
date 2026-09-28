using System.ComponentModel.DataAnnotations;

namespace SmartIOMS.DTOs.Cart
{
    public class AddToCartRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}