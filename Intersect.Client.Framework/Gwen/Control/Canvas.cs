﻿using System.Diagnostics;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Gwen.Anim;
using Intersect.Client.Framework.Gwen.DragDrop;
using Intersect.Client.Framework.Gwen.Input;
using Intersect.Client.Framework.Audio;
using Intersect.Client.Framework.Input;
using Intersect.Core;
using Microsoft.Extensions.Logging;

namespace Intersect.Client.Framework.Gwen.Control;


/// <summary>
///     Canvas control. It should be the root parent for all other controls.
/// </summary>
public partial class Canvas : Base
{
    private readonly List<IDisposable> _disposeQueue = [];

    private Color mBackgroundColor;

    // [omeg] these are not created by us, so no disposing
    internal Base? mFirstTab;

    internal Base? mNextTab;

    internal readonly LinkedList<Base> _tabQueue = new();

    private bool mNeedsRedraw;

    private float mScale;

    private List<GameAudioInstance> mUISounds;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Canvas" /> class.
    /// </summary>
    /// <param name="skin">Skin to use.</param>
    /// <param name="name"></param>
    public Canvas(Skin.Base skin, string? name = default) : base(parent: null, name: name)
    {
        mUISounds = new List<GameAudioInstance>();
        SetBounds(x: 0, y: 0, width: 10000, height: 10000);
        SetSkin(skin: skin);
        Scale = 1.0f;
        BackgroundColor = Color.White;
        ShouldDrawBackground = false;
        IsTabable = false;
        MouseInputEnabled = false;
    }

    /// <summary>
    ///     Scale for rendering.
    /// </summary>
    public float Scale
    {
        get => mScale;
        set
        {
            if (mScale == value)
            {
                return;
            }

            mScale = value;

            if (Skin != null && Skin.Renderer != null)
            {
                Skin.Renderer.Scale = mScale;
            }

            OnScaleChanged();
            Redraw();
        }
    }

    /// <summary>
    ///     Background color.
    /// </summary>
    public Color BackgroundColor
    {
        get => mBackgroundColor;
        set => mBackgroundColor = value;
    }

    /// <summary>
    ///     In most situations you will be rendering the canvas every frame.
    ///     But in some situations you will only want to render when there have been changes.
    ///     You can do this by checking NeedsRedraw.
    /// </summary>
    public bool NeedsRedraw
    {
        get => mNeedsRedraw;
        set => mNeedsRedraw = value;
    }

    protected override void Dispose(bool disposing)
    {
        ProcessDelayedDeletes();
        base.Dispose(disposing);
    }

    /// <summary>
    ///     Re-renders the control, invalidates cached texture.
    /// </summary>
    public override void Redraw()
    {
        NeedsRedraw = true;
        base.Redraw();
    }

    /// <summary>
    ///     Additional initialization (which is sometimes not appropriate in the constructor)
    /// </summary>
    protected void Initialize()
    {
    }

    /// <summary>
    ///     Renders the canvas. Call in your rendering loop.
    /// </summary>
    public void RenderCanvas(TimeSpan elapsed, TimeSpan total)
    {
        UpdateDataProviders(elapsed, total);

        DoThink();

        var render = Skin.Renderer;

        render.Begin();

        render.ClipRegion = Bounds;

        //render.RenderOffset = new Point(X,Y);
        render.Scale = Scale;

        if (ShouldDrawBackground)
        {
            render.DrawColor = mBackgroundColor;
            render.DrawFilledRect(RenderBounds);
        }

        DoRender(Skin);

        DragAndDrop.RenderOverlay(this, Skin);

        ToolTip.RenderToolTip(Skin);

        render.EndClip();

        render.End();
    }

    /// <summary>
    ///     Renders the control using specified skin.
    /// </summary>
    /// <param name="skin">Skin to use.</param>
    protected override void Render(Skin.Base skin)
    {
        //skin.Renderer.rnd = new Random(1);
        base.Render(skin);
        mNeedsRedraw = false;
    }

