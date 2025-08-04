using Microsoft.EntityFrameworkCore;

using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentTransaction> Transactions { get; set; }
    public DbSet<TransactionEvent> TransactionEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.ClientRequestId)
            .IsUnique()
            .HasFilter("\"ClientRequestId\" IS NOT NULL");

        modelBuilder.Entity<TransactionEvent>()
            .HasOne(e => e.Transaction)
            .WithMany()
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TransactionEvent>()
            .HasIndex(e => e.TransactionId);
        
        base.OnModelCreating(modelBuilder);
    }
}