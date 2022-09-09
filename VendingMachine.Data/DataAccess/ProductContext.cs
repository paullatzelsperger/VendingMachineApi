using Microsoft.EntityFrameworkCore;
using VendingMachine.Model.Models;

namespace VendingMachine.Data.DataAccess;

internal class ProductContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseInMemoryDatabase("product-db");
}