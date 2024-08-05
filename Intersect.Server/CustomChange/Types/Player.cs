namespace Intersect.Server.CustomChange.Types;

public class Player(Guid userId)
{
    public Guid UserId { get; set; } = userId;

    public string ClientId { get; set; } = string.Empty;

    public string ReconnectionToken { get; set; } = string.Empty;
}
