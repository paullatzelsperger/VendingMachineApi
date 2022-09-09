using VendingMachine.Model.Models;

namespace VendingMachine.Model;

public static class ModelExtensions
{
    public static UserDto AsDto(this User user)
    {
        return new UserDto(user.Id, user.Username!, user.Deposit!.Value, user.Roles);
    }
}