namespace VendingMachineApi.Models;

public class PurchaseResponse
{
    public int? TotalAmountSpent { get; set; }
    public Product Product { get; set; }
    public Coin[] Change { get; set; }
}