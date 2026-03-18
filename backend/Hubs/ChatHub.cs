using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace backend.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinBusinessChannel(string businessNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, businessNumber);
    }

    public async Task LeaveBusinessChannel(string businessNumber)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, businessNumber);
    }
}

