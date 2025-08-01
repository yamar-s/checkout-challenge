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
}