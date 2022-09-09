using VendingMachineApi.Models;

namespace VendingMachineApi.DataAccess;

/// <summary>
/// Interface for data retention.
/// </summary>
/// <typeparam name="TEntity">Type of the entity to store</typeparam>
public interface IEntityStore<TEntity>
{
    /// <summary>
    /// Saves an entity in the store. If the entity already exists, it will be updated, otherwise it will be added.
    /// 
    /// </summary>
    /// <param name="entity">the entity to save</param>
    /// <returns>The updated/added entity.</returns>
    Task<TEntity> Save(TEntity entity);

    /// <summary>
    /// Removes the given entity from storage. Noop if the entity does not exist
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>true if the entity was deleted, false otherwise</returns>
    Task<bool> Delete(TEntity entity);
    /// <summary>
    /// Finds an entity based on its name. Names are presumed unique.
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>null if not found.</returns>
    Task<TEntity?> FindByName(string name);
    /// <summary>
    /// Finds an entity based on its ID. IDs are presumed unique.
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>null if not found</returns>
    Task<TEntity?> FindById(string id);
    /// <summary>
    /// Gets all entities currently in the store
    /// </summary>
    /// <returns></returns>
    Task<ICollection<TEntity>> FindAll();
    /// <summary>
    /// Updates an entity if it exists. Does nothing if the entity does not exist. It is
    /// up to the implementor whether the update happens in-place (e.g. SQL UPDATE) or through
    /// remove-add (e.g. in memory collections).
    /// </summary>
    /// <param name="newUserValues">The entity to update</param>
    /// <returns>true if updated, false otherwise.</returns>
    Task<bool> Update(TEntity newUserValues);
}

internal class InMemoryEntityStore<TEntity> : IEntityStore<TEntity> where TEntity : INamedEntity
{
    private readonly ICollection<TEntity> store;

    public InMemoryEntityStore()
    {
        store = new List<TEntity>();
    }

    public async Task<TEntity> Save(TEntity entity)
    {
        if (await FindById(entity.Id) != null)
        {
            await Update(entity);
        }
        else
        {
            store.Add(entity);
        }
        return entity;
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

    public Task<bool> Update(TEntity newUserValues)
    {
        var removed = store.Remove(newUserValues);
        if (removed)
            store.Add(newUserValues);
        return Task.FromResult(removed);
    }
}