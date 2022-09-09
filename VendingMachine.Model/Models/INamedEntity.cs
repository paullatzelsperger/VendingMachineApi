namespace VendingMachine.Model.Models;

/// <summary>
/// Interface for all persistent entities
/// </summary>
public interface INamedEntity
{
    /// <summary>
    /// Machine-readable Identifier. Presumed unique.
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// Human readable name. Presumed unique
    /// </summary>
    public string Name { get; set; }
}