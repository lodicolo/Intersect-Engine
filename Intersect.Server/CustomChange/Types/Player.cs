namespace Intersect.Server.CustomChange.Types;

public class Player(string clientId, Guid userId)
{
    public string ClientId { get; set; } = clientId;

    public Guid UserId { get; set; } = userId;

    public string ReconnectionToken { get; set; } = string.Empty;
}
