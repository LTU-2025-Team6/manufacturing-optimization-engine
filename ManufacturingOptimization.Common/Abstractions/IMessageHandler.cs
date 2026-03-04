namespace ManufacturingOptimization.Common.Abstractions;

/// <summary>
/// Defines a handler for processing messages of type TMessage.
/// Implementations should be registered as Scoped services.
/// </summary>
/// <typeparam name="TMessage">The type of message to handle</typeparam>
public interface IMessageHandler<in TMessage>
{
    /// <summary>
    /// Handles the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to handle</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task HandleAsync(TMessage message);
}
