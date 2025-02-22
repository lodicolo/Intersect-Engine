namespace Intersect.Client.Framework.Graphics;

public abstract partial class GameFont
{
    public GameFont(string fontName, int fontSize)
    {
        Name = fontName;
        Size = fontSize;
    }

    public string Name { get; set; } = string.Empty;

    public int Size { get; set; } = 12;

    public string GetName()
    {
        return Name;
    }

    public int GetSize()
    {
        return Size;
    }

    public abstract object GetFont();

    public override string ToString()
    {
        return $"{Name},{Size}";
    }

    public static string ToString(GameFont font)
    {
        return font?.ToString();
    }
}