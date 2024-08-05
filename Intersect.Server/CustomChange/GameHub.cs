#pragma warning disable CA1822 // Mark members as static

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

        var player = RoomHandler.FindPlayerByToken(reconnectionToken);
        var user = User.FindById(player?.UserId ?? Guid.Empty);

        // player not found, anounymous player
        if (player == default || user == default)
        {
            return;
        }

        player.ClientId = Context.ConnectionId;
        var disconnectedPlayer = RoomHandler.GetDisconnectedPlayer(user.Id);

        // player was disconnected, but it reconnected
        if (disconnectedPlayer != null)
        {
            RoomHandler.RemoveDisconnectedPlayer(user.Id);

            var room = RoomHandler.FindRoomByUserId(user.Id);
            if (room == default)
            {
                RoomHandler.JoinRoomByName(user.Id, "menu");
            }
        }
        else
        {
            RoomHandler.JoinRoomByName(user.Id, "menu");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var player = RoomHandler.FindPlayerByConnectionId(Context.ConnectionId);
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
            RoomHandler.CreateRoom(user.Id, "menu");
        }

        var room = RoomHandler.FindRoomByName("menu");
        if (room == default)
        {
            return default;
        }

        var player = RoomHandler.AddPlayerToRoom(user.Id, room.Id);
        player.ClientId = Context.ConnectionId;
        player.ReconnectionToken = SimpleIdGenerator.NewId;

        await Clients.Caller.SendAsync("RoomCreated", new
        {
            id = room.Id,
            name = room.Name,
        });

        return new LoginResponse
        {
            RoomName = room.Name,
            ReconnectionToken = player.ReconnectionToken,
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
        var player = RoomHandler.FindPlayerByToken(reconnectionToken);
        var user = User.FindById(player?.UserId ?? Guid.Empty);
        return user?.Name;
    }
}