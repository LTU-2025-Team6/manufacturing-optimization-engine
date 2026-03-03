using Microsoft.AspNetCore.SignalR;

namespace ManufacturingOptimization.Gateway.Hubs;

public class ExecutionHub : Hub
{
    // The Gateway backend will push messages through this hub.
    // Clients (React) only need to listen, so this class can remain empty.
}