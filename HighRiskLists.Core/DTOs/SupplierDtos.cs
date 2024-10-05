namespace HighRiskLists.Core.DTOs
{
    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? TradeName { get; set; }
        public string? TaxId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Country { get; set; }
        public decimal? AnnualBillingUSD { get; set; }
    }

    public class UpdateSupplierDto : CreateSupplierDto
    {
        public int Id { get; set; }
    }

    public class SupplierDto : UpdateSupplierDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? LastEdited { get; set; }
    }
}