using System.Runtime.CompilerServices;
using Intersect.Client.Core;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Input;
using Intersect.Client.Framework.Gwen.Skin;
using Intersect.Client.General;
using Intersect.Client.Interface.Game;
using Intersect.Client.Interface.Menu;
using Intersect.Client.Interface.Shared;
using Intersect.Configuration;
using Intersect.Models;
using Base = Intersect.Client.Framework.Gwen.Renderer.Base;

namespace Intersect.Client.Interface;

public static partial class Interface
{

    private static readonly Queue<KeyValuePair<string, string>> _errorMessages = new();

    public static bool TryDequeueErrorMessage(out KeyValuePair<string, string> message) => _errorMessages.TryDequeue(out message);

    public static void ShowError(string message, string? header = default)
    {
        _errorMessages.Enqueue(new KeyValuePair<string, string>(header ?? string.Empty, message));
    }

    public static readonly ErrorHandler ErrorMessageHandler = new();
    private static InterfaceState _state;

    //GWEN GUI
    public static bool GwenInitialized { get; set; }

    public static InputBase GwenInput { get; set; }

    public static Base GwenRenderer { get; set; }

    public static bool HideUi { get; set; }

    private static Canvas sGameCanvas { get; set; }

    private static Canvas sMenuCanvas { get; set; }

    public static bool SetupHandlers { get; set; }

    public static GameInterface? GameUi { get; private set; }

    public static MenuGuiBase? MenuUi { get; private set; }

    public static MutableInterface CurrentInterface => GameUi as MutableInterface ?? MenuUi?.MainMenu;

    public static TexturedBase Skin { get; set; }

    //Input Handling
    public static List<Framework.Gwen.Control.Base> FocusElements { get; set; }

    public static List<Framework.Gwen.Control.Base> InputBlockingElements { get; set; }

    public static InterfaceState State
    {
        get => _state;
        set => SetState(default, value);
    }

    public static event InterfaceStateChangedHandler? StateChanged;

    private static void OnStateChanged(object? sender, InterfaceState state)
    {
        FocusElements = [];
        InputBlockingElements = [];
        ErrorMessageHandler.Clear();

        IMutableInterface mutableInterface;
        if (state == InterfaceState.MainMenu)
        {
            GwenInput.Initialize(sMenuCanvas);
            mutableInterface = MenuUi = new MenuGuiBase(sMenuCanvas);
            GameUi = null;
        }
        else if (state == InterfaceState.Game)
        {
            GwenInput.Initialize(sGameCanvas);
            mutableInterface = GameUi = new GameInterface(sGameCanvas);
            MenuUi = null;
        }
        else
        {
            throw new NotImplementedException($"Unimplemented InterfaceState '{state.Name}'");
        }

        HookOnStateChanged(sender, state, mutableInterface);

        StateChanged?.Invoke(sender, state, mutableInterface);
    }

    public static void SetState(object? sender, InterfaceState state)
    {
        if (state == _state)
        {
            return;
        }

        _state = state;
        OnStateChanged(sender, _state);
    }

    #region "Gwen Setup and Input"

    //Gwen Low Level Functions
    public static void InitGwen(object? sender = default)
    {
        // Preserve the debug window
        MutableInterface.DetachDebugWindow();

        //TODO: Make it easier to modify skin.
        if (Skin == null)
        {
            Skin = TexturedBase.FindSkin(GwenRenderer, Globals.ContentManager, ClientConfiguration.Instance.UiSkin);
            Skin.DefaultFont = Graphics.UIFont;
        }

        MenuUi?.Dispose();

        GameUi?.Dispose();

        // Create a Canvas (it's root, on which all other GWEN controls are created)
        sMenuCanvas = new Canvas(Skin, "MainMenu")
        {
            Scale = 1f //(GameGraphics.Renderer.GetScreenWidth()/1920f);
        };

        sMenuCanvas.SetSize(
            (int) (Graphics.Renderer.GetScreenWidth() / sMenuCanvas.Scale),
            (int) (Graphics.Renderer.GetScreenHeight() / sMenuCanvas.Scale)
        );

        sMenuCanvas.ShouldDrawBackground = false;
        sMenuCanvas.BackgroundColor = Color.FromArgb(255, 150, 170, 170);
        sMenuCanvas.KeyboardInputEnabled = true;

        // Create the game Canvas (it's root, on which all other GWEN controls are created)
        sGameCanvas = new Canvas(Skin, "InGame");

        //_gameCanvas.Scale = (GameGraphics.Renderer.GetScreenWidth() / 1920f);
        sGameCanvas.SetSize(
            (int) (Graphics.Renderer.GetScreenWidth() / sGameCanvas.Scale),
            (int) (Graphics.Renderer.GetScreenHeight() / sGameCanvas.Scale)
        );

        sGameCanvas.ShouldDrawBackground = false;
        sGameCanvas.BackgroundColor = Color.FromArgb(255, 150, 170, 170);
        sGameCanvas.KeyboardInputEnabled = true;

        // Create GWEN input processor
        var currentGameState = Globals.GameState;

        switch (currentGameState)
        {
            case GameStates.Intro:
            case GameStates.Menu:
                SetState(sender, InterfaceState.MainMenu);
                break;
            case GameStates.Loading:
            case GameStates.Error:
            case GameStates.InGame:
                SetState(sender, InterfaceState.Game);
                break;
            default:
                throw new NotImplementedException(
                    $"Interface logic for GameState '{currentGameState}' not implemented"
                );
        }

        Globals.OnLifecycleChangeState();

        GwenInitialized = true;
    }

