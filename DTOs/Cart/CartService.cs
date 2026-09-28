using Microsoft.EntityFrameworkCore;
using SmartIOMS.Data;
using SmartIOMS.DTOs.Cart;
using SmartIOMS.Models;

namespace SmartIOMS.Services
{
    public class CartService
    {
        private readonly SmartIOMSDbContext _context;

        public CartService(SmartIOMSDbContext context)
        {
            _context = context;
        }

        // GET CURRENT USER CART
        public async Task<CartResponse> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
            }

            return MapToResponse(cart);
        }


        // ADD PRODUCT TO CART
        public async Task<CartResponse> AddToCartAsync(
            int userId,
            AddToCartRequest request)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductId == request.ProductId &&
                    x.IsActive);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            if (request.Quantity > product.StockQuantity)
            {
                throw new Exception(
                    $"Only {product.StockQuantity} items are available in stock.");
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cart.CartId &&
                    x.ProductId == request.ProductId);

            if (existingItem != null)
            {
                int newQuantity =
                    existingItem.Quantity + request.Quantity;

                if (newQuantity > product.StockQuantity)
                {
                    throw new Exception(
                        $"Only {product.StockQuantity} items are available in stock.");
                }

                existingItem.Quantity = newQuantity;
                existingItem.AddedAt = DateTime.UtcNow;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    AddedAt = DateTime.UtcNow
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetOrCreateCartAsync(userId);
        }


        // UPDATE CART ITEM
        public async Task<CartResponse> UpdateCartItemAsync(
            int userId,
            int cartItemId,
            UpdateCartItemRequest request)
        {
            var cartItem = await _context.CartItems
                .Include(x => x.Cart)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.CartItemId == cartItemId &&
                    x.Cart!.UserId == userId);

            if (cartItem == null)
            {
                throw new Exception("Cart item not found.");
            }

            if (cartItem.Product == null ||
                !cartItem.Product.IsActive)
            {
                throw new Exception("Product is not available.");
            }

            if (request.Quantity > cartItem.Product.StockQuantity)
            {
                throw new Exception(
                    $"Only {cartItem.Product.StockQuantity} items are available in stock.");
            }

            cartItem.Quantity = request.Quantity;

            cartItem.Cart!.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetOrCreateCartAsync(userId);
        }


        // REMOVE CART ITEM
        public async Task RemoveCartItemAsync(
            int userId,
            int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(x => x.Cart)
                .FirstOrDefaultAsync(x =>
                    x.CartItemId == cartItemId &&
                    x.Cart!.UserId == userId);

            if (cartItem == null)
            {
                throw new Exception("Cart item not found.");
            }

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();
        }


        // CLEAR CART
        public async Task ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(x => x.CartItems)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
            {
                return;
            }

            _context.CartItems.RemoveRange(cart.CartItems);

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }


        // MAP ENTITY TO DTO
        private static CartResponse MapToResponse(Cart cart)
        {
            var response = new CartResponse
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            foreach (var item in cart.CartItems)
            {
                if (item.Product == null)
                {
                    continue;
                }

                decimal subTotal =
                    item.Product.Price * item.Quantity;

                response.CartItems.Add(new CartItemResponse
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.ProductId,
                    ProductName = item.Product.ProductName,
                    SKU = item.Product.SKU,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    SubTotal = subTotal
                });

                response.TotalAmount += subTotal;
            }

            return response;
        }
    }
}