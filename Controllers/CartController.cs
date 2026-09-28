using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIOMS.DTOs.Cart;
using SmartIOMS.Services;

namespace SmartIOMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            int userId = GetUserId();

            var cart = await _cartService.GetOrCreateCartAsync(userId);

            return Ok(cart);
        }

        // POST: api/Cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(
            [FromBody] AddToCartRequest request)
        {
            int userId = GetUserId();

            var cart = await _cartService.AddToCartAsync(
                userId,
                request);

            return Ok(cart);
        }

        // PUT: api/Cart/items/{cartItemId}
        [HttpPut("items/{cartItemId:int}")]
        public async Task<IActionResult> UpdateCartItem(
            [FromRoute(Name = "cartItemId")] int cartItemId,
            [FromBody] UpdateCartItemRequest request)
        {
            if (cartItemId <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Cart Item ID is required."
                });
            }

            int userId = GetUserId();

            var cart = await _cartService.UpdateCartItemAsync(
                userId,
                cartItemId,
                request);

            return Ok(cart);
        }

        // DELETE: api/Cart/items/{cartItemId}
        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveCartItem(
            [FromRoute(Name = "cartItemId")] int cartItemId)
        {
            if (cartItemId <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Cart Item ID is required."
                });
            }

            int userId = GetUserId();

            await _cartService.RemoveCartItemAsync(
                userId,
                cartItemId);

            return Ok(new
            {
                message = "Cart item removed successfully."
            });
        }

        // DELETE: api/Cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            int userId = GetUserId();

            await _cartService.ClearCartAsync(userId);

            return Ok(new
            {
                message = "Cart cleared successfully."
            });
        }

        // Get logged-in user ID from JWT
        private int GetUserId()
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "User ID was not found in token.");
            }

            if (!int.TryParse(userId, out int parsedUserId))
            {
                throw new UnauthorizedAccessException(
                    "Invalid user ID in token.");
            }

            return parsedUserId;
        }
    }
}