using Microsoft.EntityFrameworkCore;
using VendingMachine.Core.DataAccess;
using VendingMachine.Model;

namespace VendingMachine.Data.DataAccess;

public class DbProductStore : IEntityStore<Product>
{
    public async Task<Product> Save(Product entity)
    {
        await using var ctx = new ProductContext();

        if (ctx.Products.Contains(entity))
        {
            await UpdateInternal(entity, ctx);
        }
        else
        {
            ctx.Add(entity);
        }

        await ctx.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> Delete(Product entity)
    {
        await using var ctx = new ProductContext();
        var exists = false;
        if (ctx.Products.Contains(entity))
        {
            exists = true;
            ctx.Remove(entity);
        }

        await ctx.SaveChangesAsync();
        return exists;
    }

    public async Task<Product?> FindByName(string name)
    {
        await using var ctx = new ProductContext();
        return await ctx.Products.FirstOrDefaultAsync(u => u.Name == name);
    }

    public async Task<Product?> FindById(string id)
    {
        await using var ctx = new ProductContext();
        return await ctx.Products.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<ICollection<Product>> FindAll()
    {
        await using var ctx = new ProductContext();
        return await ctx.Products.ToListAsync();
    }

    public async Task<bool> Update(Product newProductValues)
    {
        await using var ctx = new ProductContext();
        var exists = await UpdateInternal(newProductValues, ctx);

        await ctx.SaveChangesAsync();
        return exists;
    }

    private async Task<bool> UpdateInternal(Product newProductValues, DbContext ctx)
    {
        var existing = await FindById(newProductValues.Id);
        var exists = existing != null;
        if (existing != null)
        {
            existing.Cost = newProductValues.Cost;
            existing.ProductName = newProductValues.ProductName;
            existing.AmountAvailable = newProductValues.AmountAvailable;
            ctx.Update(existing);
        }

        return exists;
    }
}