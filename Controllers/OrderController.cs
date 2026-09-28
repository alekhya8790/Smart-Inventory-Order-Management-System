using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIOMS.DTOs.Orders;
using SmartIOMS.Services;

namespace SmartIOMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // =========================================================
        // POST: api/Order
        // PLACE ORDER
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            [FromBody] PlaceOrderRequest request)
        {
            int userId = GetUserId();

            var order =
                await _orderService.PlaceOrderAsync(
                    userId,
                    request);

            return Ok(order);
        }


        // =========================================================
        // PUT: api/Order/{orderId}/cancel
        // CANCEL ORDER + RESTORE STOCK
        // =========================================================

        [HttpPut("{orderId:int}/cancel")]
        public async Task<IActionResult> CancelOrder(
            [FromRoute] int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Order ID is required."
                });
            }

            int userId = GetUserId();

            var order =
                await _orderService.CancelOrderAsync(
                    userId,
                    orderId);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(order);
        }


        // =========================================================
        // GET: api/Order
        // CUSTOMER ORDER HISTORY
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetUserId();

            var orders =
                await _orderService.GetMyOrdersAsync(
                    userId);

            return Ok(orders);
        }


        // =========================================================
        // GET: api/Order/{orderId}
        // GET ONE CUSTOMER ORDER
        // =========================================================

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrderById(
            [FromRoute] int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Order ID is required."
                });
            }

            int userId = GetUserId();

            var order =
                await _orderService.GetOrderByIdAsync(
                    userId,
                    orderId);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(order);
        }


        // =========================================================
        // GET USER ID FROM JWT
        // =========================================================

        private int GetUserId()
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "User ID was not found in token.");
            }

            if (!int.TryParse(
                    userId,
                    out int parsedUserId))
            {
                throw new UnauthorizedAccessException(
                    "Invalid user ID in token.");
            }

            return parsedUserId;
        }
    }
}