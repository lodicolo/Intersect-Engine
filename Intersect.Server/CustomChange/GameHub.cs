using Intersect.Server.CustomChange.Types;
using Intersect.Server.CustomChange.Utils;
using Intersect.Server.Database;
using Intersect.Server.Database.PlayerData;
using Intersect.Server.Localization;
using Intersect.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace Intersect.Server.CustomChange;

public class GameHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var httpContext = Context.GetHttpContext();
        var reconnectionToken = httpContext.Request.Query["access_token"].ToString();

        // anounymous player, we accept the connection, but we don't do anything with it
        if (string.IsNullOrWhiteSpace(reconnectionToken))
        {
            return;
        }

        var player = RoomHandler.FindUserByToken(reconnectionToken);
        var user = User.FindById(player?.UserId ?? Guid.Empty);

        // player not found, anounymous player
        if (player == default || user == default)
        {
            return;
        }

        var disconnectedPlayer = RoomHandler.GetDisconnectedPlayer(user.Id);

        // player was disconnected, but it reconnected
        if (disconnectedPlayer != null)
        {
            disconnectedPlayer.ClientId = Context.ConnectionId;
            RoomHandler.RemoveDisconnectedPlayer(user.Id);

            // find any room that the player was in before
            var room = RoomHandler.Rooms.Values.FirstOrDefault(r => r.Players.Any(p => p.UserId == user.Id));
            if (room != default)
            {
                await RoomHandler.JoinRoomById(Groups, Context.ConnectionId, user.Id, room.Id);
            }
        }
        else
        {
            await RoomHandler.JoinRoomByName(Groups, Context.ConnectionId, user.Id, "menu");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var player = RoomHandler.GetPlayerByConnectionId(Context.ConnectionId);
        if (player != default)
        {
            RoomHandler.AddDisconnectedPlayer(player);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<LoginResponse> Login(string username, string password)
    {
        if (!FieldChecking.IsValidUsername(username, Strings.Regex.Username))
        {
            return default;
        }

        var user = User.TryLogin(username, password);
        if (user == default)
        {
            return default;
        }

        // user found, we accept the connection and we add the player to the room
        if (RoomHandler.Rooms.Count == 0)
        {
            await RoomHandler.CreateRoom(Groups, Context.ConnectionId, user.Id, "menu");
        }

        var room = RoomHandler.Rooms.Values.FirstOrDefault(r => r.Name == "menu");
        if (room == default)
        {
            return default;
        }

        await RoomHandler.AddPlayerToRoom(Groups, Context.ConnectionId, user.Id, room.Id);

        user.ReconnectionToken = SimpleIdGenerator.NewId;
        var player = room.Players.FirstOrDefault(p => p.ClientId == Context.ConnectionId);
        if (player != default)
        {
            player.ReconnectionToken = user.ReconnectionToken;
        }

        return new LoginResponse
        {
            RoomId = room.Id,
            RoomName = room.Name,
            ReconnectionToken = user.ReconnectionToken,
            Players = room.Players
        };
    }

    public async Task<LoginResponse> Register(string email, string username, string password)
    {
        if (!FieldChecking.IsValidUsername(username, Strings.Regex.Username))
        {
            return default;
        }

        if (!FieldChecking.IsWellformedEmailAddress(email, Strings.Regex.Email))
        {
            return default;
        }

        if (User.UserExists(username))
        {
            return default;
        }

        DbInterface.CreateAccount(null, username, password.ToUpperInvariant().Trim(), email);
        return await Login(username, password);
    }

    public string? GetUsername(string reconnectionToken)
    {
        var player = RoomHandler.FindUserByToken(reconnectionToken);
        var user = User.FindById(player?.UserId ?? Guid.Empty);
        return user?.Name;
    }
}