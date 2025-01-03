namespace Intersect.Client.Interface;

public partial record struct InterfaceState(string Name)
{
    public static readonly InterfaceState Game = new(nameof(Game));
    public static readonly InterfaceState MainMenu = new(nameof(MainMenu));
}