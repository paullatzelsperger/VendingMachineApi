using Microsoft.EntityFrameworkCore;
using VendingMachineApi.DataAccess;
using VendingMachineApi.Models;

namespace VendingMachine.Data.DataAccess;

public class DbUserStore : IEntityStore<User>
{
    public async Task<User> Save(User entity)
    {
        if (await FindById(entity.Id) != null)
        {
            await Update(entity);
        }
        else
        {
            await using var ctx = new UserContext();
            ctx.Add(entity);
            await ctx.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<bool> Delete(User entity)
    {
        await using var ctx = new UserContext();
        var exists = false;
        if (ctx.Users.Contains(entity))
        {
            exists = true;
            ctx.Remove(entity);
        }

        await ctx.SaveChangesAsync();
        return exists;
    }

    public async Task<User?> FindByName(string name)
    {
        await using var ctx = new UserContext();
        return await ctx.Users.FirstOrDefaultAsync(u => u.Username == name);
    }

    public async Task<User?> FindById(string id)
    {
        await using var ctx = new UserContext();
        return await ctx.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<ICollection<User>> FindAll()
    {
        await using var ctx = new UserContext();
        return await ctx.Users.ToListAsync();
    }

    public async Task<bool> Update(User newUserValues)
    {
        await using var ctx = new UserContext();
        var exists = await UpdateInternal(newUserValues, ctx);

        await ctx.SaveChangesAsync();
        return exists;
    }

    private async Task<bool> UpdateInternal(User newUserValues, UserContext ctx)
    {
        var existing = ctx.Users.FirstOrDefault(u => u.Id == newUserValues.Id);
        var exists = existing != null;
        if (existing != null)
        {
            existing.Deposit = newUserValues.Deposit;
            existing.Username = newUserValues.Username;
            existing.Roles = newUserValues.Roles;
        }

        return exists;
    }
}