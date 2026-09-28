using Microsoft.EntityFrameworkCore;
using SmartIOMS.Data;

namespace SmartIOMS.Services
{
    public class ReportService
    {
        private readonly SmartIOMSDbContext _context;

        public ReportService(SmartIOMSDbContext context)
        {
            _context = context;
        }

        // LOW STOCK REPORT
        public async Task<List<object>> GetLowStockProductsAsync()
        {
            return await _context.Products
                .Include(x => x.Category)
                .Where(x =>
                    x.IsActive &&
                    x.StockQuantity <= x.ReorderLevel)
                .OrderBy(x => x.StockQuantity)
                .Select(x => (object)new
                {
                    x.ProductId,
                    x.SKU,
                    x.ProductName,
                    CategoryName = x.Category!.CategoryName,
                    x.StockQuantity,
                    x.ReorderLevel,
                    x.Price
                })
                .ToListAsync();
        }
    }
}