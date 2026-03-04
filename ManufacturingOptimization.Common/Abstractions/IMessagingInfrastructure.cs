namespace ManufacturingOptimization.Common.Abstractions;

public interface IMessagingInfrastructure
{
    void DeclareExchange(string exchangeName, string exchangeType = "topic");
    void DeclareQueue(string queueName);
    void BindQueue(string queueName, string exchangeName, string routingKey);
    void PurgeQueue(string queueName);
    void DeleteQueue(string queueName);
}
