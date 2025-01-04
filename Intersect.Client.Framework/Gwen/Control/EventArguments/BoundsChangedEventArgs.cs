using Intersect.Client.Framework.GenericClasses;

namespace Intersect.Client.Framework.Gwen.Control.EventArguments;

public class BoundsChangedEventArgs : EventArgs
{
    public BoundsChangedEventArgs(Rectangle oldBounds, Rectangle newBounds)
    {
        OldBounds = oldBounds;
        NewBounds = newBounds;
    }

    public Rectangle OldBounds { get; }
    public Rectangle NewBounds { get; }
}