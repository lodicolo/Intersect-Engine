using Intersect.Editor.Core;
using Intersect.Editor.Localization;

namespace Intersect.Editor.Forms;


public partial class FrmProgress : Form
{

    private int mProgressVal;

    private bool mShouldClose;

    private bool mShowCancelBtn;

    private string mStatusText;

    public FrmProgress()
    {
        InitializeComponent();
        Icon = Program.Icon;

        InitLocalization();
    }

    private void InitLocalization()
    {
        btnCancel.Text = Strings.ProgressForm.cancel;
    }

    public void SetTitle(string title)
    {
        Text = title;
    }

    public void SetProgress(string label, int progress, bool showCancel)
    {
        mStatusText = label;
        if (progress < 0)
        {
            mProgressVal = 0;
            progressBar.Style = ProgressBarStyle.Marquee;
        }
        else
        {
            mProgressVal = progress;
            progressBar.Style = ProgressBarStyle.Blocks;
        }

        mShowCancelBtn = showCancel;
        tmrUpdater_Tick(null, null);
        Application.DoEvents();
    }

    public void NotifyClose()
    {
        mShouldClose = true;
    }

    private void tmrUpdater_Tick(object sender, EventArgs e)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        if (!InvokeRequired)
        {
            lblStatus.Text = mStatusText;
            progressBar.Value = Math.Min(100, mProgressVal);
            btnCancel.Visible = mShowCancelBtn;
            if (mShouldClose)
            {
                Close();
            }
        }
    }
}
