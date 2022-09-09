using System.Text.Json.Serialization;

namespace VendingMachineApi.Models;

/// <summary>
/// Model class for a user
/// </summary>
public record User : INamedEntity
{
    public string Id { get; set; }
    public string? Username { get; set; }

    [JsonIgnore] // does not need to be serialized
    public string Name
    {
        get => Username!;
        set => Username = value;
    }

    public string? Password { get; set; }
    public int? Deposit { get; set; }
    public string[] Roles { get; set;  } // todo: externalize this, users should not know about their roles
}