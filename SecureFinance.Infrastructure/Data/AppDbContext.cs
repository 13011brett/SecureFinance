using Microsoft.EntityFrameworkCore;
using SecureFinance.Core.Models;

namespace SecureFinance.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<DataSource> DataSources { get; set; }
        public DbSet<ApiRequest> ApiRequests { get; set; }
        public DbSet<EncryptionKey> EncryptionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.ApiKey).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(64);
            });

            // DataSource configuration
            modelBuilder.Entity<DataSource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(200);
            });

            // ApiRequest configuration
            modelBuilder.Entity<ApiRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RequestedAt);
                entity.HasIndex(e => new { e.UserId, e.RequestedAt });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ApiRequests)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.DataSource)
                    .WithMany(ds => ds.ApiRequests)
                    .HasForeignKey(e => e.DataSourceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // EncryptionKey configuration
            modelBuilder.Entity<EncryptionKey>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.KeyIdentifier).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.IsActive });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.EncryptionKeys)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataSource>().HasData(
                new DataSource
                {
                    Id = 1,
                    Name = "Alpha Vantage",
                    BaseUrl = "https://www.alphavantage.co/query",
                    RateLimit = 5, // 5 requests per minute
                    IsActive = true
                },
                new DataSource
                {
                    Id = 2,
                    Name = "CoinGecko",
                    BaseUrl = "https://api.coingecko.com/api/v3",
                    RateLimit = 50, // 50 requests per minute
                    IsActive = true
                },
                new DataSource
                {
                    Id = 3,
                    Name = "FRED",
                    BaseUrl = "https://api.stlouisfed.org/fred",
                    RateLimit = 120, // 120 requests per minute
                    IsActive = true
                },
                new DataSource
                {
                    Id = 4,
                    Name = "Exchange Rates API",
                    BaseUrl = "https://api.exchangerate-api.com/v4",
                    RateLimit = 1500, // 1500 requests per month free tier
                    IsActive = true
                }
            );
        }
    }
}