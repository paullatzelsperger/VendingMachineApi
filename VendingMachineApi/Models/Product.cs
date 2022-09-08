namespace VendingMachineApi.Models;

public class Product : INamedEntity
{
    public string? ProductName { get; set; }
    public int? AmountAvailable { get; set; }
    public int? Cost { get; set; }
    public string? SellerId { get; set; }
    public string Id { get; set; }

    public string Name
    {
        get => ProductName!;
        set => ProductName = value;
    }
}