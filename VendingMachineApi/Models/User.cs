using System.Text.Json.Serialization;

namespace VendingMachineApi.Models;

public record User : INamedEntity
{
    public string Id { get; set; }
    public string? Username { get; set; }

    [JsonIgnore]
    public string Name
    {
        get => Username!;
        set => Username = value;
    }

    public string? Password { get; set; }
    public int? Deposit { get; set; }
    public string[] Roles { get; set;  } // todo: externalize this, users need not know about their roles
}