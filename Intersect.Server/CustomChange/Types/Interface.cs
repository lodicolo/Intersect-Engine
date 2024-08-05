namespace Intersect.Server.CustomChange.Types;

public interface ILoginData
{
    string Email { get; set; }

    string Password { get; set; }
}

public interface IRegisterData
{
    string Username { get; set; }

    string Password { get; set; }

    string ConfirmPassword { get; set; }

    string Email { get; set; }
}

public class LoginResponse
{
    public string RoomId { get; set; }

    public string RoomName { get; set; }

    public string ReconnectionToken { get; set; }

    public List<Player> Players { get; set; }
}
