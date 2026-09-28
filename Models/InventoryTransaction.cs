namespace SmartIOMS.Models
{
    public class InventoryTransaction
    {
        public int InventoryTransactionId { get; set; }

        public int ProductId { get; set; }

        public string TransactionType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int? ReferenceId { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Product? Product { get; set; }
    }
}
