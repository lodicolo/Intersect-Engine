namespace Intersect.Server.CustomChange.Types;

public class Room(string roomId, string roomName)
{
    public string Id { get; set; } = roomId;

    public string Name { get; set; } = roomName;

    public List<Player> Players { get; set; } = [];

    public bool IsGameStarted { get; set; } = false;

    public string? Leader { get; set; }

    public Player AddPlayer(Guid userId)
    {
        Player player = new Player(userId);

        if (Players.All(p => p.UserId != userId))
        {
            Players.Add(player);
            return player;
        }

        return default;
    }

    public void RemovePlayer(Guid userId)
    {
        _ = Players.RemoveAll(p => p.UserId == userId);
    }
}
