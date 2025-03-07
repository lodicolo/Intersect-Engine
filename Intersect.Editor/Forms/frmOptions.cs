﻿using System.Globalization;
using Intersect.Editor.Core;
using Intersect.Editor.Localization;

namespace Intersect.Editor.Forms;


public partial class FrmOptions : Form
{

    public FrmOptions()
    {
        InitializeComponent();
        Icon = Program.Icon;

        InitForm();
        InitLocalization();
    }

    private void InitForm()
    {

        // Show only the General tab by default.
        HidePanels();
        pnlGeneral.Visible = true;

        // Load settings for the General tab
        var suppressTilesetWarning = Preferences.LoadPreference("SuppressTextureWarning");

        if (suppressTilesetWarning == "")
        {
            chkSuppressTilesetWarning.Checked = false;
        }
        else
        {
            chkSuppressTilesetWarning.Checked = Convert.ToBoolean(suppressTilesetWarning);
        }

        chkCursorSprites.Checked = Preferences.EnableCursorSprites;

        txtGamePath.Text = Preferences.LoadPreference("ClientPath");

        // Load settings for the Update tab
        var packageUpdateAssets = Preferences.LoadPreference("PackageUpdateAssets");
        if (packageUpdateAssets == "")
        {
            chkPackageAssets.Checked = false;
        }
        else
        {
            chkPackageAssets.Checked = Convert.ToBoolean(packageUpdateAssets);
        }

        var soundBatchSize = Preferences.LoadPreference("SoundPackSize");
        if (soundBatchSize != "")
        {
            nudSoundBatch.Value = Convert.ToInt32(soundBatchSize);
        }

        var musicBatchSize = Preferences.LoadPreference("MusicPackSize");
        if (musicBatchSize != "")
        {
            nudMusicBatch.Value = Convert.ToInt32(musicBatchSize);
        }

        var texturePackSize = Preferences.LoadPreference("TexturePackSize");
        if (texturePackSize != "")
        {
            cmbTextureSize.SelectedIndex = cmbTextureSize.FindStringExact(texturePackSize);
        }
        else
        {
            cmbTextureSize.SelectedIndex = cmbTextureSize.FindStringExact("2048");
        }
    }

    private void InitLocalization()
    {
        Text = Strings.Options.title;
        btnGeneralOptions.Text = Strings.Options.generaltab.ToString(Application.ProductVersion);
        chkSuppressTilesetWarning.Text = Strings.Options.tilesetwarning;
        chkCursorSprites.Text = Strings.Options.CursorSprites;
        grpClientPath.Text = Strings.Options.pathgroup;
        btnBrowseClient.Text = Strings.Options.browsebtn;
        btnUpdateOptions.Text = Strings.Options.UpdateTab;
        grpAssetPackingOptions.Text = Strings.Options.PackageOptions;
        lblMusicBatch.Text = Strings.Options.MusicPackSize;
        lblSoundBatch.Text = Strings.Options.SoundPackSize;
        lblTextureSize.Text = Strings.Options.TextureSize;

    }

    private void frmOptions_FormClosing(object sender, FormClosingEventArgs e)
    {
        Preferences.SavePreference("SuppressTextureWarning", chkSuppressTilesetWarning.Checked.ToString());
        Preferences.EnableCursorSprites = chkCursorSprites.Checked;
        Preferences.SavePreference("ClientPath", txtGamePath.Text);
        Preferences.SavePreference("PackageUpdateAssets", chkPackageAssets.Checked.ToString());
        Preferences.SavePreference("SoundPackSize", nudSoundBatch.Value.ToString(CultureInfo.InvariantCulture));
        Preferences.SavePreference("MusicPackSize", nudMusicBatch.Value.ToString(CultureInfo.InvariantCulture));
        Preferences.SavePreference("TexturePackSize", cmbTextureSize.GetItemText(cmbTextureSize.SelectedItem));
    }

    private void btnBrowseClient_Click(object sender, EventArgs e)
    {
        var dialogue = new OpenFileDialog()
        {
            Title = Strings.Options.dialogueheader,
            CheckFileExists = true,
            CheckPathExists = true,
            DefaultExt = "exe",
            Filter = "(*.exe)|*.exe|" + Strings.Options.dialogueallfiles + "(*.*)|*.*",
            RestoreDirectory = true,
            ReadOnlyChecked = true,
            ShowReadOnly = true
        };

        using (dialogue)
        {
            if (dialogue.ShowDialog() == DialogResult.OK)
            {
                txtGamePath.Text = dialogue.FileName;
            }
        }
    }

    private void HidePanels()
    {
        pnlGeneral.Visible = false;
        pnlUpdate.Visible = false;
    }

    private void btnGeneralOptions_Click(object sender, EventArgs e)
    {
        // Hide other panels.
        HidePanels();

        pnlGeneral.Visible = true;
    }

    private void btnUpdateOptions_Click(object sender, EventArgs e)
    {
        // Hide other panels.
        HidePanels();

        pnlUpdate.Visible = true;
    }

    private void nudSoundBatch_ValueChanged(object sender, EventArgs e)
    {

    }
}
