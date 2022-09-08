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
    private readonly IUserStore userStore;

    public UserService(IUserStore userStore)
    {
        this.userStore = userStore;
    }

    public async Task<ServiceResult<User>> Create(User user)
    {
        var existing = await userStore.FindById(user.Id);

        if (existing != null)
        {
            return ServiceResult<User>.Failure("Exists");
        }

        await userStore.Save(user);
        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<User>> Update(string userId, User user)
    {
        var existing = await userStore.FindById(userId); // use explicit ID, ignore user.Id

        if (existing != null)
        {

            existing.Deposit = user.Deposit;
            existing.Roles = user.Roles;
            // we do not update the password. there should be a separate API for resetting it
            if (await userStore.Update(existing))
            {
                return ServiceResult<User>.Success(user);
            }

            return ServiceResult<User>.Failure("Failed to update");
        }

        return ServiceResult<User>.Failure("Not Found");
    }

    public async Task<ServiceResult<User>> Delete(string userId)
    {
        var user = await userStore.FindById(userId);

        if (user != null)
        {
            if (await userStore.Delete(user))
            {
                return ServiceResult<User>.Success(user);
            }
            return ServiceResult<User>.Failure("Failed to remove");
        }

        return ServiceResult<User>.Failure("Not Found");
    }

    public async Task<ServiceResult<User>> GetByName(string username)
    {
        var user = await userStore.FindByName(username);
        var result = user != null ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("Not found");
        return result;
    }

    public async Task<ServiceResult<User>> GetById(string userId)
    {
        var user = await userStore.FindById(userId); //.FirstOrDefault(user => user.Id == userId);
        var result = ServiceResult<User>.Success(user);
        return result;
    }

    public async Task<ServiceResult<ICollection<User>>> GetAll()
    {
        return ServiceResult<ICollection<User>>.Success(await userStore.FindAll());
    }

    public async Task<ServiceResult<User>> Authenticate(string username, string password)
    {
        var user = await userStore.FindByName(username);
        if (user == null)
        {
            return ServiceResult<User>.Failure("Not Found");
        }

        var isAuth = user!.Password == password;

        return isAuth ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("Authentication failed");
    }
}