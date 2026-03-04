namespace ManufacturingOptimization.Common.Enums;

/// <summary>
/// Standard manufacturing and remanufacturing process types.
/// Ensures consistency across the system.
/// </summary>
public enum ProcessType
{
    /// <summary>
    /// Remove dirt, oil, and contaminants from motor components.
    /// </summary>
    Cleaning,
    
    /// <summary>
    /// Take motor completely apart into individual components.
    /// </summary>
    Disassembly,
    
    /// <summary>
    /// Replace worn or damaged parts with new/refurbished components.
    /// </summary>
    PartSubstitution,
    
    /// <summary>
    /// Reassemble motor components back together.
    /// </summary>
    Reassembly,
    
    /// <summary>
    /// Test motor for efficiency compliance and quality standards.
    /// </summary>
    Certification,
    
    /// <summary>
    /// Engineer improved components for motor upgrade.
    /// </summary>
    Redesign,
    
    /// <summary>
    /// Machine parts on lathe for precision dimensions.
    /// </summary>
    Turning,
    
    /// <summary>
    /// Precision surface finishing and grinding.
    /// </summary>
    Grinding
}

/// <summary>
/// Extension methods for ProcessType enum.
/// </summary>
public static class ProcessTypeExtensions
{
    /// <summary>
    /// Get display name for UI.
    /// </summary>
    public static string GetDisplayName(this ProcessType processType)
    {
        return processType switch
        {
            ProcessType.Cleaning => "Cleaning",
            ProcessType.Disassembly => "Disassembly",
            ProcessType.PartSubstitution => "PartSubstitution",
            ProcessType.Reassembly => "Reassembly",
            ProcessType.Certification => "Certification",
            ProcessType.Redesign => "Redesign",
            ProcessType.Turning => "Turning",
            ProcessType.Grinding => "Grinding",
            _ => processType.ToString()
        };
    }
    
    /// <summary>
    /// Get description for process.
    /// </summary>
    public static string GetDescription(this ProcessType processType)
    {
        return processType switch
        {
            ProcessType.Cleaning => "Remove dirt and contaminants",
            ProcessType.Disassembly => "Take motor apart",
            ProcessType.PartSubstitution => "Replace worn parts",
            ProcessType.Reassembly => "Put motor back together",
            ProcessType.Certification => "Test for compliance",
            ProcessType.Redesign => "Engineer improved components",
            ProcessType.Turning => "Machine parts on lathe",
            ProcessType.Grinding => "Precision surface finishing",
            _ => string.Empty
        };
    }
    
    /// <summary>
    /// Parse process type from string (case-insensitive).
    /// </summary>
    public static ProcessType Parse(string value)
    {
        // Try exact match first
        if (Enum.TryParse<ProcessType>(value, ignoreCase: true, out var result))
        {
            return result;
        }
        
        // Handle display names
        return value.Trim().Replace(" ", "") switch
        {
            "PartReplacement" => ProcessType.PartSubstitution,
            _ => throw new ArgumentException($"Unknown process type: {value}")
        };
    }
}
