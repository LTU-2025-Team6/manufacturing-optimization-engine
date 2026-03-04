namespace ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;

/// <summary>
/// Response when cancelling a plan.
/// </summary>
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
