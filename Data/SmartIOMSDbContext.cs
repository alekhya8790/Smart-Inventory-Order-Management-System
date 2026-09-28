using Microsoft.EntityFrameworkCore;
using SmartIOMS.Models;

namespace SmartIOMS.Data
{
    public class SmartIOMSDbContext : DbContext
    {
        public SmartIOMSDbContext(DbContextOptions<SmartIOMSDbContext> options)
            : base(options)
        {
        }

        // Database Tables
        public DbSet<User> Users { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ==========================================
            // USER
            // ==========================================

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.UserId);

                entity.Property(x => x.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.PhoneNumber)
                    .HasMaxLength(15);

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasMaxLength(20)
                    .HasDefaultValue("Customer");

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.ToTable("Users", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Users_Role",
                        "[Role] IN ('Customer', 'Admin')");
                });
            });


            // ==========================================
            // CATEGORY
            // ==========================================

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.CategoryId);

                entity.Property(x => x.CategoryName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });


            // ==========================================
            // PRODUCT
            // ==========================================

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.ProductId);

                entity.Property(x => x.SKU)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(x => x.SKU)
                    .IsUnique();

                entity.Property(x => x.ProductName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.Price)
                    .HasPrecision(18, 2);

                entity.Property(x => x.StockQuantity)
                    .IsRequired();

                entity.Property(x => x.ReorderLevel)
                    .HasDefaultValue(5);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(x => x.UpdatedAt);

                // SQL Server RowVersion
                entity.Property(x => x.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                // Product must belong to Category
                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Products)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Useful indexes
                entity.HasIndex(x => x.CategoryId);

                entity.HasIndex(x => x.ProductName);

                entity.HasIndex(x => x.IsActive);

                // Stock can never be negative
                entity.ToTable("Products", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Products_StockQuantity",
                        "[StockQuantity] >= 0");

                    table.HasCheckConstraint(
                        "CK_Products_Price",
                        "[Price] > 0");

                    table.HasCheckConstraint(
                        "CK_Products_ReorderLevel",
                        "[ReorderLevel] >= 0");
                });
            });


            // ==========================================
            // CART
            // ==========================================

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(x => x.CartId);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // One User -> One Cart
                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<Cart>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // UserId must be unique
                entity.HasIndex(x => x.UserId)
                    .IsUnique();
            });


            // ==========================================
            // CART ITEM
            // ==========================================

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(x => x.CartItemId);

                entity.Property(x => x.Quantity)
                    .IsRequired();

                entity.Property(x => x.AddedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Cart -> CartItems
                entity.HasOne(x => x.Cart)
                    .WithMany(x => x.CartItems)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Product -> CartItems
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.CartItems)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Same product cannot appear twice in same cart
                entity.HasIndex(x => new
                {
                    x.CartId,
                    x.ProductId
                })
                .IsUnique();

                entity.ToTable("CartItems", table =>
                {
                    table.HasCheckConstraint(
                        "CK_CartItems_Quantity",
                        "[Quantity] > 0");
                });
            });


            // ==========================================
            // ORDER
            // ==========================================

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.OrderId);

                entity.Property(x => x.OrderNumber)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.HasIndex(x => x.OrderNumber)
                    .IsUnique();

                entity.Property(x => x.TotalAmount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                entity.Property(x => x.ShippingAddress)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.City)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.State)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Pincode)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.PaymentMethod)
                    .HasMaxLength(30)
                    .HasDefaultValue("CashOnDelivery");

                entity.Property(x => x.PaymentStatus)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                entity.Property(x => x.OrderedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // User -> Orders
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId);

                entity.HasIndex(x => x.Status);

                entity.ToTable("Orders", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Orders_TotalAmount",
                        "[TotalAmount] >= 0");

                    table.HasCheckConstraint(
                        "CK_Orders_Status",
                        "[Status] IN ('Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled')");

                    table.HasCheckConstraint(
                        "CK_Orders_PaymentMethod",
                        "[PaymentMethod] IN ('CashOnDelivery', 'UPI', 'Card')");

                    table.HasCheckConstraint(
                        "CK_Orders_PaymentStatus",
                        "[PaymentStatus] IN ('Pending', 'Paid', 'Failed', 'Refunded')");
                });
            });


            // ==========================================
            // ORDER ITEM
            // ==========================================

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(x => x.OrderItemId);

                entity.Property(x => x.ProductName)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.SKU)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.UnitPrice)
                    .HasPrecision(18, 2);

                entity.Property(x => x.SubTotal)
                    .HasPrecision(18, 2);

                // Order -> OrderItems
                entity.HasOne(x => x.Order)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Product -> OrderItems
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.OrderId);

                entity.ToTable("OrderItems", table =>
                {
                    table.HasCheckConstraint(
                        "CK_OrderItems_Quantity",
                        "[Quantity] > 0");

                    table.HasCheckConstraint(
                        "CK_OrderItems_UnitPrice",
                        "[UnitPrice] >= 0");

                    table.HasCheckConstraint(
                        "CK_OrderItems_SubTotal",
                        "[SubTotal] >= 0");
                });
            });


            // ==========================================
            // INVENTORY TRANSACTION
            // ==========================================

            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(x => x.InventoryTransactionId);

                entity.Property(x => x.TransactionType)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.Remarks)
                    .HasMaxLength(500);

                entity.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Product -> Inventory Transactions
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.InventoryTransactions)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ProductId);

                entity.ToTable("InventoryTransactions", table =>
                {
                    table.HasCheckConstraint(
                        "CK_InventoryTransactions_Type",
                        "[TransactionType] IN ('StockIn', 'Order', 'Cancellation', 'Adjustment')");

                    table.HasCheckConstraint(
                        "CK_InventoryTransactions_Quantity",
                        "[Quantity] > 0");
                });
            });
        }
    }
}