    /// <summary>
    ///     Handler invoked when control's bounds change.
    /// </summary>
    /// <param name="oldBounds">Old bounds.</param>
    /// <param name="newBounds"></param>
    protected override void OnBoundsChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        base.OnBoundsChanged(oldBounds, newBounds);
        InvalidateChildren(true);
    }

    public void PlayAndAddSound(GameAudioInstance sound)
    {
        // Track the sound - we will check to see when it's done and dispose of it in DoThink()
        mUISounds.Add(sound);
        // And play the sound
        sound.Volume = 100;
        sound.Play();
    }

    private void RemoveAndDisposeDeadSounds()
    {
        mUISounds.RemoveAll(item =>
        {
            if (item.State == GameAudioInstance.AudioInstanceState.Stopped || item.State == GameAudioInstance.AudioInstanceState.Disposed)
            {
                item.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        });
    }

    /// <summary>
    ///     Processes input and layout. Also purges delayed delete queue.
    /// </summary>
    private void DoThink()
    {
        if (IsHidden)
        {
            return;
        }

        RemoveAndDisposeDeadSounds();

        Animation.GlobalThink();

        // Reset tabbing
        mNextTab = null;
        mFirstTab = null;

        ProcessDelayedDeletes();

        InvokeThreadQueue();

        RecurseLayout(Skin);

        // If we didn't have a next tab, cycle to the start.
        if (mNextTab == null)
        {
            mNextTab = mFirstTab;
        }

        InputHandler.OnCanvasThink(this);
    }

    /// <summary>
    ///     Adds given control to the delete queue and detaches it from canvas. Don't call from Dispose, it modifies child
    ///     list.
    /// </summary>
    /// <param name="control">Control to delete.</param>
    public void AddDelayedDelete(Base control)
    {
        lock (_disposeQueue)
        {
            if (!_disposeQueue.Contains(control))
            {
                _disposeQueue.Add(control);
                control.Parent?.RemoveChild(control, false);
#if DEBUG
                _delayedDeleteStackTraces.Add(control, new StackTrace());
            }
            else
            {
                throw new InvalidOperationException("Control deleted twice");
#endif
            }
        }
    }

#if DEBUG
    private readonly Dictionary<Base, StackTrace> _delayedDeleteStackTraces = [];
#endif

    private void ProcessDelayedDeletes()
    {
        lock (_disposeQueue)
        {
            foreach (var control in _disposeQueue)
            {
#if DEBUG
                if (control is Base node)
                {
                    _delayedDeleteStackTraces.Remove(node);
                }
#endif

                control.Dispose();
            }

            _disposeQueue.Clear();
        }
    }

    /// <summary>
    ///     Handles mouse movement events. Called from Input subsystems.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool Input_MouseMoved(int x, int y, int dx, int dy)
    {
        if (IsHidden)
        {
            return false;
        }

        // Todo: Handle scaling here..
        var fScale = 1.0f / Scale;
        x = (int) (x * fScale);
        y = (int) (y * fScale);
        dx = (int) (dx * fScale);
        dy = (int) (dy * fScale);

        InputHandler.OnMouseMoved(this, x, y, dx, dy);

        var hoveredControl = InputHandler.HoveredControl;
        if (hoveredControl == null)
        {
            // ApplicationContext.Context.Value?.Logger.LogTrace(
            //     "Skipping emitting mouse moved because there is no hovered control"
            // );
            return false;
        }

        if (hoveredControl == this)
        {
            // ApplicationContext.Context.Value?.Logger.LogTrace(
            //     "Skipping emitting mouse moved because {ControlName} is this canvas",
            //     hoveredControl.CanonicalName
            // );
            return false;
        }

        if (hoveredControl.Canvas != this)
        {
            // ApplicationContext.Context.Value?.Logger.LogTrace(
            //     "Skipping emitting mouse moved because {ControlName} is not part of this canvas",
            //     hoveredControl.CanonicalName
            // );
            return false;
        }

        // ApplicationContext.Context.Value?.Logger.LogTrace(
        //     "Emitting mouse moved to {ControlName}",
        //     hoveredControl.CanonicalName
        // );

        hoveredControl.InputMouseMoved(x, y, dx, dy);
        hoveredControl.UpdateCursor();

        DragAndDrop.OnMouseMoved(hoveredControl, x, y);

        return true;
    }

    /// <summary>
    ///     Handles mouse button events. Called from Input subsystems.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool Input_MouseButton(MouseButton button, bool down) =>
        !IsHidden && InputHandler.OnMouseButtonStateChanged(this, button, down);

    /// <summary>
    ///     Handles mouse button events. Called from Input subsystems.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool Input_MouseScroll(int deltaX, int deltaY)
    {
        if (IsHidden)
        {
            return false;
        }

        return InputHandler.OnMouseScroll(this, deltaX, deltaY);
    }

    /// <summary>
    ///     Handles keyboard events. Called from Input subsystems.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool Input_Key(Key key, bool down, bool shift = false)
    {
        if (IsHidden)
        {
            return false;
        }

        if (key <= Key.Invalid)
        {
            return false;
        }

        if (key >= Key.Count)
        {
            return false;
        }

        if (key == Key.Tab)
        {
            return OnKeyTab(down, shift);
        }

        return InputHandler.OnKeyEvent(this, key, down);
    }

    /// <summary>
    ///     Handles keyboard events. Called from Input subsystems.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool Input_Character(char chr)
    {
        if (IsHidden)
        {
            return false;
        }

        if (char.IsControl(chr))
        {
            return false;
        }

        //Handle Accelerators
        if (InputHandler.HandleAccelerator(this, chr))
        {
            return true;
        }

        //Handle characters
        if (InputHandler.KeyboardFocus == null)
        {
            return false;
        }

        if (InputHandler.KeyboardFocus.Canvas != this)
        {
            return false;
        }

        if (!InputHandler.KeyboardFocus.IsVisibleInTree)
        {
            return false;
        }

        //if (InputHandler.IsControlDown) return false;

        return InputHandler.KeyboardFocus.InputChar(chr);
    }

    /// <summary>
    ///     Handles the mouse wheel events. Called from Input subsystems.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool Input_MouseWheel(int val)
    {
        if (IsHidden)
        {
            return false;
        }

        if (InputHandler.HoveredControl == null)
        {
            return false;
        }

        if (InputHandler.HoveredControl == this)
        {
            return false;
        }

        if (InputHandler.HoveredControl.Canvas != this)
        {
            return false;
        }

        return InputHandler.HoveredControl.InputMouseWheeled(val);
    }

}