    public static void DestroyGwen(bool exiting = false)
    {
        // Preserve the debug window
        MutableInterface.DetachDebugWindow();

        //The canvases dispose of all of their children.
        sMenuCanvas?.Dispose();
        sGameCanvas?.Dispose();
        GameUi?.Dispose();

        // Destroy our target UI as well! Above code does NOT appear to clear this properly.
        if (Globals.Me != null)
        {
            Globals.Me.ClearTarget();
            Globals.Me.TargetBox?.Dispose();
            Globals.Me.TargetBox = null;
        }

        GwenInitialized = false;

        if (exiting)
        {
            MutableInterface.DisposeDebugWindow();
        }
    }

    public static bool HasInputFocus()
    {
        if (FocusElements == null || InputBlockingElements == null)
        {
            return false;
        }

        return FocusElements.Any(t => t.MouseInputEnabled && (t?.HasFocus ?? false)) || InputBlockingElements.Any(t => t?.IsHidden == false);
    }

    #endregion

    #region "GUI Functions"

    //Actual Drawing Function
    public static void DrawGui()
    {
        if (!GwenInitialized)
        {
            InitGwen();
        }

        if (Globals.GameState == GameStates.Menu)
        {
            MenuUi.Update();
        }
        else if (Globals.GameState == GameStates.InGame)
        {
            GameUi.Update();
        }

        //Do not allow hiding of UI under several conditions
        var forceShowUi = Globals.InCraft || Globals.InBank || Globals.InShop || Globals.InTrade || Globals.InBag || Globals.EventDialogs?.Count > 0 || HasInputFocus() || (!Interface.GameUi?.EscapeMenu?.IsHidden ?? true);

        ErrorMessageHandler.Update();
        sGameCanvas.RestrictToParent = false;
        if (Globals.GameState == GameStates.Menu)
        {
            MenuUi.Draw();
        }
        else if (Globals.GameState == GameStates.InGame)
        {
            if (HideUi && !forceShowUi)
            {
                if (sGameCanvas.IsVisible)
                {
                    sGameCanvas.Hide();
                }
            }
            else
            {
                if (!sGameCanvas.IsVisible)
                {
                    sGameCanvas.Show();
                }
                GameUi.Draw();
            }
        }
    }

    public static void SetHandleInput(bool val) => GwenInput.HandleInput = val;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DoesMouseHitInterface() => DoesMouseHitComponentOrChildren(sGameCanvas);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DoesMouseHitComponentOrChildren(Framework.Gwen.Control.Base? component) =>
        DoesComponentOrChildrenContainMousePoint(component, InputHandler.MousePosition);

    public static bool DoesComponentOrChildrenContainMousePoint(Framework.Gwen.Control.Base? component, Point position)
    {
        if (component == default)
        {
            return false;
        }

        if (component.IsHidden)
        {
            return false;
        }

        if (!component.Bounds.Contains(position))
        {
            return false;
        }

        if (component.MouseInputEnabled)
        {
            return true;
        }

        var localPosition = position;
        localPosition.X -= component.X;
        localPosition.Y -= component.Y;

        return component.Children.Any(child => DoesComponentOrChildrenContainMousePoint(child, localPosition));
    }

    public static string[] WrapText(string input, int width, GameFont font)
    {
        var myOutput = new List<string>();
        if (input == null)
        {
            myOutput.Add("");
        }
        else
        {
            var lastSpace = 0;
            var curPos = 0;
            var curLen = 1;
            var lastOk = 0;
            var lastCut = 0;
            input = input.Replace("\r\n", "\n");
            float measured;
            string line;
            while (curPos + curLen < input.Length)
            {
                line = input.Substring(curPos, curLen);
                measured = Graphics.Renderer.MeasureText(line, font, 1).X;
                if (measured < width)
                {
                    lastOk = lastSpace;
                    switch (input[curPos + curLen])
                    {
                        case ' ':
                        case '-':
                            lastSpace = curLen;

                            break;

                        case '\n':
                            myOutput.Add(input.Substring(curPos, curLen).Trim());
                            lastSpace = 0;
                            curPos = curPos + curLen + 1;
                            curLen = 1;

                            break;
                    }
                }
                else
                {
                    if (lastOk == 0)
                    {
                        lastOk = curLen - 1;
                    }

                    line = input.Substring(curPos, lastOk).Trim();
                    myOutput.Add(line);
                    curPos = curPos + lastOk;
                    lastOk = 0;
                    lastSpace = 0;
                    curLen = 1;
                }

                curLen++;
            }

            myOutput.Add(input.Substring(curPos, input.Length - curPos).Trim());
        }

        return myOutput.ToArray();
    }

    #endregion

    public static Framework.Gwen.Control.Base FindControlAtCursor()
    {
        var currentElement = CurrentInterface?.Root;
        var cursor = new Point(InputHandler.MousePosition.X, InputHandler.MousePosition.Y);

        while (default != currentElement)
        {
            var elementAt = currentElement.GetControlAt(cursor);
            if (elementAt == currentElement || elementAt == default)
            {
                break;
}

            currentElement = elementAt;
        }

        return currentElement;
    }

    private static partial void HookOnStateChanged(object? sender, InterfaceState state, IMutableInterface mutableInterface);
}
