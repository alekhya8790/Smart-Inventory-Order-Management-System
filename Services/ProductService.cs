using Microsoft.EntityFrameworkCore;
using SmartIOMS.Data;
using SmartIOMS.DTOs.Products;
using SmartIOMS.Models;

namespace SmartIOMS.Services
{
    public class ProductService
    {
        private readonly SmartIOMSDbContext _context;

        public ProductService(SmartIOMSDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE PRODUCT
        // =========================================================

        public async Task<ProductResponse> CreateProductAsync(
            CreateProductRequest request)
        {
            bool skuExists = await _context.Products
                .AnyAsync(x => x.SKU == request.SKU);

            if (skuExists)
            {
                throw new Exception(
                    "Product SKU already exists.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.CategoryId == request.CategoryId &&
                    x.IsActive);

            if (category == null)
            {
                throw new Exception(
                    $"Category with ID {request.CategoryId} " +
                    "was not found or is inactive.");
            }

            if (request.Price <= 0)
            {
                throw new Exception(
                    "Product price must be greater than zero.");
            }

            if (request.StockQuantity < 0)
            {
                throw new Exception(
                    "Stock quantity cannot be negative.");
            }

            if (request.ReorderLevel < 0)
            {
                throw new Exception(
                    "Reorder level cannot be negative.");
            }

            var product = new Product
            {
                SKU = request.SKU.Trim(),
                ProductName = request.ProductName.Trim(),
                Description = request.Description?.Trim(),
                CategoryId = request.CategoryId,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                ReorderLevel = request.ReorderLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return MapToResponse(
                product,
                category.CategoryName);
        }


        // =========================================================
        // GET ALL PRODUCTS
        // SEARCH + PAGINATION
        // =========================================================

        public async Task<List<ProductResponse>> GetAllProductsAsync(
            string? search,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = _context.Products
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .AsQueryable();

            // SEARCH BY PRODUCT NAME OR SKU
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.ProductName.Contains(search) ||
                    x.SKU.Contains(search));
            }

            return await query
                .OrderBy(x => x.ProductName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProductResponse
                {
                    ProductId = x.ProductId,
                    SKU = x.SKU,
                    ProductName = x.ProductName,
                    Description = x.Description,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category!.CategoryName,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    ReorderLevel = x.ReorderLevel,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();
        }


        // =========================================================
        // GET PRODUCT BY ID
        // =========================================================

        public async Task<ProductResponse?> GetProductByIdAsync(
            int id)
        {
            var product = await _context.Products
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x =>
                    x.ProductId == id &&
                    x.IsActive);

            if (product == null)
            {
                return null;
            }

            return MapToResponse(
                product,
                product.Category?.CategoryName ??
                string.Empty);
        }


        // =========================================================
        // LOW STOCK REPORT
        // =========================================================

        public async Task<List<ProductResponse>>
            GetLowStockProductsAsync()
        {
            return await _context.Products
                .Include(x => x.Category)
                .Where(x =>
                    x.IsActive &&
                    x.StockQuantity <= x.ReorderLevel)
                .OrderBy(x => x.StockQuantity)
                .Select(x => new ProductResponse
                {
                    ProductId = x.ProductId,
                    SKU = x.SKU,
                    ProductName = x.ProductName,
                    Description = x.Description,
                    CategoryId = x.CategoryId,
                    CategoryName =
                        x.Category!.CategoryName,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    ReorderLevel = x.ReorderLevel,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();
        }


        // =========================================================
        // UPDATE PRODUCT
        // =========================================================

        public async Task<ProductResponse?> UpdateProductAsync(
            int id,
            UpdateProductRequest request)
        {
            var product = await _context.Products
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x =>
                    x.ProductId == id);

            if (product == null)
            {
                return null;
            }

            bool skuExists = await _context.Products
                .AnyAsync(x =>
                    x.SKU == request.SKU &&
                    x.ProductId != id);

            if (skuExists)
            {
                throw new Exception(
                    "Product SKU already exists.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.CategoryId == request.CategoryId &&
                    x.IsActive);

            if (category == null)
            {
                throw new Exception(
                    $"Category with ID {request.CategoryId} " +
                    "was not found or inactive.");
            }

            if (request.Price <= 0)
            {
                throw new Exception(
                    "Product price must be greater than zero.");
            }

            if (request.StockQuantity < 0)
            {
                throw new Exception(
                    "Stock quantity cannot be negative.");
            }

            if (request.ReorderLevel < 0)
            {
                throw new Exception(
                    "Reorder level cannot be negative.");
            }

            product.SKU =
                request.SKU.Trim();

            product.ProductName =
                request.ProductName.Trim();

            product.Description =
                request.Description?.Trim();

            product.CategoryId =
                request.CategoryId;

            product.Price =
                request.Price;

            product.StockQuantity =
                request.StockQuantity;

            product.ReorderLevel =
                request.ReorderLevel;

            product.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(
                product,
                category.CategoryName);
        }


        // =========================================================
        // DELETE PRODUCT
        // SOFT DELETE
        // =========================================================

        public async Task<bool> DeleteProductAsync(
            int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductId == id);

            if (product == null)
            {
                return false;
            }

            product.IsActive = false;

            product.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }


        // =========================================================
        // COMMON RESPONSE MAPPING
        // =========================================================

        private static ProductResponse MapToResponse(
            Product product,
            string categoryName)
        {
            return new ProductResponse
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ReorderLevel = product.ReorderLevel,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}