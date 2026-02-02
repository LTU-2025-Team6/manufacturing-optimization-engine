namespace ManufacturingOptimization.Common.Models.DTOs;

public class OptimizationPlanDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public List<OptimizationStrategyDto> Strategies { get; set; } = [];
    public OptimizationStrategyDto? SelectedStrategy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SelectedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
