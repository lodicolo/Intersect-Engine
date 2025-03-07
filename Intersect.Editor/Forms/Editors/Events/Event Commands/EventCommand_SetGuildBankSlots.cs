﻿using Intersect.Enums;
using Intersect.Editor.Localization;
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.Framework.Core.GameObjects.Events.Commands;
using Intersect.Framework.Core.GameObjects.Variables;

namespace Intersect.Editor.Forms.Editors.Events.Event_Commands;


public partial class EventCommandSetGuildBankSlots : UserControl
{

    private readonly FrmEvent mEventEditor;

    private EventPage mCurrentPage;

    private SetGuildBankSlotsCommand mMyCommand;

    public EventCommandSetGuildBankSlots(SetGuildBankSlotsCommand refCommand, EventPage refPage, FrmEvent editor)
    {
        InitializeComponent();
        mMyCommand = refCommand;
        mEventEditor = editor;
        mCurrentPage = refPage;
        InitLocalization();

        rdoGlobalVariable.Checked = mMyCommand.VariableType == VariableType.ServerVariable;
        rdoGuildVariable.Checked = mMyCommand.VariableType == VariableType.GuildVariable;

        SetupAmountInput();
    }

    private void InitLocalization()
    {
        grpGuildSlots.Text = Strings.EventGuildSetBankSlotsCount.title;
        lblVariable.Text = Strings.EventGuildSetBankSlotsCount.Variable;

        rdoPlayerVariable.Text = Strings.EventGuildSetBankSlotsCount.PlayerVariable;
        rdoGlobalVariable.Text = Strings.EventGuildSetBankSlotsCount.ServerVariable;
        rdoGuildVariable.Text = Strings.EventGuildSetBankSlotsCount.GuildVariable;

        btnSave.Text = Strings.EventGuildSetBankSlotsCount.okay;
        btnCancel.Text = Strings.EventGuildSetBankSlotsCount.cancel;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (rdoPlayerVariable.Checked)
        {
            mMyCommand.VariableType = VariableType.PlayerVariable;
            mMyCommand.VariableId = PlayerVariableDescriptor.IdFromList(cmbVariable.SelectedIndex, VariableDataType.Integer);
        }
        else if (rdoGlobalVariable.Checked)
        {
            mMyCommand.VariableType = VariableType.ServerVariable;
            mMyCommand.VariableId = ServerVariableDescriptor.IdFromList(cmbVariable.SelectedIndex, VariableDataType.Integer);
        }
        else if (rdoGuildVariable.Checked)
        {
            mMyCommand.VariableType = VariableType.GuildVariable;
            mMyCommand.VariableId = GuildVariableDescriptor.IdFromList(cmbVariable.SelectedIndex, VariableDataType.Integer);
        }

        mEventEditor.FinishCommandEdit();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        mEventEditor.CancelCommandEdit();
    }

    private void VariableBlank()
    {
        if (cmbVariable.Items.Count > 0)
        {
            cmbVariable.SelectedIndex = 0;
        }
        else
        {
            cmbVariable.SelectedIndex = -1;
            cmbVariable.Text = string.Empty;
        }
    }

    private void rdoPlayerVariable_CheckedChanged(object sender, EventArgs e)
    {
        SetupAmountInput();
    }

    private void rdoGlobalVariable_CheckedChanged(object sender, EventArgs e)
    {
        SetupAmountInput();
    }

    private void rdoGuildVariable_CheckedChanged(object sender, EventArgs e)
    {
        SetupAmountInput();
    }

    private void SetupAmountInput()
    {

        cmbVariable.Items.Clear();
        if (rdoPlayerVariable.Checked)
        {
            cmbVariable.Items.AddRange(PlayerVariableDescriptor.GetNamesByType(VariableDataType.Integer));
            // Do not update if the wrong type of variable is saved
            if (mMyCommand.VariableType == VariableType.PlayerVariable)
            {
                var index = PlayerVariableDescriptor.ListIndex(mMyCommand.VariableId, VariableDataType.Integer);
                if (index > -1)
                {
                    cmbVariable.SelectedIndex = index;
                }
                else
                {
                    VariableBlank();
                }
            }
            else
            {
                VariableBlank();
            }
        }
        else if (rdoGlobalVariable.Checked)
        {
            cmbVariable.Items.AddRange(ServerVariableDescriptor.GetNamesByType(VariableDataType.Integer));
            // Do not update if the wrong type of variable is saved
            if (mMyCommand.VariableType == VariableType.ServerVariable)
            {
                var index = ServerVariableDescriptor.ListIndex(mMyCommand.VariableId, VariableDataType.Integer);
                if (index > -1)
                {
                    cmbVariable.SelectedIndex = index;
                }
                else
                {
                    VariableBlank();
                }
            }
            else
            {
                VariableBlank();
            }
        }
        if (rdoGuildVariable.Checked)
        {
            cmbVariable.Items.AddRange(GuildVariableDescriptor.GetNamesByType(VariableDataType.Integer));
            // Do not update if the wrong type of variable is saved
            if (mMyCommand.VariableType == VariableType.GuildVariable)
            {
                var index = GuildVariableDescriptor.ListIndex(mMyCommand.VariableId, VariableDataType.Integer);
                if (index > -1)
                {
                    cmbVariable.SelectedIndex = index;
                }
                else
                {
                    VariableBlank();
                }
            }
            else
            {
                VariableBlank();
            }
        }
    }
}
