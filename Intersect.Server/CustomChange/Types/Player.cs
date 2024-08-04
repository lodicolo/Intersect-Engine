namespace Intersect.Server.CustomChange.Types;

public class Player(string clientId)
{
    public string ClientId { get; set; } = clientId;
}
