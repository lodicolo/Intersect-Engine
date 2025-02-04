using Intersect.Client.Core;
using Intersect.Client.Core.Controls;
using Intersect.Client.Framework.Content;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Control.EventArguments;
using Intersect.Client.Framework.Gwen.Control.Layout;
using Intersect.Client.Framework.Gwen.ControlInternal;
using Intersect.Client.Framework.Input;
using Intersect.Client.General;
using Intersect.Client.Interface.Game;
using Intersect.Client.Interface.Menu;
using Intersect.Client.Localization;
using Intersect.Config;
using Intersect.Utilities;
using static Intersect.Client.Framework.File_Management.GameContentManager;
using MathHelper = Intersect.Client.Utilities.MathHelper;

using static Intersect.Client.Framework.File_Management.GameContentManager;

namespace Intersect.Client.Interface.Shared;

public partial class SettingsWindow : WindowControl
{
    private readonly GameFont? _defaultFont;

    // Parent Window.
    private readonly MainMenu? _mainMenu;
    private readonly EscapeMenu? _escapeMenu;

    // Settings Window.
    private readonly Label _settingsHeader;
    private readonly Button _applyPendingChangesButton;
    private readonly Button _cancelPendingChangesButton;

    // Settings Containers.
    private readonly ScrollControl _gameSettingsContainer;
    private readonly ScrollControl _videoSettingsContainer;
    private readonly ScrollControl _audioSettingsContainer;
    private readonly Table _audioTable;

    private readonly ScrollControl _keybindingSettingsContainer;
    private readonly Table _controlsTable;

    // Tabs.
    private readonly TabButton _gameSettingsTab;
    private readonly TabButton _videoSettingsTab;
    private readonly TabButton _audioSettingsTab;
    private readonly TabButton _keybindingSettingsTab;

    // Game Settings - Interface.
    private readonly ScrollControl _interfaceSettings;
    private readonly LabeledCheckBox _autoCloseWindowsCheckbox;
    private readonly LabeledCheckBox _autoToggleChatLogCheckbox;
    private readonly LabeledCheckBox _showExperienceAsPercentageCheckbox;
    private readonly LabeledCheckBox _showHealthAsPercentageCheckbox;
    private readonly LabeledCheckBox _showManaAsPercentageCheckbox;
    private readonly LabeledCheckBox _simplifiedEscapeMenu;

    // Game Settings - Information.
    private readonly ScrollControl _informationSettings;
    private readonly LabeledCheckBox _friendOverheadInfoCheckbox;
    private readonly LabeledCheckBox _guildMemberOverheadInfoCheckbox;
    private readonly LabeledCheckBox _myOverheadInfoCheckbox;
    private readonly LabeledCheckBox _npcOverheadInfoCheckbox;
    private readonly LabeledCheckBox _partyMemberOverheadInfoCheckbox;
    private readonly LabeledCheckBox _playerOverheadInfoCheckbox;
    private readonly LabeledCheckBox _friendOverheadHpBarCheckbox;
    private readonly LabeledCheckBox _guildMemberOverheadHpBarCheckbox;
    private readonly LabeledCheckBox _myOverheadHpBarCheckbox;
    private readonly LabeledCheckBox _mpcOverheadHpBarCheckbox;
    private readonly LabeledCheckBox _partyMemberOverheadHpBarCheckbox;
    private readonly LabeledCheckBox _playerOverheadHpBarCheckbox;
    private readonly LabeledCheckBox _typewriterCheckbox;

    // Game Settings - Targeting.
    private readonly ScrollControl _targetingSettings;
    private readonly LabeledCheckBox _stickyTarget;
    private readonly LabeledCheckBox _autoTurnToTarget;
    private readonly LabeledCheckBox _autoSoftRetargetOnSelfCast;

    // Video Settings.
    private readonly ComboBox _resolutionList;
    private MenuItem? _customResolutionMenuItem;
    private readonly ComboBox _fpsList;
    private readonly LabeledHorizontalSlider _worldScale;
    private readonly LabeledCheckBox _fullscreenCheckbox;
    private readonly LabeledCheckBox _lightingEnabledCheckbox;

    // Audio Settings.
    private readonly LabeledSlider _musicSlider;
    private readonly LabeledSlider _soundEffectsSlider;
    private int _previousMusicVolume;
    private int _previousSoundVolume;

    // Keybinding Settings.
    private Control _keybindingEditControl;
    private Controls? _keybindingEditControls;
    private Button? _keybindingEditBtn;
    private readonly Button _restoreDefaultKeybindingsButton;
    private long _keybindingListeningTimer;
    private int _keyEdit = -1;
    private readonly Dictionary<Control, Button[]> mKeybindingBtns = [];

    private Panel _bottomBar;
    private TabControl _tabs;

    // Open Settings.
    private bool _returnToMenu;

