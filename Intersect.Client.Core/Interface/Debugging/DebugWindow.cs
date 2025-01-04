using Intersect.Async;
using Intersect.Client.Core;
using Intersect.Client.Framework.Gwen;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Control.Data;
using Intersect.Client.Framework.Gwen.Control.Layout;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.Client.Maps;
using Intersect.Extensions;
using static Intersect.Client.Framework.File_Management.GameContentManager;

namespace Intersect.Client.Interface.Debugging;

internal sealed partial class DebugWindow : Window
{
    private readonly List<IDisposable> _disposables;
    private bool _wasParentDrawDebugOutlinesEnabled;
    private bool _drawDebugOutlinesEnabled;

    public DebugWindow(Base parent) : base(parent, Strings.Debug.Title, false, nameof(DebugWindow))
    {
        _disposables = [];
        DisableResizing();

        CheckboxDrawDebugOutlines = CreateCheckboxDrawDebugOutlines();
        CheckboxEnableLayoutHotReloading = CreateCheckboxEnableLayoutHotReloading();
        ButtonShutdownServer = CreateButtonShutdownServer();
        ButtonShutdownServerAndExit = CreateButtonShutdownServerAndExit();
        TableDebugStats = CreateTableDebugStats();

        IsHidden = true;
    }

    private LabeledCheckBox CheckboxDrawDebugOutlines { get; set; }

    private LabeledCheckBox CheckboxEnableLayoutHotReloading { get; set; }

    private Button ButtonShutdownServer { get; set; }

    private Button ButtonShutdownServerAndExit { get; set; }

    private Table TableDebugStats { get; set; }

    protected override void EnsureInitialized()
    {
        LoadJsonUi(UI.Debug, Graphics.Renderer?.GetResolutionString());
    }

    protected override void OnAttached(Base parent)
    {
        base.OnAttached(parent);

        Root.DrawDebugOutlines = _drawDebugOutlinesEnabled;
    }

    protected override void OnAttaching(Base newParent)
    {
        base.OnAttaching(newParent);
        _wasParentDrawDebugOutlinesEnabled = newParent.DrawDebugOutlines;
    }

    protected override void OnDetached()
    {
        base.OnDetached();
    }

    protected override void OnDetaching(Base oldParent)
    {
        base.OnDetaching(oldParent);
        oldParent.DrawDebugOutlines = _wasParentDrawDebugOutlinesEnabled;
    }

