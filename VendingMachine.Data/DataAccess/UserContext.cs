using Microsoft.EntityFrameworkCore;
using VendingMachineApi.Models;

namespace VendingMachine.Data.DataAccess;

internal class UserContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseInMemoryDatabase("user-db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        // EF Core cannot map primitive types, so we need to encode/decode them when storing/loading
        modelBuilder.Entity<User>()
                    .Property(u => u.Roles)
                    .HasConversion(v => string.Join(",", v),
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries));
        
        modelBuilder.Entity<User>(e => e.HasKey(u => u.Id));
    }

    
}