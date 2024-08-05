namespace Intersect.Server.CustomChange.Types;

public class Room(string roomId, string roomName)
{
    public string Id { get; set; } = roomId;

    public string Name { get; set; } = roomName;

    public List<Player> Players { get; set; } = [];

    public bool IsGameStarted { get; set; } = false;

    public string? Leader { get; set; }

    public void AddPlayer(string clientId, Guid userId)
    {
        if (Players.All(p => p.ClientId != clientId))
        {
            Players.Add(new Player(clientId, userId));
        }
    }

    public void RemovePlayer(string clientId)
    {
        _ = Players.RemoveAll(p => p.ClientId == clientId);
    }
}
