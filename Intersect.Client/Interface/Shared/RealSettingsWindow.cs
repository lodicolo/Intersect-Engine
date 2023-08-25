using System;
using Intersect.Client.Core;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Control.EventArguments;
using Intersect.Client.General;
using Intersect.Client.Interface.Game;
using Intersect.Client.Interface.Menu;
using Intersect.Client.Localization;

using static Intersect.Client.Framework.File_Management.GameContentManager;

namespace Intersect.Client.Interface.Shared
{
    public abstract class LazyLoadedWindowControl : WindowControl
    {
        private object _initializationLock;
        private bool _initialized;

        protected LazyLoadedWindowControl(UI stage, Base parent, string title, bool modal, string name)
            : base(stage, parent, title, modal, name)
        {
            _initializationLock = new object();

            IsHidden = true;
        }

        protected void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (_initializationLock)
            {
                Initialize();

                ReloadJson(Graphics.Renderer.GetResolutionString());
            }

            _initialized = true;
        }
        
        protected virtual void Initialize() {}

        protected override void Render(Framework.Gwen.Skin.Base skin)
        {
            EnsureInitialized();
            base.Render(skin);
        }
    }
    
    public class RealSettingsWindow : LazyLoadedWindowControl
    {
        private readonly MainMenu mMainMenu;
        private readonly EscapeMenu mEscapeMenu;
        private bool _returnToMenu;

        public RealSettingsWindow(Canvas canvas, MainMenu mainMenu, EscapeMenu escapeMenu)
            : base(UI.Shared, canvas, Strings.Settings.Title, false, nameof(RealSettingsWindow))
        {
            mMainMenu = mainMenu;
            mEscapeMenu = escapeMenu;
            
            DisableResizing();

            IsHidden = true;
        }

        public void Update()
        {
            if (IsVisible /*&&
                mKeybindingEditBtn != null &&
                mKeybindingListeningTimer < Timing.Global.MillisecondsUtc*/)
            {
                /*OnKeyUp(Keys.None, Keys.None);*/
            }
        }

        public override void Hide()
        {
            base.Hide();
            RemoveModal();

            // Return to our previous menus (or not) depending on gamestate and the method we'd been opened.
            if (!_returnToMenu)
            {
                return;
            }

            switch (Globals.GameState)
            {
                case GameStates.Menu:
                    mMainMenu?.Show();
                    break;

                case GameStates.InGame:
                    mEscapeMenu?.Show();
                    break;

                case GameStates.Intro:
                case GameStates.Loading:
                case GameStates.Error:
                default:
                    throw new NotImplementedException();
            }

            _returnToMenu = false;
        }

        public override void Show() => Show(true);

        public void Show(bool returnToMenu)
        {
            // Dim background when we're in-game
            MakeModal(Globals.GameState == GameStates.InGame);

            ResetPositions();

            base.Show();

            _returnToMenu = returnToMenu;
        }

        private void ResetPositions()
        {
            // mSettingsHeader.SetText(Strings.Settings.Title);
            //
            // // Containers.
            // mGameSettingsContainer.Show();
            // mVideoSettingsContainer.Hide();
            // mAudioSettingsContainer.Hide();
            // mKeybindingSettingsContainer.Hide();
            //
            // // Tabs.
            // mGameSettingsTab.Show();
            // mVideoSettingsTab.Show();
            // mAudioSettingsTab.Show();
            // mKeybindingSettingsTab.Show();
            //
            // // Disable the GameSettingsTab to fake it being selected visually by default.
            // mGameSettingsTab.Disable();
            // mVideoSettingsTab.Enable();
            // mAudioSettingsTab.Enable();
            // mKeybindingSettingsTab.Enable();
            //
            // // Buttons.
            // mSettingsApplyBtn.Show();
            // mSettingsCancelBtn.Show();
            // mKeybindingRestoreBtn.Hide();
        }

        private Button mSettingsApplyBtn;
        private Button mSettingsCancelBtn;
        // private LabeledHorizontalSlider mUiScaleSlider;
        private HorizontalSlider mUiScaleSlider;

        protected override void Initialize()
        {
            base.Initialize();
            

            // Apply Button.
            mSettingsApplyBtn = new Button(this, "SettingsApplyBtn");
            mSettingsApplyBtn.SetText(Strings.Settings.Apply);
            mSettingsApplyBtn.Clicked += SettingsApplyBtn_Clicked;

            // Cancel Button.
            mSettingsCancelBtn = new Button(this, "SettingsCancelBtn");
            mSettingsCancelBtn.SetText(Strings.Settings.Cancel);
            mSettingsCancelBtn.Clicked += SettingsCancelBtn_Clicked;

            mUiScaleSlider = new HorizontalSlider(this, "UiScaleSlider")
            {
                Max = 2,
                Min = 0.5,
                NotchCount = 7,
                SnapToNotches = true,
                Value = 1,
            };
        }

        private void SettingsApplyBtn_Clicked(Base sender, ClickedEventArgs arguments)
        {
            var shouldReset = false;

            // // Game Settings.
            // Globals.Database.HideOthersOnWindowOpen = mAutoCloseWindowsCheckbox.IsChecked;
            // Globals.Database.AutoToggleChatLog = mAutoToggleChatLogCheckbox.IsChecked;
            // Globals.Database.ShowExperienceAsPercentage = mShowExperienceAsPercentageCheckbox.IsChecked;
            // Globals.Database.ShowHealthAsPercentage = mShowHealthAsPercentageCheckbox.IsChecked;
            // Globals.Database.ShowManaAsPercentage = mShowManaAsPercentageCheckbox.IsChecked;
            // Globals.Database.FriendOverheadInfo = mFriendOverheadInfoCheckbox.IsChecked;
            // Globals.Database.GuildMemberOverheadInfo = mGuildMemberOverheadInfoCheckbox.IsChecked;
            // Globals.Database.MyOverheadInfo = mMyOverheadInfoCheckbox.IsChecked;
            // Globals.Database.NpcOverheadInfo = mNpcOverheadInfoCheckbox.IsChecked;
            // Globals.Database.PartyMemberOverheadInfo = mPartyMemberOverheadInfoCheckbox.IsChecked;
            // Globals.Database.PlayerOverheadInfo = mPlayerOverheadInfoCheckbox.IsChecked;
            // Globals.Database.FriendOverheadHpBar = mFriendOverheadHpBarCheckbox.IsChecked;
            // Globals.Database.GuildMemberOverheadHpBar = mGuildMemberOverheadHpBarCheckbox.IsChecked;
            // Globals.Database.MyOverheadHpBar= mMyOverheadHpBarCheckbox.IsChecked;
            // Globals.Database.NpcOverheadHpBar= mNpcOverheadHpBarCheckbox.IsChecked;
            // Globals.Database.PartyMemberOverheadHpBar = mPartyMemberOverheadHpBarCheckbox.IsChecked;
            // Globals.Database.PlayerOverheadHpBar = mPlayerOverheadHpBarCheckbox.IsChecked;
            // Globals.Database.StickyTarget = mStickyTarget.IsChecked;
            // Globals.Database.AutoTurnToTarget = mAutoTurnToTarget.IsChecked;
            // Globals.Database.TypewriterBehavior = mTypewriterCheckbox.IsChecked ? Enums.TypewriterBehavior.Word : Enums.TypewriterBehavior.Off;
            //
            // // Video Settings.
            // var resolution = mResolutionList.SelectedItem;
            // var validVideoModes = Graphics.Renderer.GetValidVideoModes();
            // var targetResolution = validVideoModes?.FindIndex(videoMode =>
            //     string.Equals(videoMode, resolution.Text)) ?? -1;
            // var newFps = 0;
            //
            // Globals.Database.EnableLighting = mLightingEnabledCheckbox.IsChecked;
            //
            // if (targetResolution > -1)
            // {
            //     shouldReset = Globals.Database.TargetResolution != targetResolution ||
            //                   Graphics.Renderer.HasOverrideResolution;
            //     Globals.Database.TargetResolution = targetResolution;
            // }
            //
            // if (Globals.Database.FullScreen != mFullscreenCheckbox.IsChecked)
            // {
            //     Globals.Database.FullScreen = mFullscreenCheckbox.IsChecked;
            //     shouldReset = true;
            // }
            //
            // if (mFpsList.SelectedItem.Text == Strings.Settings.UnlimitedFps)
            // {
            //     newFps = -1;
            // }
            // else if (mFpsList.SelectedItem.Text == Strings.Settings.Fps30)
            // {
            //     newFps = 1;
            // }
            // else if (mFpsList.SelectedItem.Text == Strings.Settings.Fps60)
            // {
            //     newFps = 2;
            // }
            // else if (mFpsList.SelectedItem.Text == Strings.Settings.Fps90)
            // {
            //     newFps = 3;
            // }
            // else if (mFpsList.SelectedItem.Text == Strings.Settings.Fps120)
            // {
            //     newFps = 4;
            // }
            //
            // if (newFps != Globals.Database.TargetFps)
            // {
            //     shouldReset = true;
            //     Globals.Database.TargetFps = newFps;
            // }
            //
            // // Audio Settings.
            // Globals.Database.MusicVolume = (int)mMusicSlider.Value;
            // Globals.Database.SoundVolume = (int)mSoundSlider.Value;
            // Audio.UpdateGlobalVolume();
            //
            // // Control Settings.
            // Controls.ActiveControls = mKeybindingEditControls;
            // Controls.ActiveControls.Save();
            //
            // // Save Preferences.
            // Globals.Database.SavePreferences();
            //
            // if (shouldReset)
            // {
            //     mCustomResolutionMenuItem?.Hide();
            //     Graphics.Renderer.OverrideResolution = Resolution.Empty;
            //     Graphics.Renderer.Init();
            // }

            // Hide the currently opened window.
            Hide();
        }

        private void SettingsCancelBtn_Clicked(Base sender, ClickedEventArgs arguments)
        {
            // Update previously saved values in order to discard changes.
            // Globals.Database.MusicVolume = mPreviousMusicVolume;
            // Globals.Database.SoundVolume = mPreviousSoundVolume;
            Audio.UpdateGlobalVolume();
            // mKeybindingEditControls = new Controls(Controls.ActiveControls);

            // Hide our current window.
            Hide();
        }
    }
}