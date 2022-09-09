namespace VendingMachine.Model.Models;

/// <summary>
/// Represents a stack of coins that can be put into or output by a vending machine. Has a value (e.g. "20 cents") and an
/// amount
/// </summary>
public record Coin
{
    public string Value { get; set; }
    public int Amount { get; set; }
}