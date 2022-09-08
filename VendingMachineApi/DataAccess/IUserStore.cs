using VendingMachineApi.Models;

namespace VendingMachineApi.DataAccess;

public interface IUserStore
{
    Task<User> Save(User user);
    Task<bool> Delete(User actualUser);
    Task<User?> FindByName(string username);
    Task<User?> FindById(string userId);
    Task<ICollection<User>> FindAll();
    Task<bool> Update(User existingUser);
}

internal class InMemoryUserStore : IUserStore
{
    private readonly ICollection<User> store;

    public InMemoryUserStore()
    {
        store = new List<User>();
    }

    public Task<User> Save(User user)
    {
        store.Add(user);
        return Task.FromResult(user);
    }

    public Task<bool> Delete(User actualUser)
    {
        return Task.FromResult(store.Remove(actualUser));
    }

    public Task<User?> FindByName(string username)
    {
        return Task.FromResult(store.FirstOrDefault(u => u.Username.Equals(username)));
    }

    public Task<User?> FindById(string userId)
    {
        return Task.FromResult(store.FirstOrDefault(u => u.Id.Equals(userId)));
    }

    public Task<ICollection<User>> FindAll()
    {
        return Task.FromResult(store);
    }

    public Task<bool> Update(User existingUser)
    {
        var removed = store.Remove(existingUser);
        if (removed)
            store.Add(existingUser);
        return Task.FromResult(removed);
    }
}