namespace ManufacturingOptimization.Common.Services;

// Helper class to store subscription information
internal class MessageHandler
{
    public Type MessageType { get; set; } = null!;
    public Delegate Handler { get; set; } = null!;
}
