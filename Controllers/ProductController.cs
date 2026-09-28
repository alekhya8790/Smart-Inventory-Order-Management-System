using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIOMS.DTOs.Products;
using SmartIOMS.Services;

namespace SmartIOMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // =========================================================
        // CREATE PRODUCT
        // POST: api/Product
        // ADMIN ONLY
        // =========================================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductRequest request)
        {
            var product =
                await _productService.CreateProductAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.ProductId },
                product);
        }


        // =========================================================
        // GET ALL PRODUCTS
        // SEARCH + PAGINATION
        // GET: api/Product
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var products =
                await _productService.GetAllProductsAsync(
                    search,
                    page,
                    pageSize);

            return Ok(products);
        }


        // =========================================================
        // LOW STOCK REPORT
        // GET: api/Product/low-stock
        // ADMIN ONLY
        // =========================================================

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            var products =
                await _productService.GetLowStockProductsAsync();

            return Ok(products);
        }


        // =========================================================
        // GET PRODUCT BY ID
        // GET: api/Product/{id}
        // =========================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            [FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Product ID is required."
                });
            }

            var product =
                await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            return Ok(product);
        }


        // =========================================================
        // UPDATE PRODUCT
        // PUT: api/Product/{id}
        // ADMIN ONLY
        // =========================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            [FromRoute] int id,
            [FromBody] UpdateProductRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Product ID is required."
                });
            }

            var product =
                await _productService.UpdateProductAsync(
                    id,
                    request);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            return Ok(product);
        }


        // =========================================================
        // DELETE PRODUCT
        // DELETE: api/Product/{id}
        // ADMIN ONLY
        // =========================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(
            [FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Product ID is required."
                });
            }

            bool deleted =
                await _productService.DeleteProductAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            return Ok(new
            {
                message = "Product deleted successfully."
            });
        }
    }
}