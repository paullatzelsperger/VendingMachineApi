namespace VendingMachineApi.Models;

/// <summary>
/// Value object to hold user information excluding the password. It is intended for the read and update requests of the user API.
/// </summary>
public record UserDto(string Id, string Username, int Deposit, string[] Roles);