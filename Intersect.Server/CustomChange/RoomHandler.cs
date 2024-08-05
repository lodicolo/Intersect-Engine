using Intersect.Server.CustomChange.Types;
using Intersect.Server.CustomChange.Utils;
using Microsoft.AspNetCore.SignalR;

namespace Intersect.Server.CustomChange;

public static class RoomHandler
{
    public static readonly Dictionary<string, Room> Rooms = [];

    public static async Task CreateRoom(IGroupManager groups, string clientId, Guid userId, string roomName)
    {
        var roomId = SimpleIdGenerator.NewId;
        if (!Rooms.TryAdd(roomId, new Room(roomId, roomName)))
        {
            return;
        }

        await AddPlayerToRoom(groups, clientId, userId, roomId);
    }

    public static async Task JoinRoomByName(IGroupManager groups, string clientId, Guid userId, string roomName)
    {
        var room = Rooms.FirstOrDefault(r => r.Value.Name == roomName).Value;
        if (room == default)
        {
            return;
        }

        await AddPlayerToRoom(groups, clientId, userId, room.Id);
    }

    public static async Task JoinRoomById(IGroupManager groups, string clientId, Guid userId, string roomId)
    {
        var room = Rooms[roomId];
        if (room == default)
        {
            return;
        }

        await AddPlayerToRoom(groups, clientId, userId, room.Id);
    }

    public static async Task LeaveRoom(IGroupManager groups, string clientId, string? roomId)
    {
        var room = Rooms.Values.FirstOrDefault(r => r.Players.FirstOrDefault(p => p.ClientId == clientId) != default);
        if (room == default)
        {
            return;
        }

        roomId ??= room.Id;
        await groups.RemoveFromGroupAsync(clientId, roomId);
        room.RemovePlayer(clientId);
    }

    public static async Task AddPlayerToRoom(IGroupManager groups, string clientId, Guid userId, string roomId)
    {
        // if the room does not exist, skip
        if (!Rooms.TryGetValue(roomId, out var room))
        {
            return;
        }

        // if the player is already in the room, skip
        if (room.Players.FirstOrDefault(p => p.ClientId == clientId) != default)
        {
            return;
        }

        // remove from all previous rooms
        foreach (var r in Rooms.Values)
        {
            // if the player is not in the room, skip
            if (r.Players.FirstOrDefault(p => p.ClientId == clientId) == default)
            {
                continue;
            }

            await LeaveRoom(groups, clientId, r.Id);
        }

        room.AddPlayer(clientId, userId);
        await groups.AddToGroupAsync(clientId, room.Id);

        // Give some line breaks in console and print a table with all rooms and players
        Console.WriteLine();
        Console.WriteLine("Rooms:");
        Console.WriteLine("------");
        Console.WriteLine("Room ID | Room Name | Players");
        Console.WriteLine("--------|-----------|--------");
        foreach (var r in Rooms.Values)
        {
            Console.WriteLine($"{r.Id} | {r.Name} | {string.Join(", ", r.Players.Select(p => p.ClientId))}");
        }
    }

    public static Player? FindUserByToken(string token)
    {
        return Rooms.Values.SelectMany(r => r.Players).FirstOrDefault(p => p.ReconnectionToken == token);
    }
}
