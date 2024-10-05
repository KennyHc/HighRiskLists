namespace HighRiskLists.Core.Entities;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }  // Changed from number to string to accommodate different formats
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Country { get; set; }
    public decimal? AnnualBillingUSD { get; set; }
    public DateTime? LastEdited { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}