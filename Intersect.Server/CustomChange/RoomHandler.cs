using Intersect.Server.CustomChange.Types;
using Intersect.Server.CustomChange.Utils;
using Intersect.Server.Database.PlayerData;
using Intersect.Utilities;

namespace Intersect.Server.CustomChange;

public static class RoomHandler
{
    public static readonly Dictionary<string, Room> Rooms = [];
    //TODO: Implement a way to remove disconnected players or rooms that are empty after a certain amount of time
    private static readonly Dictionary<Guid, Tuple<long, Player>> DisconnectedPlayers = [];

    public static void CreateRoom(Guid userId, string roomName)
    {
        var roomId = SimpleIdGenerator.NewId;
        if (!Rooms.TryAdd(roomId, new Room(roomId, roomName)))
        {
            return;
        }

        _ = AddPlayerToRoom(userId, roomId);
    }

    public static void JoinRoomByName(Guid userId, string roomName)
    {
        var room = FindRoomByName(roomName);
        if (room == default)
        {
            return;
        }

        _ = AddPlayerToRoom(userId, room.Id);
    }

    public static void JoinRoomById(Guid userId, string roomId)
    {
        var room = Rooms[roomId];
        if (room == default)
        {
            return;
        }

        _ = AddPlayerToRoom(userId, room.Id);
    }

    public static void LeaveRoom(Guid userId)
    {
        var room = FindRoomByUserId(userId);
        if (room == default)
        {
            return;
        }

        room.RemovePlayer(userId);

        var menuRoom = FindRoomByName("menu");
        if (menuRoom == default)
        {
            return;
        }

        _ = AddPlayerToRoom(userId, menuRoom.Id);
    }

    public static Player AddPlayerToRoom(Guid userId, string roomId)
    {
        // if the room does not exist, skip
        if (!Rooms.TryGetValue(roomId, out var room))
        {
            return default;
        }

        // if the player is already in the room, skip
        if (room.Players.FirstOrDefault(p => p.UserId == userId) != default)
        {
            return room.Players.FirstOrDefault(p => p.UserId == userId);
        }

        // remove from all previous rooms
        foreach (var r in Rooms.Values)
        {
            // if the player is not in the room, skip
            if (r.Players.FirstOrDefault(p => p.UserId == userId) == default)
            {
                continue;
            }

            r.RemovePlayer(userId);
        }

        return room.AddPlayer(userId);
    }

    public static Room? FindRoomById(string roomId)
    {
        return Rooms.TryGetValue(roomId, out var room) ? room : default;
    }

    public static Room? FindRoomByName(string roomName)
    {
        return Rooms.Values.FirstOrDefault(r => r.Name == roomName);
    }

    public static Room? FindRoomByUserId(Guid userId)
    {
        return Rooms.Values.FirstOrDefault(r => r.Players.FirstOrDefault(p => p.UserId == userId) != default);
    }

    public static Player? FindPlayerById(Guid userId)
    {
        return Rooms.Values.SelectMany(r => r.Players).FirstOrDefault(p => p.UserId == userId);
    }

    public static Player? FindPlayerByToken(string token)
    {
        return Rooms.Values.SelectMany(r => r.Players).FirstOrDefault(p => p.ReconnectionToken == token);
    }

    public static Player? FindPlayerByConnectionId(string connectionId)
    {
        return Rooms.Values.SelectMany(r => r.Players).FirstOrDefault(p => p.ClientId == connectionId);
    }

    public static void AddDisconnectedPlayer(Player player)
    {
        DisconnectedPlayers[player.UserId] = new Tuple<long, Player>(Timing.Global.Milliseconds + 30000, player);
    }

    public static Player GetDisconnectedPlayer(Guid userId)
    {
        return DisconnectedPlayers.TryGetValue(userId, out var data) ? data.Item2 : default;
    }

    public static void RemoveDisconnectedPlayer(Guid userId)
    {
        _ = DisconnectedPlayers.Remove(userId);
    }
}
