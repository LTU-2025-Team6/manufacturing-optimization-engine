namespace ManufacturingOptimization.Common.Models.Contracts;

public class WarrantyTermsModel
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IncludesInsurance { get; set; }
}
