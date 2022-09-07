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

internal class UserService : IUserService
{
    //todo: convert this to a persistent storage, e.g. using EF Core
    private readonly ICollection<User> userStorage;

    public UserService()
    {
        userStorage = new List<User>();
    }

    public async Task<ServiceResult<User>> Create(User user)
    {
        var existing = await GetById(user.Id);

        if (existing.Succeeded)
        {
            return ServiceResult<User>.Failure("Exists");
        }

        userStorage.Add(user);
        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<User>> Update(string userId, User user)
    {
        var existing = await GetById(userId); // use explicit ID, ignore user.Id

        if (existing.Succeeded)
        {
            var existingUser = existing.Content!;
            
            existingUser.Deposit = user.Deposit;
            existingUser.Roles = user.Roles;
            // we do not update the password. there should be a separate API for resetting it
            
            return ServiceResult<User>.Success(user);
        }

        return ServiceResult<User>.Failure("Not Found");
    }

    public async Task<ServiceResult<User>> Delete(string userId)
    {
        var result = await GetById(userId);

        if (result.Succeeded)
        {
            var actualUser = result.Content!;
            if (userStorage.Remove(actualUser))
            {
                return ServiceResult<User>.Success(actualUser);
            }
        }

        return ServiceResult<User>.Failure("Failed to remove");
    }

    public Task<ServiceResult<User>> GetByName(string username)
    {
        var user = userStorage.FirstOrDefault(user => user.Username == username);
        var result = user == null ? ServiceResult<User>.Failure("Not found") : ServiceResult<User>.Success(user);
        return Task.FromResult(result);
    }

    public Task<ServiceResult<User>> GetById(string userId)
    {
        var user = userStorage.FirstOrDefault(user => user.Id == userId);
        var result = user == null ? ServiceResult<User>.Failure("Not found") : ServiceResult<User>.Success(user);
        return Task.FromResult(result);
    }

    public Task<ServiceResult<ICollection<User>>> GetAll()
    {
        return Task.FromResult(ServiceResult<ICollection<User>>.Success(userStorage));
    }

    public Task<ServiceResult<User>> Authenticate(string username, string password)
    {
        var user = userStorage.FirstOrDefault(user => user.Username == username);
        if (user == null)
        {
            return Task.FromResult(ServiceResult<User>.Failure("Not Found"));
        }

        var isAuth = user.Password == password;

        return Task.FromResult(isAuth ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("Not Authenticated"));
    }
}