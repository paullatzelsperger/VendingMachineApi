using VendingMachineApi.DataAccess;
using VendingMachineApi.Models;

namespace VendingMachineApi.Services;

public interface IUserService
{
    Task<ServiceResult<User>> Create(User user);
    Task<ServiceResult<User>> Update(string userId, User user);
    Task<ServiceResult<User>> Delete(string userId);
    Task<ServiceResult<User>> GetByName(string username);
    Task<ServiceResult<User>> GetById(string userId);
    Task<ServiceResult<ICollection<User>>> GetAll();
    Task<ServiceResult<User>> Authenticate(string username, string password);
}

public class UserService : IUserService
{
    //todo: convert this to a persistent storage, e.g. using EF Core
    private readonly IEntityStore<User> entityStore;

    public UserService(IEntityStore<User> entityStore)
    {
        this.entityStore = entityStore;
    }

    public async Task<ServiceResult<User>> Create(User user)
    {
        var existing = await entityStore.FindById(user.Id);

        if (existing != null)
        {
            return ServiceResult<User>.Failure("Exists");
        }

        await entityStore.Save(user);
        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<User>> Update(string userId, User user)
    {
        var existing = await entityStore.FindById(userId); // use explicit ID, ignore user.Id

        if (existing != null)
        {

            existing.Deposit = user.Deposit;
            existing.Roles = user.Roles;
            // we do not update the password. there should be a separate API for resetting it
            if (await entityStore.Update(existing))
            {
                return ServiceResult<User>.Success(user);
            }

            return ServiceResult<User>.Failure("Failed to update");
        }

        return ServiceResult<User>.Failure("Not Found");
    }

    public async Task<ServiceResult<User>> Delete(string userId)
    {
        var user = await entityStore.FindById(userId);

        if (user != null)
        {
            if (await entityStore.Delete(user))
            {
                return ServiceResult<User>.Success(user);
            }
            return ServiceResult<User>.Failure("Failed to remove");
        }

        return ServiceResult<User>.Failure("Not Found");
    }

    public async Task<ServiceResult<User>> GetByName(string username)
    {
        var user = await entityStore.FindByName(username);
        var result = user != null ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("Not found");
        return result;
    }

    public async Task<ServiceResult<User>> GetById(string userId)
    {
        var user = await entityStore.FindById(userId); 
        var result = user != null ?ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("Not found");
        return result;
    }

    public async Task<ServiceResult<ICollection<User>>> GetAll()
    {
        return ServiceResult<ICollection<User>>.Success(await entityStore.FindAll());
    }

    public async Task<ServiceResult<User>> Authenticate(string username, string password)
    {
        var user = await entityStore.FindByName(username);
        if (user == null)
        {
            return ServiceResult<User>.Failure("Not Found");
        }

        var isAuth = user!.Password == password;

        return isAuth ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("Authentication failed");
    }
}