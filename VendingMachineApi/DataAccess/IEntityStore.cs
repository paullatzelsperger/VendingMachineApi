using VendingMachineApi.Models;

namespace VendingMachineApi.DataAccess;

public interface IEntityStore<TEntity> 
{
    Task<TEntity> Save(TEntity entity);
    Task<bool> Delete(TEntity entity);
    Task<TEntity?> FindByName(string name);
    Task<TEntity?> FindById(string id);
    Task<ICollection<TEntity>> FindAll();
    Task<bool> Update(TEntity existing);
}

internal class InMemoryEntityStore<TEntity> : IEntityStore<TEntity> where TEntity : INamedEntity
{
    private readonly ICollection<TEntity> store;

    public InMemoryEntityStore()
    {
        store = new List<TEntity>();
    }

    public Task<TEntity> Save(TEntity entity)
    {
        store.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<bool> Delete(TEntity entity)
    {
        return Task.FromResult(store.Remove(entity));
    }

    public Task<TEntity?> FindByName(string name)
    {
        return Task.FromResult(store.FirstOrDefault(u => u.Name.Equals(name)));
    }

    public Task<TEntity?> FindById(string id)
    {
        return Task.FromResult(store.FirstOrDefault(u => u.Id.Equals(id)));
    }

    public Task<ICollection<TEntity>> FindAll()
    {
        return Task.FromResult(store);
    }

    public Task<bool> Update(TEntity existing)
    {
        var removed = store.Remove(existing);
        if (removed)
            store.Add(existing);
        return Task.FromResult(removed);
    }
}