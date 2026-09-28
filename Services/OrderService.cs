using Microsoft.EntityFrameworkCore;
using SmartIOMS.Data;
using SmartIOMS.DTOs.Orders;
using SmartIOMS.Models;

namespace SmartIOMS.Services
{
    public class OrderService
    {
        private readonly SmartIOMSDbContext _context;

        public OrderService(SmartIOMSDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // PLACE ORDER
        // =========================================================

        public async Task<OrderResponse> PlaceOrderAsync(
            int userId,
            PlaceOrderRequest request)
        {
            var cart = await _context.Carts
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new Exception("Your cart is empty.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                decimal totalAmount = 0;

                // Validate cart items
                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.Product == null)
                    {
                        throw new Exception(
                            $"Product with ID {cartItem.ProductId} was not found.");
                    }

                    if (!cartItem.Product.IsActive)
                    {
                        throw new Exception(
                            $"Product '{cartItem.Product.ProductName}' is no longer available.");
                    }

                    if (cartItem.Quantity <= 0)
                    {
                        throw new Exception(
                            $"Invalid quantity for product '{cartItem.Product.ProductName}'.");
                    }

                    if (cartItem.Quantity >
                        cartItem.Product.StockQuantity)
                    {
                        throw new Exception(
                            $"Insufficient stock for '{cartItem.Product.ProductName}'. " +
                            $"Available: {cartItem.Product.StockQuantity}, " +
                            $"Requested: {cartItem.Quantity}.");
                    }

                    totalAmount +=
                        cartItem.Product.Price *
                        cartItem.Quantity;
                }

                // Create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    ShippingAddress = request.ShippingAddress.Trim(),
                    City = request.City.Trim(),
                    State = request.State.Trim(),
                    Pincode = request.Pincode.Trim(),
                    PaymentMethod = request.PaymentMethod.Trim(),
                    PaymentStatus = "Pending",
                    OrderedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);

                await _context.SaveChangesAsync();

                // Create order items and reduce stock
                foreach (var cartItem in cart.CartItems)
                {
                    var product = cartItem.Product!;

                    // Keep original RowVersion for concurrency check
                    byte[] originalRowVersion =
                        product.RowVersion.ToArray();

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        SKU = product.SKU,
                        UnitPrice = product.Price,
                        Quantity = cartItem.Quantity,
                        SubTotal =
                            product.Price *
                            cartItem.Quantity
                    };

                    _context.OrderItems.Add(orderItem);

                    // Reduce stock
                    product.StockQuantity -=
                        cartItem.Quantity;

                    // Explicit concurrency value
                    _context.Entry(product)
                        .Property(x => x.RowVersion)
                        .OriginalValue = originalRowVersion;

                    // Inventory transaction
                    var inventoryTransaction =
                        new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            TransactionType = "Order",
                            Quantity = cartItem.Quantity,
                            ReferenceId = order.OrderId,
                            Remarks =
                                $"Stock deducted for order {order.OrderNumber}",
                            CreatedAt = DateTime.UtcNow
                        };

                    _context.InventoryTransactions.Add(
                        inventoryTransaction);
                }

                // Clear cart
                _context.CartItems.RemoveRange(
                    cart.CartItems);

                cart.UpdatedAt = DateTime.UtcNow;

                // Save everything
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return await GetOrderByIdAsync(
                    userId,
                    order.OrderId)
                    ?? throw new Exception(
                        "Order was created but could not be retrieved.");
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                throw new Exception(
                    "The product stock was updated by another order. " +
                    "Please refresh the product stock and try again.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // =========================================================
        // CANCEL ORDER + RESTORE STOCK
        // =========================================================

        public async Task<OrderResponse?> CancelOrderAsync(
            int userId,
            int orderId)
        {
            // Get customer's own order
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId &&
                         x.UserId == userId);

            if (order == null)
            {
                return null;
            }

            // Already cancelled
            if (order.Status == "Cancelled")
            {
                throw new Exception(
                    "Order is already cancelled.");
            }

            // Only pending orders can be cancelled
            if (order.Status != "Pending")
            {
                throw new Exception(
                    "Only pending orders can be cancelled.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(
                            x => x.ProductId ==
                                 orderItem.ProductId);

                    if (product == null)
                    {
                        throw new Exception(
                            $"Product with ID {orderItem.ProductId} was not found.");
                    }

                    // Restore stock
                    product.StockQuantity +=
                        orderItem.Quantity;

                    // Record stock restoration
                    var inventoryTransaction =
                        new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            TransactionType = "Cancellation",
                            Quantity = orderItem.Quantity,
                            ReferenceId = order.OrderId,
                            Remarks =
                                $"Stock restored after cancellation of order {order.OrderNumber}",
                            CreatedAt = DateTime.UtcNow
                        };

                    _context.InventoryTransactions.Add(
                        inventoryTransaction);
                }

                // Update order status
                order.Status = "Cancelled";
                order.CancelledAt = DateTime.UtcNow;

                // Save stock + inventory transaction + order
                await _context.SaveChangesAsync();

                // Commit
                await transaction.CommitAsync();

                return await GetOrderByIdAsync(
                    userId,
                    orderId);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                throw new Exception(
                    "Stock was updated by another operation. " +
                    "Please refresh and try again.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // =========================================================
        // GET CUSTOMER ORDER BY ID
        // =========================================================

        public async Task<OrderResponse?> GetOrderByIdAsync(
            int userId,
            int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId &&
                         x.UserId == userId);

            if (order == null)
            {
                return null;
            }

            return MapToResponse(order);
        }


        // =========================================================
        // GET CUSTOMER ORDER HISTORY
        // =========================================================

        public async Task<List<OrderResponse>> GetMyOrdersAsync(
            int userId)
        {
            var orders = await _context.Orders
                .Include(x => x.OrderItems)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.OrderedAt)
                .ToListAsync();

            return orders
                .Select(MapToResponse)
                .ToList();
        }


        // =========================================================
        // GENERATE UNIQUE ORDER NUMBER
        // =========================================================

        private static string GenerateOrderNumber()
        {
            return
                $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
                .Substring(0, 25)
                .ToUpper();
        }


        // =========================================================
        // MAP ENTITY TO RESPONSE
        // =========================================================

        private static OrderResponse MapToResponse(
            Order order)
        {
            var response = new OrderResponse
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                Pincode = order.Pincode,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderedAt = order.OrderedAt,
                CancelledAt = order.CancelledAt
            };

            foreach (var item in order.OrderItems)
            {
                response.Items.Add(
                    new OrderItemResponse
                    {
                        OrderItemId =
                            item.OrderItemId,

                        ProductId =
                            item.ProductId,

                        ProductName =
                            item.ProductName,

                        SKU =
                            item.SKU,

                        UnitPrice =
                            item.UnitPrice,

                        Quantity =
                            item.Quantity,

                        SubTotal =
                            item.SubTotal
                    });
            }

            return response;
        }
    }
}