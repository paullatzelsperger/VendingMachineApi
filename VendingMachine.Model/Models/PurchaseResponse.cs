namespace VendingMachineApi.Models;

/// <summary>
/// Response object when buying a <see cref="Product"/> from a vending machine.
/// </summary>
public class PurchaseResponse
{
    public int? TotalAmountSpent { get; set; }
    public Product Product { get; set; }
    public Coin[] Change { get; set; }
}