    // Initialize.
    public SettingsWindow(Base parent, MainMenu? mainMenu, EscapeMenu? escapeMenu) : base(parent: parent, title: Strings.Settings.Title, modal: false, name: nameof(SettingsWindow))
    {
        _mainMenu = mainMenu;
        _escapeMenu = escapeMenu;

        Interface.InputBlockingElements.Add(item: this);

        IconName = "SettingsWindow.icon.png";

        MinimumSize = new Point(x: 640, y: 400);
        IsResizable = false;
        IsClosable = false;

        Titlebar.MouseInputEnabled = false;

        TitleLabel.FontSize = 14;
        TitleLabel.TextColorOverride = Color.White;

        var fontNormal = Current?.GetFont(name: TitleLabel.FontName, size: 12);
        _defaultFont = Current?.GetFont(name: TitleLabel.FontName, 10);

        _bottomBar = new Panel(parent: this, name: "BottomBar")
        {
            Dock = Pos.Bottom,
            MinimumSize = new Point(x: 0, y: 40),
            Margin = Margin.Four,
            Padding = new Padding(horizontal: 8, vertical: 4),
        };

        _tabs = new TabControl(parent: this)
        {
            Dock = Pos.Fill,
            Margin = new Margin(left: 4, top: 4, right: 4, bottom: 0),
        };

        // Apply Button.
        _applyPendingChangesButton = new Button(parent: _bottomBar, name: nameof(_applyPendingChangesButton))
        {
            Alignment = [Alignments.Center],
            AutoSizeToContents = true,
            Font = fontNormal,
            MinimumSize = new Point(x: 96, y: 24),
            TextPadding = new Padding(horizontal: 16, vertical: 2),
            Text = Strings.Settings.Apply,
        };
        _applyPendingChangesButton.Clicked += SettingsApplyBtn_Clicked;
        _applyPendingChangesButton.SetHoverSound("octave-tap-resonant.wav");

        // Cancel Button.
        _cancelPendingChangesButton = new Button(parent: _bottomBar, name: nameof(_cancelPendingChangesButton))
        {
            Alignment = [Alignments.Right, Alignments.CenterV],
            AutoSizeToContents = true,
            Font = fontNormal,
            MinimumSize = new Point(x: 96, y: 24),
            TextPadding = new Padding(horizontal: 16, vertical: 2),
            Text = Strings.Settings.Cancel,
        };
        _cancelPendingChangesButton.Clicked += CancelPendingChangesButton_Clicked;
        _cancelPendingChangesButton.SetHoverSound("octave-tap-resonant.wav");

        #region InitGameSettings

        // Game Settings are stored in the GameSettings Scroll Control.
        _gameSettingsContainer = new ScrollControl(parent: this, name: "GameSettingsContainer")
        {
            Dock = Pos.Fill,
        };
        _gameSettingsContainer.EnableScroll(horizontal: false, vertical: false);

        // Game Settings subcategories are stored in the GameSettings List.
        var gameSettingsList = new ListBox(parent: _gameSettingsContainer, name: "GameSettingsList");
        _ = gameSettingsList.AddRow(label: Strings.Settings.InterfaceSettings);
        _ = gameSettingsList.AddRow(label: Strings.Settings.InformationSettings);
        _ = gameSettingsList.AddRow(label: Strings.Settings.TargetingSettings);
        gameSettingsList.EnableScroll(horizontal: false, vertical: true);
        gameSettingsList.SelectedRowIndex = 0;
        gameSettingsList[index: 0].Clicked += InterfaceSettings_Clicked;
        gameSettingsList[index: 1].Clicked += InformationSettings_Clicked;
        gameSettingsList[index: 2].Clicked += TargetingSettings_Clicked;

        // Game Settings - Interface.
        _interfaceSettings = new ScrollControl(parent: _gameSettingsContainer, name: "InterfaceSettings");
        _interfaceSettings.EnableScroll(horizontal: false, vertical: true);

        // Game Settings - Interface: Auto-close Windows.
        _autoCloseWindowsCheckbox = new LabeledCheckBox(parent: _interfaceSettings, name: "AutoCloseWindowsCheckbox")
        {
            Text = Strings.Settings.AutoCloseWindows
        };

        // Game Settings - Interface: Auto-toggle chat log visibility.
        _autoToggleChatLogCheckbox = new LabeledCheckBox(parent: _interfaceSettings, name: "AutoToggleChatLogCheckbox")
        {
            Text = Strings.Settings.AutoToggleChatLog
        };

        // Game Settings - Interface: Show EXP/HP/MP as Percentage.
        _showExperienceAsPercentageCheckbox = new LabeledCheckBox(parent: _interfaceSettings, name: "ShowExperienceAsPercentageCheckbox")
        {
            Text = Strings.Settings.ShowExperienceAsPercentage
        };

        _showHealthAsPercentageCheckbox = new LabeledCheckBox(parent: _interfaceSettings, name: "ShowHealthAsPercentageCheckbox")
        {
            Text = Strings.Settings.ShowHealthAsPercentage
        };

        _showManaAsPercentageCheckbox = new LabeledCheckBox(parent: _interfaceSettings, name: "ShowManaAsPercentageCheckbox")
        {
            Text = Strings.Settings.ShowManaAsPercentage
        };

        // Game Settings - Interface: simplified escape menu.
        _simplifiedEscapeMenu = new LabeledCheckBox(parent: _interfaceSettings, name: "SimplifiedEscapeMenu")
        {
            Text = Strings.Settings.SimplifiedEscapeMenu
        };

        // Game Settings - Information.
        _informationSettings = new ScrollControl(parent: _gameSettingsContainer, name: "InformationSettings");
        _informationSettings.EnableScroll(horizontal: false, vertical: true);

        // Game Settings - Information: Friends.
        _friendOverheadInfoCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "FriendOverheadInfoCheckbox")
        {
            Text = Strings.Settings.ShowFriendOverheadInformation
        };

