using Microsoft.EntityFrameworkCore;
using StockReplenishment.Models;

namespace StockReplenishment.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ReplenishmentRequest> Requests => Set<ReplenishmentRequest>();
    public DbSet<RequestItem> Items => Set<RequestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReplenishmentRequest>()
            .HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.ReplenishmentRequestId)
            .OnDelete(DeleteBehavior.Cascade);
            
        base.OnModelCreating(modelBuilder);
    }
}