using Microsoft.EntityFrameworkCore;
using VendingMachineApi.Models;

namespace VendingMachineApi.DataAccess;

public class ProductContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseInMemoryDatabase("product-db");
}