namespace Intersect.Server.CustomChange.Types;

public class Room(string roomId, string roomName)
{
    public string Id { get; set; } = roomId;

    public string Name { get; set; } = roomName;

    public List<Player> Players { get; set; } = [];

    public bool IsGameStarted { get; set; } = false;

    public void AddPlayer(string clientId)
    {
        if (Players.All(p => p.ClientId != clientId))
        {
            Players.Add(new Player(clientId));
        }
    }

    public void RemovePlayer(string clientId)
    {
        _ = Players.RemoveAll(p => p.ClientId == clientId);
    }
}
