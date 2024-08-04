using Microsoft.AspNetCore.SignalR;

namespace Intersect.Server.CustomChange;

public class GameHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        if (RoomHandler.Rooms.Count == 0)
        {
            await RoomHandler.CreateRoom(Groups, Context.ConnectionId, "MainMenu");
        }

        // Find any room that the player was in before
        var room = RoomHandler.Rooms.Values.FirstOrDefault(r => r.Players.FirstOrDefault(p => p.ClientId == Context.ConnectionId) != default);
        if (room != default)
        {
            await RoomHandler.JoinRoomById(Groups, Context.ConnectionId, room.Id);
        }
        else
        {
            await RoomHandler.JoinRoomByName(Groups, Context.ConnectionId, "MainMenu");
        }

        // TODO: Send all rooms to the client and handle reconnection
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        // TODO: Handle disconnection and reconnection

        await RoomHandler.LeaveRoom(Groups, Context.ConnectionId, default);
    }
}