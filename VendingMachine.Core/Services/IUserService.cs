using VendingMachine.Core.DataAccess;
using VendingMachine.Model;

namespace VendingMachine.Core.Services;

public interface IUserService
{
    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="user">The user to create anew.</param>
    /// <returns>A failed result if the user exists, a successful result containing the user otherwise.</returns>
    Task<ServiceResult<User>> Create(User user);

    /// <summary>
    /// Updates a user identified by the given ID. Note that the ID property of the <paramref name="user"></paramref>
    /// is ignored.
    /// </summary>
    /// <param name="userId">ID of the user who is to be updated.</param>
    /// <param name="user">A user model object containing the new values. Note that the entire user object
    ///     is replaced.</param>
    /// <returns>A failed result if the user does not exist, a successful result containing the user otherwise.</returns>
    Task<ServiceResult<User>> Update(string userId, UserDto user);

    /// <summary>
    /// Deletes a user with the given ID
    /// </summary>
    /// <param name="userId">The ID of the user to delete</param>
    /// <returns>A failed result if the user does not exist.</returns>
    Task<ServiceResult<User>> Delete(string userId);

    /// <summary>
    /// Finds a user by ID.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>A failed result if the user does not exist</returns>
    Task<ServiceResult<User>> GetById(string userId);

    /// <summary>
    /// Returns a collection of all currently available users.
    /// </summary>
    Task<ServiceResult<ICollection<User>>> GetAll();

    /// <summary>
    /// Authenticates a user using its username and password.
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The password</param>
    /// <returns>A failed result if the authentication failed, or the user was not found, the user object otherwise.</returns>
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

    public async Task<ServiceResult<User>> Update(string userId, UserDto user)
    {
        var existing = await entityStore.FindById(userId); // use explicit ID, ignore user.Id

        if (existing != null)
        {
            existing.Deposit = user.Deposit;
            existing.Roles = user.Roles;
            existing.Username = user.Username;
            // we do not update the password. there should be a separate API for resetting it
            if (await entityStore.Update(existing))
            {
                return ServiceResult<User>.Success(existing);
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

    public async Task<ServiceResult<User>> GetById(string userId)
    {
        var user = await entityStore.FindById(userId);
        var result = user != null ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("Not found");
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