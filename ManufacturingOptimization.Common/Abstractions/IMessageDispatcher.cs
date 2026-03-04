namespace ManufacturingOptimization.Common.Abstractions;

/// <summary>
/// Dispatches messages to their registered handlers using scoped service resolution.
/// </summary>
public interface IMessageDispatcher
{
    /// <summary>
    /// Dispatches a message to its registered handler within a new service scope.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to dispatch</typeparam>
    /// <param name="message">The message to dispatch</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DispatchAsync<TMessage>(TMessage message);
}
