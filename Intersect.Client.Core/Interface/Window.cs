using Intersect.Client.Framework.Gwen.Control;
using Intersect.Logging;

namespace Intersect.Client.Interface;

public abstract class Window : WindowControl
{
    protected readonly Queue<Action> _postRenderActions = [];

    private readonly object _initializationLock = new();
    private volatile bool _initialized;

    protected Window(
        Base parent,
        string? title = default,
        bool modal = false,
        string? name = default
    ) : base(parent, title, modal, name)
    {
        SetTextColor(Color.White, ControlState.Active);
        SetTextColor(new Color(a: 255, r: 191, g: 191, b: 191), ControlState.Inactive);
    }

    protected override void Render(Framework.Gwen.Skin.Base skin)
    {
        lock (_initializationLock)
        {
            if (!_initialized)
            {
                _initialized = true;
                EnsureInitialized();
            }
        }

        base.Render(skin);

        while (_postRenderActions.TryDequeue(out var postRenderAction))
        {
            try
            {
                postRenderAction();
            }
            catch (Exception exception)
            {
                LegacyLogging.Logger?.Error(
                    exception,
                    $"Error while running a post-render action for window '{CanonicalName}'"
                );
            }
        }
    }

    protected abstract void EnsureInitialized();
}