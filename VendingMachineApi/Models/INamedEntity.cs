namespace VendingMachineApi.Models;

public interface INamedEntity
{
    public string Id { get; }
    public string Name { get; set; }
}