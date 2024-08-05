namespace Intersect.Server.CustomChange.Types;

public class LoginResponse
{
    public string RoomId { get; set; }

    public string RoomName { get; set; }

    public string ReconnectionToken { get; set; }

    public List<Player> Players { get; set; }
}
