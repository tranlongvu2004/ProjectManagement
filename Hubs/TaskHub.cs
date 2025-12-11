using Microsoft.AspNetCore.SignalR;

public class TaskHub : Hub
{
    public async Task NotifyTaskChange()
    {
        await Clients.All.SendAsync("TaskUpdated");
    }
}
