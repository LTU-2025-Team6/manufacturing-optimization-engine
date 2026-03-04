using Google.OrTools.LinearSolver;

namespace ManufacturingOptimization.Engine.Models;

public class MipVariables
{
    /// <summary>
    /// Binary assignment variables x[step, provider, slot].
    /// x[i,j,k] = 1 if provider j with slot k is selected for step i.
    /// </summary>
    public Dictionary<(int step, int provider, int slot), Variable> Assignments { get; } = new();
        
    /// <summary>
    /// Continuous time variables for process start times (in hours from reference).
    /// </summary>
    public Dictionary<int, Variable> StartTimes { get; } = new();
        
    /// <summary>
    /// Continuous time variables for process end times (in hours from reference).
    /// </summary>
    public Dictionary<int, Variable> EndTimes { get; } = new();
}