    public override void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable?.Dispose();
        }

        base.Dispose();
    }

    private LabeledCheckBox CreateCheckboxDrawDebugOutlines()
    {
        var checkbox = new LabeledCheckBox(this, nameof(CheckboxDrawDebugOutlines))
        {
            IsChecked = Root?.DrawDebugOutlines ?? false,
            Text = Strings.Debug.DrawDebugOutlines,
        };

        checkbox.CheckChanged += (sender, args) =>
        {
            _drawDebugOutlinesEnabled = checkbox.IsChecked;
            if (Root != default)
            {
                Root.DrawDebugOutlines = _drawDebugOutlinesEnabled;
            }
        };

        return checkbox;
    }

    private LabeledCheckBox CreateCheckboxEnableLayoutHotReloading()
    {
        var checkbox = new LabeledCheckBox(this, nameof(CheckboxEnableLayoutHotReloading))
        {
            IsChecked = Globals.ContentManager.ContentWatcher.Enabled,
            Text = Strings.Debug.EnableLayoutHotReloading,
        };

        checkbox.CheckChanged += (sender, args) => Globals.ContentManager.ContentWatcher.Enabled = checkbox.IsChecked;

        checkbox.SetToolTipText(Strings.Internals.ExperimentalFeatureTooltip);
        checkbox.ToolTipFont = Skin.DefaultFont;

        return checkbox;
    }

    private Button CreateButtonShutdownServer()
    {
        var button = new Button(this, nameof(ButtonShutdownServer))
        {
            IsHidden = true,
            Text = Strings.Debug.ShutdownServer,
        };

        button.Clicked += (sender, args) =>
        {
            // TODO: Implement
        };

        return button;
    }

    private Button CreateButtonShutdownServerAndExit()
    {
        var button = new Button(this, nameof(ButtonShutdownServerAndExit))
        {
            IsHidden = true,
            Text = Strings.Debug.ShutdownServerAndExit,
        };

        button.Clicked += (sender, args) =>
        {
            // TODO: Implement
        };

        return button;
    }

    private Table CreateTableDebugStats()
    {
        var table = new Table(this, nameof(TableDebugStats))
        {
            ColumnCount = 2,
        };

        var fpsProvider = new ValueTableCellDataProvider<int>(() => Graphics.Renderer?.GetFps() ?? 0);
        table.AddRow(Strings.Debug.Fps).Listen(fpsProvider, 1);
        _disposables.Add(fpsProvider.Generator.Start());

        var pingProvider = new ValueTableCellDataProvider<int>(() => Networking.Network.Ping);
        table.AddRow(Strings.Debug.Ping).Listen(pingProvider, 1);
        _disposables.Add(pingProvider.Generator.Start());

        var drawCallsProvider = new ValueTableCellDataProvider<int>(() => Graphics.DrawCalls);
        table.AddRow(Strings.Debug.Draws).Listen(drawCallsProvider, 1);
        _disposables.Add(drawCallsProvider.Generator.Start());

        var mapNameProvider = new ValueTableCellDataProvider<string>(() =>
        {
            var mapId = Globals.Me?.MapId ?? default;

            if (mapId == default)
            {
                return Strings.Internals.NotApplicable;
            }

            return MapInstance.Get(mapId)?.Name ?? Strings.Internals.NotApplicable;
        });
        table.AddRow(Strings.Debug.Map).Listen(mapNameProvider, 1);
        _disposables.Add(mapNameProvider.Generator.Start());

        var coordinateXProvider = new ValueTableCellDataProvider<int>(() => Globals.Me?.X ?? -1);
        table.AddRow(Strings.Internals.CoordinateX).Listen(coordinateXProvider, 1);
        _disposables.Add(coordinateXProvider.Generator.Start());

        var coordinateYProvider = new ValueTableCellDataProvider<int>(() => Globals.Me?.Y ?? -1);
        table.AddRow(Strings.Internals.CoordinateY).Listen(coordinateYProvider, 1);
        _disposables.Add(coordinateYProvider.Generator.Start());

        var coordinateZProvider = new ValueTableCellDataProvider<int>(() => Globals.Me?.Z ?? -1);
        table.AddRow(Strings.Internals.CoordinateZ).Listen(coordinateZProvider, 1);
        _disposables.Add(coordinateZProvider.Generator.Start());

        var knownEntitiesProvider = new ValueTableCellDataProvider<int>(() => Graphics.DrawCalls);
        table.AddRow(Strings.Debug.KnownEntities).Listen(knownEntitiesProvider, 1);
        _disposables.Add(knownEntitiesProvider.Generator.Start());

        var knownMapsProvider = new ValueTableCellDataProvider<int>(() => MapInstance.Lookup.Count);
        table.AddRow(Strings.Debug.KnownMaps).Listen(knownMapsProvider, 1);
        _disposables.Add(knownMapsProvider.Generator.Start());

        var mapsDrawnProvider = new ValueTableCellDataProvider<int>(() => Graphics.MapsDrawn);
        table.AddRow(Strings.Debug.MapsDrawn).Listen(mapsDrawnProvider, 1);
        _disposables.Add(mapsDrawnProvider.Generator.Start());

        var entitiesDrawnProvider = new ValueTableCellDataProvider<int>(() => Graphics.EntitiesDrawn);
        table.AddRow(Strings.Debug.EntitiesDrawn).Listen(entitiesDrawnProvider, 1);
        _disposables.Add(entitiesDrawnProvider.Generator.Start());

        var lightsDrawnProvider = new ValueTableCellDataProvider<int>(() => Graphics.LightsDrawn);
        table.AddRow(Strings.Debug.LightsDrawn).Listen(lightsDrawnProvider, 1);
        _disposables.Add(lightsDrawnProvider.Generator.Start());

        var timeProvider = new ValueTableCellDataProvider<string>(Time.GetTime);
        table.AddRow(Strings.Debug.Time).Listen(timeProvider, 1);
        _disposables.Add(timeProvider.Generator.Start());

        var interfaceObjectsProvider = new ValueTableCellDataProvider<int>(cancellationToken =>
        {
            try
            {
                return Interface.CurrentInterface?.Children.ToArray().SelectManyRecursive(
                    x => x != default ? [.. x.Children] : [],
                    cancellationToken
                ).ToArray().Length ?? 0;
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        });
        table.AddRow(Strings.Debug.InterfaceObjects).Listen(interfaceObjectsProvider, 1);
        _disposables.Add(interfaceObjectsProvider.Generator.Start());

        _ = table.AddRow(Strings.Debug.ControlUnderCursor, 1);

        var controlUnderCursorProvider = new ControlUnderCursorProvider();
        var controlUnderCursorRow = 0;
        table.AddRow(Strings.Internals.Type).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.Name).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.CanonicalName).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.LocalItem.ToString(Strings.Internals.Bounds)).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.GlobalItem.ToString(Strings.Internals.Bounds)).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.Margin).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.Padding).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.Dock).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.TextPadding).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.Font).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.HasOwnFont).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.Color).Listen(controlUnderCursorProvider, controlUnderCursorRow++);
        table.AddRow(Strings.Internals.ColorOverride).Listen(controlUnderCursorProvider, controlUnderCursorRow++);

        _disposables.Add(controlUnderCursorProvider.Generator.Start());

        return table;
    }

    private partial class ControlUnderCursorProvider : ITableDataProvider
    {
        public ControlUnderCursorProvider()
        {
            Generator = new CancellableGenerator<Base>(CreateControlUnderCursorGenerator);
        }

        public event TableDataChangedEventHandler? DataChanged;

        public CancellableGenerator<Base> Generator { get; }

        public void Start()
        {
            _ = Generator.Start();
        }

        private void UpdateValues(Base component)
        {
            var row = 0;
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.GetType().Name ?? Strings.Internals.NotApplicable
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.Name ?? string.Empty
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.CanonicalName ?? string.Empty
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.Bounds.ToString() ?? string.Empty
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.BoundsGlobal.ToString() ?? string.Empty
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.Margin.ToString() ?? string.Empty
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.Padding.ToString() ?? string.Empty
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component == default
                        ? Strings.Internals.NotApplicable.ToString()
                        : string.Join(
                            " | ",
                            Enum.GetValues<Pos>().Where(p => component.Dock.HasFlag(p)).Select(p => p.ToString())
                        )
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    (component as Label)?.TextPadding.ToString() ?? Strings.Internals.NotApplicable.ToString()
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.Font?.ToString() ?? Strings.Internals.NotApplicable.ToString()
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    component?.HasOwnFont.ToString() ?? Strings.Internals.NotApplicable.ToString()
                )
            );

            var colorableText = component as IColorableText;
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    colorableText?.TextColor ?? string.Empty
                )
            );
            DataChanged?.Invoke(
                this,
                new TableDataChangedEventArgs(
                    row++,
                    1,
                    default,
                    colorableText?.TextColorOverride?.ToString() ?? Strings.Internals.NotApplicable.ToString()
                )
            );
        }

        private AsyncValueGenerator<Base> CreateControlUnderCursorGenerator(CancellationToken cancellationToken)
        {
            return new AsyncValueGenerator<Base>(
                () => Task.Delay(100).ContinueWith(_ => Interface.FindControlAtCursor(), TaskScheduler.Current),
                UpdateValues,
                cancellationToken
            );
        }
    }
}
