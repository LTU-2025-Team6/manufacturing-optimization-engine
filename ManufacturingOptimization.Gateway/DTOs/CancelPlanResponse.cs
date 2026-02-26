namespace ManufacturingOptimization.Gateway.DTOs;

public class CancelPlanResponse
{
    public OptimizationPlanDto Plan { get; set; }
    public List<string> Errors { get; set; }

    public CancelPlanResponse(OptimizationPlanDto plan, List<string>? errors = null)
    {
        Plan = plan;
        Errors = errors ?? new List<string>();
    }

    public bool IsSuccess => !Errors.Any();
}
