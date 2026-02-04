namespace ManufacturingOptimization.Common.Messaging.Abstractions;

public interface IMessageSubscriber
{
    void Subscribe<T>(string queueName, Action<T> handler) where T : IMessage;
    void Unsubscribe(string queueName);
}
