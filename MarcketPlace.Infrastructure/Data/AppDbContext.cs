using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Vendor> Vendors { get; set; } = default!;
        public DbSet<Driver> Drivers { get; set; } = default!;
        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<OtpCode> OtpCodes { get; set; } = default!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = default!;
        public DbSet<DeliveryZone> DeliveryZones { get; set; } = default!;
        public DbSet<Store> Stores { get; set; } = default!;
        public DbSet<MarketWorkingHour> MarketWorkingHours { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<CartItem> CartItems { get; set; } = default!;
        public DbSet<CustomerFavorite> CustomerFavorites { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> OrderItems { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<StoreRating> StoreRatings { get; set; } = default!;
        public DbSet<DriverRating> DriverRatings { get; set; } = default!;
        public DbSet<ProductRequest> ProductRequests { get; set; }
        public DbSet<ProductUnit> ProductUnits { get; set; } = default!;

        public DbSet<ProductOption> ProductOptions { get; set; } = default!;

        public DbSet<ProductOptionValue> ProductOptionValues { get; set; } = default!;

        public DbSet<ProductVariant> ProductVariants { get; set; } = default!;

        public DbSet<ProductVariantOptionValue> ProductVariantOptionValues { get; set; } = default!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}