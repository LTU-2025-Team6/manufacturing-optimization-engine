namespace ManufacturingOptimization.Common.Abstractions;

public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified exchange with the given routing key.
    /// </summary>
    /// <remarks>This method sends the specified message to the given exchange using the provided routing key.
    /// Ensure that the exchange and routing key are correctly configured in the messaging system to  route the message
    /// to the intended destination.</remarks>
    /// <typeparam name="T">The type of the message to be published. Must implement the <see cref="IMessage"/> interface.</typeparam>
    /// <param name="exchangeName">The name of the exchange to which the message will be published. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="routingKey">The routing key used to route the message to the appropriate queue. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="message">The message to be published. Cannot be <see langword="null"/>.</param>
    void Publish<T>(string exchangeName, string routingKey, T message) where T : IMessage;
}
