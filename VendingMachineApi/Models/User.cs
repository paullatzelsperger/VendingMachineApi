namespace VendingMachineApi.Models;

public class User : INamedEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Password { get; set; }
    public int? Deposit { get; set; }
    public string[] Roles { get; set;  } // todo: externalize this, users need not know about their roles
}