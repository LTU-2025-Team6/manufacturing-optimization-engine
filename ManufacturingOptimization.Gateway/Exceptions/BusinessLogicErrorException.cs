namespace ManufacturingOptimization.Gateway.Exceptions
{
    public class BusinessLogicErrorException : GatewayException
    {
        public BusinessLogicErrorException(string message) 
            : base(message, 500)
        {
        }
    }
}