        // Game Settings - Information: Guild Members.
        _guildMemberOverheadInfoCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "GuildMemberOverheadInfoCheckbox")
        {
            Text = Strings.Settings.ShowGuildOverheadInformation
        };

        // Game Settings - Information: Myself.
        _myOverheadInfoCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "MyOverheadInfoCheckbox")
        {
            Text = Strings.Settings.ShowMyOverheadInformation
        };

        // Game Settings - Information: NPCs.
        _npcOverheadInfoCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "NpcOverheadInfoCheckbox")
        {
            Text = Strings.Settings.ShowNpcOverheadInformation
        };

        // Game Settings - Information: Party Members.
        _partyMemberOverheadInfoCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "PartyMemberOverheadInfoCheckbox")
        {
            Text = Strings.Settings.ShowPartyOverheadInformation
        };

        // Game Settings - Information: Players.
        _playerOverheadInfoCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "PlayerOverheadInfoCheckbox")
        {
            Text = Strings.Settings.ShowPlayerOverheadInformation
        };

        // Game Settings - Information: friends overhead hp bar.
        _friendOverheadHpBarCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "FriendOverheadHpBarCheckbox")
        {
            Text = Strings.Settings.ShowFriendOverheadHpBar
        };

        // Game Settings - Information: guild members overhead hp bar.
        _guildMemberOverheadHpBarCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "GuildMemberOverheadHpBarCheckbox")
        {
            Text = Strings.Settings.ShowGuildOverheadHpBar
        };

        // Game Settings - Information: my overhead hp bar.
        _myOverheadHpBarCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "MyOverheadHpBarCheckbox")
        {
            Text = Strings.Settings.ShowMyOverheadHpBar
        };

        // Game Settings - Information: NPC overhead hp bar.
        _mpcOverheadHpBarCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "NpcOverheadHpBarCheckbox")
        {
            Text = Strings.Settings.ShowNpcOverheadHpBar
        };

        // Game Settings - Information: party members overhead hp bar.
        _partyMemberOverheadHpBarCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "PartyMemberOverheadHpBarCheckbox")
        {
            Text = Strings.Settings.ShowPartyOverheadHpBar
        };

        // Game Settings - Information: players overhead hp bar.
        _playerOverheadHpBarCheckbox = new LabeledCheckBox(parent: _informationSettings, name: "PlayerOverheadHpBarCheckbox")
        {
            Text = Strings.Settings.ShowPlayerOverheadHpBar
        };

        // Game Settings - Targeting.
        _targetingSettings = new ScrollControl(parent: _gameSettingsContainer, name: "TargetingSettings");
        _targetingSettings.EnableScroll(horizontal: false, vertical: false);

        // Game Settings - Targeting: Sticky Target.
        _stickyTarget = new LabeledCheckBox(parent: _targetingSettings, name: "StickyTargetCheckbox")
        {
            Text = Strings.Settings.StickyTarget
        };

        // Game Settings - Targeting: Auto-turn to Target.
        _autoTurnToTarget = new LabeledCheckBox(parent: _targetingSettings, name: "AutoTurnToTargetCheckbox")
        {
            Text = Strings.Settings.AutoTurnToTarget,
        };

        // Game Settings - Targeting: Auto-turn to Target.
        _autoSoftRetargetOnSelfCast = new LabeledCheckBox(parent: _targetingSettings, name: "AutoSoftRetargetOnSelfCast")
        {
            Text = Strings.Settings.AutoSoftRetargetOnSelfCast,
            TooltipText = Strings.Settings.AutoSoftRetargetOnSelfCastTooltip,
            TooltipBackgroundName = "tooltip.png",
            TooltipFontName = "sourcesansproblack",
            TooltipTextColor = Color.White,
        };

        // Game Settings - Typewriter Text
        _typewriterCheckbox = new LabeledCheckBox(parent: _interfaceSettings, name: "TypewriterCheckbox")
        {
            Text = Strings.Settings.TypewriterText
        };

        #endregion

        #region InitVideoSettings

        // Video Settings Get Stored in the VideoSettings Scroll Control.
        _videoSettingsContainer = new ScrollControl(parent: this, name: "VideoSettingsContainer")
        {
            Dock = Pos.Fill,
        };
        _videoSettingsContainer.EnableScroll(horizontal: false, vertical: false);

        // Video Settings - Resolution Background.
        var resolutionBackground = new ImagePanel(parent: _videoSettingsContainer, name: "ResolutionPanel");

        // Video Settings - Resolution Label.
        var resolutionLabel = new Label(parent: resolutionBackground, name: "ResolutionLabel")
        {
            Text = Strings.Settings.Resolution
        };

        // Video Settings - Resolution List.
        _resolutionList = new ComboBox(parent: resolutionBackground, name: "ResolutionCombobox");
        var myModes = Graphics.Renderer?.GetValidVideoModes();
        myModes?.ForEach(
            action: t =>
            {
                var item = _resolutionList.AddItem(label: t);
                item.TextAlign = Pos.Left;
            }
        );

        _worldScale = new LabeledHorizontalSlider(parent: _videoSettingsContainer, name: "WorldScale")
        {
            IsDisabled = !Options.IsLoaded,
            Label = Strings.Settings.WorldScale,
            SnapToNotches = false,
        };

        // Video Settings - FPS Background.
        var fpsBackground = new ImagePanel(parent: _videoSettingsContainer, name: "FPSPanel");

        // Video Settings - FPS Label.
        var fpsLabel = new Label(parent: fpsBackground, name: "FPSLabel")
        {
            Text = Strings.Settings.TargetFps
        };

        // Video Settings - FPS List.
        _fpsList = new ComboBox(parent: fpsBackground, name: "FPSCombobox");
        _ = _fpsList.AddItem(label: Strings.Settings.Vsync);
        _ = _fpsList.AddItem(label: Strings.Settings.Fps30);
        _ = _fpsList.AddItem(label: Strings.Settings.Fps60);
        _ = _fpsList.AddItem(label: Strings.Settings.Fps90);
        _ = _fpsList.AddItem(label: Strings.Settings.Fps120);
        _ = _fpsList.AddItem(label: Strings.Settings.UnlimitedFps);

        // Video Settings - Fullscreen Checkbox.
        _fullscreenCheckbox = new LabeledCheckBox(parent: _videoSettingsContainer, name: "FullscreenCheckbox")
        {
            Text = Strings.Settings.Fullscreen
        };

        // Video Settings - Enable Lighting Checkbox
        _lightingEnabledCheckbox = new LabeledCheckBox(parent: _videoSettingsContainer, name: "EnableLightingCheckbox")
        {
            Text = Strings.Settings.EnableLighting
        };

        #endregion

        #region InitAudioSettings

        // Audio Settings Get Stored in the AudioSettings Scroll Control.
        _audioSettingsContainer = new ScrollControl(parent: this, name: "AudioSettingsContainer")
        {
            Dock = Pos.Fill,
        };
        _audioSettingsContainer.EnableScroll(horizontal: false, vertical: false);

        _audioTable = new Table(parent: _audioSettingsContainer, name: nameof(_audioTable))
        {
            ColumnCount = 2,
            CellSpacing = new Point(4, 4),
            Dock = Pos.Fill,
            SizeToContents = true,
        };

        var textureVolumeSlider = Current?.GetTexture(TextureType.Gui, "volume_slider.png");
        var textureVolumeSliderHovered = Current?.GetTexture(TextureType.Gui, "volume_slider_hovered.png");

        // Audio Settings - Music Slider
        _musicSlider = new LabeledSlider(parent: _audioSettingsContainer, name: nameof(_musicSlider))
        {
            BackgroundImageName = "volume_bar.png",
            Dock = Pos.Top,
            Font = _defaultFont,
            Orientation = Orientation.LeftToRight,
            Height = 35,
            DraggerSize = new Point(9, 35),
            SliderSize =  new Point(200, 35),
            Label = Strings.Settings.VolumeMusic,
            LabelMinimumSize = new Point(100, 0),
            Rounding = 0,
            Min = 0,
            Max = 100,
            NotchCount = 5,
            SnapToNotches = false,
            ShouldDrawBackground = true,
        };

        _musicSlider.ValueChanged += MusicSliderOnValueChanged;
        _musicSlider.SetDraggerImage(textureVolumeSlider, Dragger.ControlState.Normal);
        _musicSlider.SetDraggerImage(textureVolumeSliderHovered, Dragger.ControlState.Hovered);
        _musicSlider.SetDraggerImage(textureVolumeSlider, Dragger.ControlState.Clicked);
        _musicSlider.SetSound("octave-tap-resonant.wav", Dragger.ControlSoundState.Hover);
        _musicSlider.SetSound("octave-tap-professional.wav", Dragger.ControlSoundState.MouseDown);
        _musicSlider.SetSound("octave-tap-professional.wav", Dragger.ControlSoundState.MouseUp);

        // Audio Settings - Sound Slider
        _soundEffectsSlider = new LabeledSlider(parent: _audioSettingsContainer, name: nameof(_soundEffectsSlider))
        {
            BackgroundImageName = "volume_bar.png",
            Dock = Pos.Top,
            Font = _defaultFont,
            Orientation = Orientation.LeftToRight,
            Height = 35,
            DraggerSize = new Point(9, 35),
            SliderSize =  new Point(200, 35),
            Label = Strings.Settings.VolumeSoundEffects,
            LabelMinimumSize = new Point(100, 0),
            Rounding = 0,
            Min = 0,
            Max = 100,
            NotchCount = 5,
            SnapToNotches = false,
            ShouldDrawBackground = true,
        };

        _soundEffectsSlider.ValueChanged += SoundEffectsSliderOnValueChanged;
        _soundEffectsSlider.SetDraggerImage(textureVolumeSlider, Dragger.ControlState.Normal);
        _soundEffectsSlider.SetDraggerImage(textureVolumeSliderHovered, Dragger.ControlState.Hovered);
        _soundEffectsSlider.SetDraggerImage(textureVolumeSlider, Dragger.ControlState.Clicked);
        _soundEffectsSlider.SetSound("octave-tap-resonant.wav", Dragger.ControlSoundState.Hover);
        _soundEffectsSlider.SetSound("octave-tap-professional.wav", Dragger.ControlSoundState.MouseDown);
        _soundEffectsSlider.SetSound("octave-tap-professional.wav", Dragger.ControlSoundState.MouseUp);

        #endregion

        #region InitKeybindingSettings

        // KeybindingSettings Get Stored in the KeybindingSettings Scroll Control
        _keybindingSettingsContainer = new ScrollControl(parent: this, name: "KeybindingSettingsContainer")
        {
            Dock = Pos.Fill,
        };
        _keybindingSettingsContainer.EnableScroll(horizontal: false, vertical: true);

        _controlsTable = new Table(parent: _keybindingSettingsContainer, name: nameof(_controlsTable))
        {
            Dock = Pos.Top,
            ColumnCount = 3,
            SizeToContents = true,
        };

        // Keybinding Settings - Restore Default Keys Button.
        _restoreDefaultKeybindingsButton = new Button(parent: _bottomBar, name: nameof(_restoreDefaultKeybindingsButton))
        {
            Alignment = [Alignments.Left, Alignments.CenterV],
            AutoSizeToContents = true,
            Font = fontNormal,
            IsVisible = false,
            MinimumSize = new Point(x: 96, y: 24),
            TextPadding = new Padding(horizontal: 16, vertical: 2),
            Text = Strings.Settings.Restore,
        };
        _restoreDefaultKeybindingsButton.Clicked += RestoreDefaultKeybindingsButton_Clicked;
        _restoreDefaultKeybindingsButton.SetHoverSound("octave-tap-resonant.wav");

        // Keybinding Settings - Controls
        var row = 0;
        foreach (var control in (_keybindingEditControls ?? Controls.ActiveControls).Mappings.Keys)
        {
            AddControlKeybindRow(control: control, row: ref row, keyButtons: out _);
        }

        Input.KeyDown += OnKeyDown;
        Input.MouseDown += OnKeyDown;
        Input.KeyUp += OnKeyUp;
        Input.MouseUp += OnKeyUp;

        #endregion

        _gameSettingsTab = _tabs.AddPage(label: Strings.Settings.GameSettingsTab, page: _gameSettingsContainer);
        _videoSettingsTab = _tabs.AddPage(label: Strings.Settings.VideoSettingsTab, page: _videoSettingsContainer);
        _audioSettingsTab = _tabs.AddPage(label: Strings.Settings.AudioSettingsTab, page: _audioSettingsContainer);
        _keybindingSettingsTab = _tabs.AddPage(label: Strings.Settings.KeyBindingSettingsTab, page: _keybindingSettingsContainer);
        _tabs.TabChanged += TabsOnTabChanged;

        LoadJsonUi(stage: UI.Shared, resolution: Graphics.Renderer?.GetResolutionString());
        IsHidden = true;
    }

    private void TabsOnTabChanged(Base @base, TabChangeEventArgs tabChangeEventArgs)
    {
        if (_keybindingSettingsTab.IsActive)
        {
            _restoreDefaultKeybindingsButton.IsVisible = true;

            bool controlsAdded = false;

            _controlsTable.SizeToContents = true;

            var row = mKeybindingBtns.Count;
            foreach (var (control, mapping) in (_keybindingEditControls ?? Controls.ActiveControls).Mappings)
            {
                if (!mKeybindingBtns.TryGetValue(control, out var controlButtons))
                {
                    controlsAdded |= AddControlKeybindRow(control, ref row, out controlButtons);
                }

                var bindings = mapping.Bindings;
                for (var bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
                {
                    var binding = bindings[bindingIndex];
                    controlButtons[bindingIndex].Text = Strings.Keys.FormatKeyName(binding.Modifier, binding.Key);
                }
            }

            if (controlsAdded)
            {
                // Current.SaveUIJson(UI.Shared, Name, GetJsonUI(true), Graphics.Renderer?.GetResolutionString());
            }
        }
        else
        {
            _restoreDefaultKeybindingsButton.IsVisible = false;
        }
    }

    private bool AddControlKeybindRow(Control control, ref int row, out Button[] keyButtons)
    {
        if (mKeybindingBtns.TryGetValue(control, out var existingButtons))
        {
            keyButtons = existingButtons;
            return false;
        }

        GameFont? defaultFont = Current.GetFont("sourcesansproblack", 10);

            var offset = row++ * 32;
        var controlName = control.GetControlId();
            var name = controlName?.ToLower() ?? string.Empty;

        if (!Strings.Controls.KeyDictionary.TryGetValue(name, out var localizedControlName))
        {
            var hotbarSlotCount = Options.Instance?.Player.HotbarSlotCount ?? PlayerOptions.DefaultHotbarSlotCount;
            var hotkeySlotHumanNumber = control - Control.HotkeyOffset;
            if (0 < hotkeySlotHumanNumber && hotkeySlotHumanNumber <= hotbarSlotCount)
            {
                localizedControlName = Strings.Controls.HotkeyXLabel.ToString(hotkeySlotHumanNumber);
            }
            else
            {
                localizedControlName = $"ControlName:{controlName}";
            }
        }

        var controlRow = _controlsTable.AddRow();

        var prefix = $"Control{controlName}";
        var label = new Label(controlRow, $"{prefix}Label")
        {
            Alignment = [Alignments.CenterV, Alignments.Right],
            Margin = new Margin(0, 0, 8, 0),
            Text = localizedControlName,
            AutoSizeToContents = true,
            MouseInputEnabled = false,
            Font = defaultFont,
        };

        controlRow.SetCellContents(0, label, enableMouseInput: false);
        // _ = label.SetBounds(14, 11 + offset, 21, 16);
        label.SetTextColor(new Color(255, 255, 255, 255), Label.ControlState.Normal);

        var key1 = new Button(controlRow, $"{prefix}Button1")
        {
            Alignment = [Alignments.Center],
            AutoSizeToContents = false,
            Font = defaultFont,
            MinimumSize = new Point(150, 20),
            Text = string.Empty,
            UserData = new KeyValuePair<Control, int>(control, 0),
        };
        key1.SetTextColor(Color.White, Label.ControlState.Normal);
        key1.SetHoverSound("octave-tap-resonant.wav");
        key1.SetMouseDownSound("octave-tap-warm.wav");
        controlRow.SetCellContents(1, key1, enableMouseInput: true);
        // _ = key1.SetBounds(181, 6 + offset, 120, 28);
        key1.Clicked += Key_Clicked;

        var key2 = new Button(controlRow, $"{prefix}Button2")
        {
            Alignment = [Alignments.Center],
            AutoSizeToContents = false,
            Font = defaultFont,
            MinimumSize = new Point(150, 20),
            Text = string.Empty,
            UserData = new KeyValuePair<Control, int>(control, 1),
        };
        key2.SetTextColor(Color.White, Label.ControlState.Normal);
        key2.SetHoverSound("octave-tap-resonant.wav");
        key2.SetMouseDownSound("octave-tap-warm.wav");
        controlRow.SetCellContents(2, key2, enableMouseInput: true);
        // _ = key2.SetBounds(309, 6 + offset, 120, 28);
        key2.Clicked += Key_Clicked;

        keyButtons = [key1, key2];
        mKeybindingBtns.Add(control, keyButtons);
        return true;
    }

    protected override void OnVisibilityChanged(object? sender, VisibilityChangedEventArgs eventArgs)
    {
        base.OnVisibilityChanged(sender, eventArgs);

        if (eventArgs.IsVisible)
        {
            UpdateWorldScaleControls();
        }
    }

    private void UpdateWorldScaleControls()
    {
        var worldScaleNotches = new double[] { 1, 2, 4 }.Select(n => n * Graphics.MinimumWorldScale).ToList();
        while (worldScaleNotches.Last() < Graphics.MaximumWorldScale)
        {
            worldScaleNotches.Add(worldScaleNotches.Last() * 2);
        }

        if (Options.IsLoaded)
        {
            _worldScale.IsDisabled = false;
            _worldScale.SetToolTipText(null);

            Globals.Database.WorldZoom = (float)MathHelper.Clamp(
                Globals.Database.WorldZoom,
                worldScaleNotches.Min(),
                worldScaleNotches.Max()
            );
        }
        else
        {
            _worldScale.SetToolTipText(Strings.Settings.WorldScaleTooltip);
            _worldScale.IsDisabled = true;
        }

        _worldScale.SetRange(worldScaleNotches.Min(), worldScaleNotches.Max());
        _worldScale.Notches = worldScaleNotches.ToArray();
        _worldScale.Value = Globals.Database.WorldZoom;
    }

    void InterfaceSettings_Clicked(Base sender, ClickedEventArgs arguments)
    {
        _interfaceSettings.Show();
        _informationSettings.Hide();
        _targetingSettings.Hide();
    }

    void InformationSettings_Clicked(Base sender, ClickedEventArgs arguments)
    {
        _interfaceSettings.Hide();
        _informationSettings.Show();
        _targetingSettings.Hide();
    }

    void TargetingSettings_Clicked(Base sender, ClickedEventArgs arguments)
    {
        _interfaceSettings.Hide();
        _informationSettings.Hide();
        _targetingSettings.Show();
        _autoTurnToTarget.IsDisabled = !(Options.Instance?.Player?.EnableAutoTurnToTarget ?? false);
        _autoSoftRetargetOnSelfCast.IsDisabled =
            !(Options.Instance?.Combat?.EnableAutoSelfCastFriendlySpellsWhenTargetingHostile ?? false);
    }

    private void Reset()
    {
        Title = Strings.Settings.Title;

        _gameSettingsTab.Select();

        UpdateWorldScaleControls();
    }

    private readonly HashSet<Keys> _keysDown = [];

    private void OnKeyDown(Keys modifier, Keys key)
    {
        if (_keybindingEditBtn != default)
        {
            _ = _keysDown.Add(key);
        }
    }

    private void OnKeyUp(Keys modifier, Keys key)
    {
        if (_keybindingEditBtn == null)
        {
            return;
        }

        if (key != Keys.None && !_keysDown.Remove(key))
        {
            return;
        }

        _keybindingEditControls.UpdateControl(_keybindingEditControl, _keyEdit, modifier, key);
        _keybindingEditBtn.Text = Strings.Keys.FormatKeyName(modifier, key);

        if (key != Keys.None)
        {
            foreach (var control in _keybindingEditControls.Mappings)
            {
                if (control.Key == _keybindingEditControl)
                {
                    continue;
                }

                var bindings = control.Value.Bindings;
                for (var bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
                {
                    var binding = bindings[bindingIndex];

                    if (binding.Modifier == modifier && binding.Key == key)
                    {
                        // Remove this mapping.
                        _keybindingEditControls.UpdateControl(control.Key, bindingIndex, Keys.None, Keys.None);

                        // Update UI.
                        mKeybindingBtns[control.Key][bindingIndex].Text = Strings.Keys.KeyDictionary[Enum.GetName(typeof(Keys), Keys.None)?.ToLower() ?? string.Empty];
                    }
                }
            }

            _keybindingEditBtn.PlayHoverSound();
        }

        _keybindingEditBtn = null;
        _keysDown.Clear();
        Interface.GwenInput.HandleInput = true;
    }

    // Methods.
    public void Update()
    {
        if (!IsHidden &&
            _keybindingEditBtn != null &&
            _keybindingListeningTimer < Timing.Global.MillisecondsUtc)
        {
            OnKeyUp(Keys.None, Keys.None);
        }
    }

    public void Show(bool returnToMenu = false)
    {
        // Take over all input when we're in-game.
        if (Globals.GameState == GameStates.InGame)
        {
            MakeModal(true);
        }

        // Game Settings.
        _autoCloseWindowsCheckbox.IsChecked = Globals.Database.HideOthersOnWindowOpen;
        _autoToggleChatLogCheckbox.IsChecked = Globals.Database.AutoToggleChatLog;
        _showHealthAsPercentageCheckbox.IsChecked = Globals.Database.ShowHealthAsPercentage;
        _showManaAsPercentageCheckbox.IsChecked = Globals.Database.ShowManaAsPercentage;
        _showExperienceAsPercentageCheckbox.IsChecked = Globals.Database.ShowExperienceAsPercentage;
        _simplifiedEscapeMenu.IsChecked = Globals.Database.SimplifiedEscapeMenu;
        _friendOverheadInfoCheckbox.IsChecked = Globals.Database.FriendOverheadInfo;
        _guildMemberOverheadInfoCheckbox.IsChecked = Globals.Database.GuildMemberOverheadInfo;
        _myOverheadInfoCheckbox.IsChecked = Globals.Database.MyOverheadInfo;
        _npcOverheadInfoCheckbox.IsChecked = Globals.Database.NpcOverheadInfo;
        _partyMemberOverheadInfoCheckbox.IsChecked = Globals.Database.PartyMemberOverheadInfo;
        _playerOverheadInfoCheckbox.IsChecked = Globals.Database.PlayerOverheadInfo;
        _friendOverheadHpBarCheckbox.IsChecked = Globals.Database.FriendOverheadHpBar;
        _guildMemberOverheadHpBarCheckbox.IsChecked = Globals.Database.GuildMemberOverheadHpBar;
        _myOverheadHpBarCheckbox.IsChecked = Globals.Database.MyOverheadHpBar;
        _mpcOverheadHpBarCheckbox.IsChecked = Globals.Database.NpcOverheadHpBar;
        _partyMemberOverheadHpBarCheckbox.IsChecked = Globals.Database.PartyMemberOverheadHpBar;
        _playerOverheadHpBarCheckbox.IsChecked = Globals.Database.PlayerOverheadHpBar;
        _stickyTarget.IsChecked = Globals.Database.StickyTarget;
        _autoTurnToTarget.IsChecked = Globals.Database.AutoTurnToTarget;
        _autoSoftRetargetOnSelfCast.IsChecked = Globals.Database.AutoSoftRetargetOnSelfCast;
        _typewriterCheckbox.IsChecked = Globals.Database.TypewriterBehavior == Enums.TypewriterBehavior.Word;

        // Video Settings.
        _fullscreenCheckbox.IsChecked = Globals.Database.FullScreen;
        _lightingEnabledCheckbox.IsChecked = Globals.Database.EnableLighting;

        // _uiScale.Value = Globals.Database.UIScale;

        if (Graphics.Renderer?.GetValidVideoModes().Count > 0)
        {
            string resolutionLabel;
            if (Graphics.Renderer.HasOverrideResolution)
            {
                resolutionLabel = Strings.Settings.ResolutionCustom;

                _customResolutionMenuItem ??= _resolutionList.AddItem(Strings.Settings.ResolutionCustom);
                _customResolutionMenuItem.Show();
            }
            else
            {
                var validVideoModes = Graphics.Renderer.GetValidVideoModes();
                var targetResolution = Globals.Database.TargetResolution;
                resolutionLabel = targetResolution < 0 || validVideoModes.Count <= targetResolution ? Strings.Settings.ResolutionCustom : validVideoModes[Globals.Database.TargetResolution];
            }

            _resolutionList.SelectByText(resolutionLabel);
        }

        switch (Globals.Database.TargetFps)
        {
            case -1: // Unlimited.
                _fpsList.SelectByText(Strings.Settings.UnlimitedFps);

                break;
            case 0: // Vertical Sync.
                _fpsList.SelectByText(Strings.Settings.Vsync);

                break;
            case 1: // 30 Frames per second.
                _fpsList.SelectByText(Strings.Settings.Fps30);

                break;
            case 2: // 60 Frames per second.
                _fpsList.SelectByText(Strings.Settings.Fps60);

                break;
            case 3: // 90 Frames per second.
                _fpsList.SelectByText(Strings.Settings.Fps90);

                break;
            case 4: // 120 Frames per second.
                _fpsList.SelectByText(Strings.Settings.Fps120);

                break;
            default:
                _fpsList.SelectByText(Strings.Settings.Vsync);

                break;
        }

        // Audio Settings.
        _previousMusicVolume = Globals.Database.MusicVolume;
        _previousSoundVolume = Globals.Database.SoundVolume;
        _musicSlider.Value = Globals.Database.MusicVolume;
        _soundEffectsSlider.Value = Globals.Database.SoundVolume;

        // Control Settings.
        _keybindingEditControls = new Controls(Controls.ActiveControls);

        // Settings Window is not hidden anymore.
        base.Show();

        // Load every GUI element to their default state when showing up the settings window (pressed tabs, containers, etc.)
        Reset();

        // Set up whether we're supposed to return to the previous menu.
        _returnToMenu = returnToMenu;
    }

    public override void Hide()
    {
        // Hide the current window.
        base.Hide();
        RemoveModal();

        // Return to our previous menus (or not) depending on gamestate and the method we'd been opened.
        if (_returnToMenu)
        {
            switch (Globals.GameState)
            {
                case GameStates.Menu:
                    _mainMenu?.Show();
                    break;

                case GameStates.InGame:
                    _escapeMenu?.Show();
                    break;

                default:
                    throw new NotImplementedException();
            }

            _returnToMenu = false;
        }
    }

    // Input Handlers
    private void MusicSliderOnValueChanged(Base sender, ValueChangedEventArgs<double> arguments)
    {
        Globals.Database.MusicVolume = (int)arguments.Value;
        ApplicationContext.CurrentContext.Logger.LogInformation("Music volume set to {MusicVolume}", arguments.Value);
        Audio.UpdateGlobalVolume();
    }

    private void SoundEffectsSliderOnValueChanged(Base sender, ValueChangedEventArgs<double> arguments)
    {
        Globals.Database.SoundVolume = (int)arguments.Value;
        ApplicationContext.CurrentContext.Logger.LogInformation("Sound volume set to {SoundVolume}", arguments.Value);
        Audio.UpdateGlobalVolume();
    }

    private void Key_Clicked(Base sender, ClickedEventArgs arguments)
    {
        EditKeyPressed((Button)sender);
    }

    private void EditKeyPressed(Button sender)
    {
        if (_keybindingEditBtn == null)
        {
            sender.Text = Strings.Controls.Listening;
            _keyEdit = ((KeyValuePair<Control, int>)sender.UserData).Value;
            _keybindingEditControl = ((KeyValuePair<Control, int>)sender.UserData).Key;
            _keybindingEditBtn = sender;
            Interface.GwenInput.HandleInput = false;
            _keybindingListeningTimer = Timing.Global.MillisecondsUtc + 3000;
        }
    }

    private void RestoreDefaultKeybindingsButton_Clicked(Base sender, ClickedEventArgs arguments)
    {
        if (_keybindingEditControls is not {} controls)
        {
            return;
        }

        controls.ResetDefaults();
        foreach (Control control in GameInput.Current.AllControls)
        {
            if (!controls.TryGetMappingFor(control, out var mapping))
            {
                continue;
            }

            for (var bindingIndex = 0; bindingIndex < mapping.Bindings.Count; bindingIndex++)
            {
                var binding = mapping.Bindings[bindingIndex];
                mKeybindingBtns[control][bindingIndex].Text = Strings.Keys.FormatKeyName(binding.Modifier, binding.Key);
            }
        }
    }

    private void SettingsApplyBtn_Clicked(Base sender, ClickedEventArgs arguments)
    {
        var shouldReset = false;

        // Game Settings.
        Globals.Database.HideOthersOnWindowOpen = _autoCloseWindowsCheckbox.IsChecked;
        Globals.Database.AutoToggleChatLog = _autoToggleChatLogCheckbox.IsChecked;
        Globals.Database.ShowExperienceAsPercentage = _showExperienceAsPercentageCheckbox.IsChecked;
        Globals.Database.ShowHealthAsPercentage = _showHealthAsPercentageCheckbox.IsChecked;
        Globals.Database.ShowManaAsPercentage = _showManaAsPercentageCheckbox.IsChecked;
        Globals.Database.SimplifiedEscapeMenu = _simplifiedEscapeMenu.IsChecked;
        Globals.Database.FriendOverheadInfo = _friendOverheadInfoCheckbox.IsChecked;
        Globals.Database.GuildMemberOverheadInfo = _guildMemberOverheadInfoCheckbox.IsChecked;
        Globals.Database.MyOverheadInfo = _myOverheadInfoCheckbox.IsChecked;
        Globals.Database.NpcOverheadInfo = _npcOverheadInfoCheckbox.IsChecked;
        Globals.Database.PartyMemberOverheadInfo = _partyMemberOverheadInfoCheckbox.IsChecked;
        Globals.Database.PlayerOverheadInfo = _playerOverheadInfoCheckbox.IsChecked;
        Globals.Database.FriendOverheadHpBar = _friendOverheadHpBarCheckbox.IsChecked;
        Globals.Database.GuildMemberOverheadHpBar = _guildMemberOverheadHpBarCheckbox.IsChecked;
        Globals.Database.MyOverheadHpBar = _myOverheadHpBarCheckbox.IsChecked;
        Globals.Database.NpcOverheadHpBar = _mpcOverheadHpBarCheckbox.IsChecked;
        Globals.Database.PartyMemberOverheadHpBar = _partyMemberOverheadHpBarCheckbox.IsChecked;
        Globals.Database.PlayerOverheadHpBar = _playerOverheadHpBarCheckbox.IsChecked;
        Globals.Database.StickyTarget = _stickyTarget.IsChecked;
        Globals.Database.AutoTurnToTarget = _autoTurnToTarget.IsChecked;
        Globals.Database.AutoSoftRetargetOnSelfCast = _autoSoftRetargetOnSelfCast.IsChecked;
        Globals.Database.TypewriterBehavior = _typewriterCheckbox.IsChecked ? Enums.TypewriterBehavior.Word : Enums.TypewriterBehavior.Off;

        // Video Settings.
        Globals.Database.WorldZoom = (float)_worldScale.Value;

        var resolution = _resolutionList.SelectedItem;
        var validVideoModes = Graphics.Renderer?.GetValidVideoModes();
        var targetResolution = validVideoModes?.FindIndex(videoMode => string.Equals(videoMode, resolution.Text)) ?? -1;
        var newFps = 0;

        Globals.Database.EnableLighting = _lightingEnabledCheckbox.IsChecked;

        if (targetResolution > -1)
        {
            shouldReset = Globals.Database.TargetResolution != targetResolution || Graphics.Renderer?.HasOverrideResolution == true;
            Globals.Database.TargetResolution = targetResolution;
        }

        if (Globals.Database.FullScreen != _fullscreenCheckbox.IsChecked)
        {
            Globals.Database.FullScreen = _fullscreenCheckbox.IsChecked;
            shouldReset = true;
        }

        if (_fpsList.SelectedItem.Text == Strings.Settings.UnlimitedFps)
        {
            newFps = -1;
        }
        else if (_fpsList.SelectedItem.Text == Strings.Settings.Fps30)
        {
            newFps = 1;
        }
        else if (_fpsList.SelectedItem.Text == Strings.Settings.Fps60)
        {
            newFps = 2;
        }
        else if (_fpsList.SelectedItem.Text == Strings.Settings.Fps90)
        {
            newFps = 3;
        }
        else if (_fpsList.SelectedItem.Text == Strings.Settings.Fps120)
        {
            newFps = 4;
        }

        if (newFps != Globals.Database.TargetFps)
        {
            shouldReset = true;
            Globals.Database.TargetFps = newFps;
        }

        // Audio Settings.
        Globals.Database.MusicVolume = (int)_musicSlider.Value;
        Globals.Database.SoundVolume = (int)_soundEffectsSlider.Value;
        Audio.UpdateGlobalVolume();

        // Control Settings.
        var activeControls = _keybindingEditControls ?? Controls.ActiveControls;
        Controls.ActiveControls = activeControls;
        activeControls.TrySave();

        // Save Preferences.
        Globals.Database.SavePreferences();

        if (shouldReset && Graphics.Renderer != default)
        {
            _customResolutionMenuItem?.Hide();
            Graphics.Renderer.OverrideResolution = Resolution.Empty;
            Graphics.Renderer.Init();
        }

        // Hide the currently opened window.
        Hide();
    }

    private void CancelPendingChangesButton_Clicked(Base sender, ClickedEventArgs arguments)
    {
        // Update previously saved values in order to discard changes.
        Globals.Database.MusicVolume = _previousMusicVolume;
        Globals.Database.SoundVolume = _previousSoundVolume;
        Audio.UpdateGlobalVolume();
        _keybindingEditControls = new Controls(Controls.ActiveControls);

        // Hide our current window.
        Hide();
    }
}
