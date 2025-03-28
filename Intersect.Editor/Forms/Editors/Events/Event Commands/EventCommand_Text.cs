using Intersect.Editor.Content;
using Intersect.Editor.Localization;
using Intersect.Framework.Core.GameObjects.Events.Commands;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors.Events.Event_Commands;


public partial class EventCommandText : UserControl
{

    private readonly FrmEvent mEventEditor;

    private ShowTextCommand mMyCommand;

    public EventCommandText(ShowTextCommand refCommand, FrmEvent editor)
    {
        InitializeComponent();
        mMyCommand = refCommand;
        mEventEditor = editor;
        InitLocalization();
        txtShowText.Text = mMyCommand.Text;
        cmbFace.Items.Clear();
        cmbFace.Items.Add(Strings.General.None);
        cmbFace.Items.AddRange(GameContentManager.GetSmartSortedTextureNames(GameContentManager.TextureType.Face));
        cmbFace.SelectedIndex = Math.Max(0, cmbFace.Items.IndexOf(TextUtils.NullToNone(mMyCommand.Face)));

        UpdateFacePreview();
    }

    private void InitLocalization()
    {
        grpShowText.Text = Strings.EventShowText.title;
        lblText.Text = Strings.EventShowText.text;
        lblFace.Text = Strings.EventShowText.face;
        lblCommands.Text = Strings.EventShowText.commands;
        btnSave.Text = Strings.EventShowText.okay;
        btnCancel.Text = Strings.EventShowText.cancel;
    }

    private void UpdateFacePreview()
    {
        if (File.Exists("resources/faces/" + cmbFace.Text))
        {
            pnlFace.BackgroundImage = new Bitmap("resources/faces/" + cmbFace.Text);
        }
        else
        {
            pnlFace.BackgroundImage = null;
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        mMyCommand.Text = txtShowText.Text;
        mMyCommand.Face = TextUtils.SanitizeNone(cmbFace?.Text);
        mEventEditor.FinishCommandEdit();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        mEventEditor.CancelCommandEdit();
    }

    private void cmbFace_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateFacePreview();
    }

    private void lblCommands_Click(object sender, EventArgs e)
    {
        BrowserUtils.Open("http://www.ascensiongamedev.com/community/topic/749-event-text-variables/");
    }

}
