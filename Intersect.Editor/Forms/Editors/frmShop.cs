using DarkUI.Forms;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Editor.Forms.Editors;


public partial class FrmShop : EditorForm
{

    private List<ShopDescriptor> mChanged = new List<ShopDescriptor>();

    private string mCopiedItem;

    private ShopDescriptor mEditorItem;

    private List<string> mKnownFolders = new List<string>();

    public FrmShop()
    {
        ApplyHooks();
        InitializeComponent();
        Icon = Program.Icon;
        _btnSave = btnSave;
        _btnCancel = btnCancel;

        lstGameObjects.Init(UpdateToolStripItems, AssignEditorItem, toolStripItemNew_Click, toolStripItemCopy_Click, toolStripItemUndo_Click, toolStripItemPaste_Click, toolStripItemDelete_Click);
    }
    private void AssignEditorItem(Guid id)
    {
        mEditorItem = ShopDescriptor.Get(id);
        UpdateEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.Shop)
        {
            InitEditor();
            if (mEditorItem != null && !ShopDescriptor.Lookup.Values.Contains(mEditorItem))
            {
                mEditorItem = null;
                UpdateEditor();
            }
        }
    }

    private void FrmShop_FormClosed(object sender, FormClosedEventArgs e)
    {
        btnCancel_Click(null, null);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        foreach (var item in mChanged)
        {
            item.RestoreBackup();
            item.DeleteBackup();
        }

        Hide();
        Globals.CurrentEditor = -1;
        Dispose();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        //Send Changed items
        foreach (var item in mChanged)
        {
            PacketSender.SendSaveObject(item);
            item.DeleteBackup();
        }

        Hide();
        Globals.CurrentEditor = -1;
        Dispose();
    }

    private void frmShop_Load(object sender, EventArgs e)
    {
        cmbAddBoughtItem.Items.Clear();
        cmbAddSoldItem.Items.Clear();
        cmbBuyFor.Items.Clear();
        cmbSellFor.Items.Clear();
        cmbDefaultCurrency.Items.Clear();
        cmbAddBoughtItem.Items.AddRange(ItemDescriptor.Names);
        cmbAddSoldItem.Items.AddRange(ItemDescriptor.Names);
        cmbBuyFor.Items.AddRange(ItemDescriptor.Names);
        cmbSellFor.Items.AddRange(ItemDescriptor.Names);
        cmbDefaultCurrency.Items.AddRange(ItemDescriptor.Names);
        if (cmbAddBoughtItem.Items.Count > 0)
        {
            cmbAddBoughtItem.SelectedIndex = 0;
        }

        if (cmbAddSoldItem.Items.Count > 0)
        {
            cmbAddSoldItem.SelectedIndex = 0;
        }

        if (cmbBuyFor.Items.Count > 0)
        {
            cmbBuyFor.SelectedIndex = 0;
        }

        if (cmbSellFor.Items.Count > 0)
        {
            cmbSellFor.SelectedIndex = 0;
        }

        cmbBuySound.Items.Clear();
        cmbBuySound.Items.Add(Strings.General.None);
        cmbBuySound.Items.AddRange(GameContentManager.SmartSortedSoundNames);

        cmbSellSound.Items.Clear();
        cmbSellSound.Items.Add(Strings.General.None);
        cmbSellSound.Items.AddRange(GameContentManager.SmartSortedSoundNames);

        InitLocalization();
        UpdateEditor();
    }

    private void InitLocalization()
    {
        Text = Strings.ShopEditor.title;
        toolStripItemNew.Text = Strings.ShopEditor.New;
        toolStripItemDelete.Text = Strings.ShopEditor.delete;
        toolStripItemCopy.Text = Strings.ShopEditor.copy;
        toolStripItemPaste.Text = Strings.ShopEditor.paste;
        toolStripItemUndo.Text = Strings.ShopEditor.undo;

        grpGeneral.Text = Strings.ShopEditor.general;
        lblName.Text = Strings.ShopEditor.name;
        lblDefaultCurrency.Text = Strings.ShopEditor.defaultcurrency;

        grpItemsSold.Text = Strings.ShopEditor.itemssold;
        lblAddSoldItem.Text = Strings.ShopEditor.addlabel;
        lblSellFor.Text = Strings.ShopEditor.sellfor;
        lblSellCost.Text = Strings.ShopEditor.sellcost;
        btnAddSoldItem.Text = Strings.ShopEditor.addsolditem;
        btnDelSoldItem.Text = Strings.ShopEditor.removesolditem;

        grpItemsBought.Text = Strings.ShopEditor.itemsboughtwhitelist;
        rdoBuyWhitelist.Text = Strings.ShopEditor.whitelist;
        rdoBuyBlacklist.Text = Strings.ShopEditor.blacklist;
        lblItemBought.Text = Strings.ShopEditor.addboughtitem;
        lblBuyFor.Text = Strings.ShopEditor.buyfor;
        lblBuyAmount.Text = Strings.ShopEditor.buycost;
        btnAddBoughtItem.Text = Strings.ShopEditor.addboughtitem;
        btnDelBoughtItem.Text = Strings.ShopEditor.removeboughtitem;

        lblBuySound.Text = Strings.ShopEditor.buysound;
        lblSellSound.Text = Strings.ShopEditor.sellsound;

        //Searching/Sorting
        btnAlphabetical.ToolTipText = Strings.ShopEditor.sortalphabetically;
        txtSearch.Text = Strings.ShopEditor.searchplaceholder;
        lblFolder.Text = Strings.ShopEditor.folderlabel;

        btnSave.Text = Strings.ShopEditor.save;
        btnCancel.Text = Strings.ShopEditor.cancel;
    }

    private void UpdateEditor()
    {
        if (mEditorItem != null)
        {
            pnlContainer.Show();

            txtName.Text = mEditorItem.Name;
            cmbFolder.Text = mEditorItem.Folder;
            cmbDefaultCurrency.SelectedIndex = ItemDescriptor.ListIndex(mEditorItem.DefaultCurrencyId);
            if (mEditorItem.BuyingWhitelist)
            {
                rdoBuyWhitelist.Checked = true;
            }
            else
            {
                rdoBuyBlacklist.Checked = true;
            }

            cmbBuySound.SelectedIndex = cmbBuySound.FindString(TextUtils.NullToNone(mEditorItem.BuySound));
            cmbSellSound.SelectedIndex = cmbSellSound.FindString(TextUtils.NullToNone(mEditorItem.SellSound));

            UpdateWhitelist();
            UpdateLists();
            if (mChanged.IndexOf(mEditorItem) == -1)
            {
                mChanged.Add(mEditorItem);
                mEditorItem.MakeBackup();
            }
        }
        else
        {
            pnlContainer.Hide();
        }

        var hasItem = mEditorItem != null;
        
        UpdateEditorButtons(hasItem);
        UpdateToolStripItems();
    }

    private void UpdateWhitelist()
    {
        if (rdoBuyWhitelist.Checked)
        {
            cmbBuyFor.Enabled = true;
            nudBuyAmount.Enabled = true;
            grpItemsBought.Text = Strings.ShopEditor.itemsboughtwhitelist;
        }
        else
        {
            cmbBuyFor.Enabled = false;
            nudBuyAmount.Enabled = false;
            grpItemsBought.Text = Strings.ShopEditor.itemsboughtblacklist;
        }
    }

    private void rdoBuyWhitelist_CheckedChanged(object sender, EventArgs e)
    {
        mEditorItem.BuyingWhitelist = rdoBuyWhitelist.Checked;
        UpdateLists();
        UpdateWhitelist();
    }

    private void rdoBuyBlacklist_CheckedChanged(object sender, EventArgs e)
    {
        mEditorItem.BuyingWhitelist = rdoBuyWhitelist.Checked;
        UpdateLists();
        UpdateWhitelist();
    }

    private void txtName_TextChanged(object sender, EventArgs e)
    {
        mEditorItem.Name = txtName.Text;
        lstGameObjects.UpdateText(txtName.Text);
    }

    private void UpdateLists()
    {
        lstSoldItems.Items.Clear();
        for (var i = 0; i < mEditorItem.SellingItems.Count; i++)
        {
            lstSoldItems.Items.Add(
                Strings.ShopEditor.selldesc.ToString(
                    ItemDescriptor.GetName(mEditorItem.SellingItems[i].ItemId),
                    mEditorItem.SellingItems[i].CostItemQuantity,
                    ItemDescriptor.GetName(mEditorItem.SellingItems[i].CostItemId)
                )
            );
        }

        lstBoughtItems.Items.Clear();
        if (mEditorItem.BuyingWhitelist)
        {
            for (var i = 0; i < mEditorItem.BuyingItems.Count; i++)
            {
                lstBoughtItems.Items.Add(
                    Strings.ShopEditor.buydesc.ToString(
                        ItemDescriptor.GetName(mEditorItem.BuyingItems[i].ItemId),
                        mEditorItem.BuyingItems[i].CostItemQuantity,
                        ItemDescriptor.GetName(mEditorItem.BuyingItems[i].CostItemId)
                    )
                );
            }
        }
        else
        {
            for (var i = 0; i < mEditorItem.BuyingItems.Count; i++)
            {
                lstBoughtItems.Items.Add(
                    Strings.ShopEditor.dontbuy.ToString(ItemDescriptor.GetName(mEditorItem.BuyingItems[i].ItemId))
                );
            }
        }
    }

    private void btnAddSoldItem_Click(object sender, EventArgs e)
    {
        var addedItem = false;
        var cost = (int) nudSellCost.Value;
        var newItem = new ShopItemDescriptor(
            ItemDescriptor.IdFromList(cmbAddSoldItem.SelectedIndex), ItemDescriptor.IdFromList(cmbSellFor.SelectedIndex), cost
        );

        for (var i = 0; i < mEditorItem.SellingItems.Count; i++)
        {
            if (mEditorItem.SellingItems[i].ItemId == newItem.ItemId)
            {
                mEditorItem.SellingItems[i] = newItem;
                addedItem = true;

                break;
            }
        }

        if (!addedItem)
        {
            mEditorItem.SellingItems.Add(newItem);
        }

        UpdateLists();
    }

    private void btnDelSoldItem_Click(object sender, EventArgs e)
    {
        if (lstSoldItems.SelectedIndex > -1)
        {
            mEditorItem.SellingItems.RemoveAt(lstSoldItems.SelectedIndex);
        }

        UpdateLists();
    }

    private void btnAddBoughtItem_Click(object sender, EventArgs e)
    {
        var addedItem = false;
        var cost = (int) nudBuyAmount.Value;
        var newItem = new ShopItemDescriptor(
            ItemDescriptor.IdFromList(cmbAddBoughtItem.SelectedIndex), ItemDescriptor.IdFromList(cmbBuyFor.SelectedIndex), cost
        );

        for (var i = 0; i < mEditorItem.BuyingItems.Count; i++)
        {
            if (mEditorItem.BuyingItems[i].ItemId == newItem.ItemId)
            {
                mEditorItem.BuyingItems[i] = newItem;
                addedItem = true;

                break;
            }
        }

        if (!addedItem)
        {
            mEditorItem.BuyingItems.Add(newItem);
        }

        UpdateLists();
    }

    private void btnDelBoughtItem_Click(object sender, EventArgs e)
    {
        if (lstBoughtItems.SelectedIndex > -1)
        {
            mEditorItem.BuyingItems.RemoveAt(lstBoughtItems.SelectedIndex);
        }

        UpdateLists();
    }

    private void cmbDefaultCurrency_SelectedIndexChanged(object sender, EventArgs e)
    {
        mEditorItem.DefaultCurrency = ItemDescriptor.FromList(cmbDefaultCurrency.SelectedIndex);
    }

    private void btnItemUp_Click(object sender, EventArgs e)
    {
        if (lstSoldItems.SelectedIndex > 0 && lstSoldItems.Items.Count > 1)
        {
            var index = lstSoldItems.SelectedIndex;
            var swapWith = mEditorItem.SellingItems[index - 1];
            mEditorItem.SellingItems[index - 1] = mEditorItem.SellingItems[index];
            mEditorItem.SellingItems[index] = swapWith;
            UpdateLists();
            lstSoldItems.SelectedIndex = index - 1;
        }
    }

    private void btnItemDown_Click(object sender, EventArgs e)
    {
        if (lstSoldItems.SelectedIndex > -1 && lstSoldItems.SelectedIndex + 1 != lstSoldItems.Items.Count)
        {
            var index = lstSoldItems.SelectedIndex;
            var swapWith = mEditorItem.SellingItems[index + 1];
            mEditorItem.SellingItems[index + 1] = mEditorItem.SellingItems[index];
            mEditorItem.SellingItems[index] = swapWith;
            UpdateLists();
            lstSoldItems.SelectedIndex = index + 1;
        }
    }

    private void cmbBuySound_SelectedIndexChanged(object sender, EventArgs e)
    {
        mEditorItem.BuySound = TextUtils.SanitizeNone(cmbBuySound?.Text);
    }

    private void cmbSellSound_SelectedIndexChanged(object sender, EventArgs e)
    {
        mEditorItem.SellSound = TextUtils.SanitizeNone(cmbSellSound?.Text);
    }

    private void toolStripItemNew_Click(object sender, EventArgs e)
    {
        PacketSender.SendCreateObject(GameObjectType.Shop);
    }

    private void toolStripItemDelete_Click(object sender, EventArgs e)
    {
        if (mEditorItem != null && lstGameObjects.Focused)
        {
            if (DarkMessageBox.ShowWarning(
                    Strings.ShopEditor.deleteprompt, Strings.ShopEditor.deletetitle, DarkDialogButton.YesNo,
                    Icon
                ) ==
                DialogResult.Yes)
            {
                PacketSender.SendDeleteObject(mEditorItem);
            }
        }
    }

    private void toolStripItemCopy_Click(object sender, EventArgs e)
    {
        if (mEditorItem != null && lstGameObjects.Focused)
        {
            mCopiedItem = mEditorItem.JsonData;
            toolStripItemPaste.Enabled = true;
        }
    }

    private void toolStripItemPaste_Click(object sender, EventArgs e)
    {
        if (mEditorItem != null && mCopiedItem != null && lstGameObjects.Focused)
        {
            mEditorItem.Load(mCopiedItem, true);
            UpdateEditor();
        }
    }

    private void toolStripItemUndo_Click(object sender, EventArgs e)
    {
        if (mChanged.Contains(mEditorItem) && mEditorItem != null)
        {
            if (DarkMessageBox.ShowWarning(
                    Strings.ShopEditor.undoprompt, Strings.ShopEditor.undotitle, DarkDialogButton.YesNo,
                    Icon
                ) ==
                DialogResult.Yes)
            {
                mEditorItem.RestoreBackup();
                UpdateEditor();
            }
        }
    }

    private void UpdateToolStripItems()
    {
        toolStripItemCopy.Enabled = mEditorItem != null && lstGameObjects.Focused;
        toolStripItemPaste.Enabled = mEditorItem != null && mCopiedItem != null && lstGameObjects.Focused;
        toolStripItemDelete.Enabled = mEditorItem != null && lstGameObjects.Focused;
        toolStripItemUndo.Enabled = mEditorItem != null && lstGameObjects.Focused;
    }

    private void form_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control)
        {
            if (e.KeyCode == Keys.N)
            {
                toolStripItemNew_Click(null, null);
            }
        }
    }

    #region "Item List - Folders, Searching, Sorting, Etc"

    public void InitEditor()
    {
        //Collect folders
        var mFolders = new List<string>();
        foreach (var itm in ShopDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((ShopDescriptor) itm.Value).Folder) &&
                !mFolders.Contains(((ShopDescriptor) itm.Value).Folder))
            {
                mFolders.Add(((ShopDescriptor) itm.Value).Folder);
                if (!mKnownFolders.Contains(((ShopDescriptor) itm.Value).Folder))
                {
                    mKnownFolders.Add(((ShopDescriptor) itm.Value).Folder);
                }
            }
        }

        mFolders.Sort();
        mKnownFolders.Sort();
        cmbFolder.Items.Clear();
        cmbFolder.Items.Add("");
        cmbFolder.Items.AddRange(mKnownFolders.ToArray());

        var items = ShopDescriptor.Lookup.OrderBy(p => p.Value?.Name).Select(pair => new KeyValuePair<Guid, KeyValuePair<string, string>>(pair.Key,
            new KeyValuePair<string, string>(((ShopDescriptor)pair.Value)?.Name ?? Models.DatabaseObject<ShopDescriptor>.Deleted, ((ShopDescriptor)pair.Value)?.Folder ?? ""))).ToArray();
        lstGameObjects.Repopulate(items, mFolders, btnAlphabetical.Checked, CustomSearch(), txtSearch.Text);
    }

    private void btnAddFolder_Click(object sender, EventArgs e)
    {
        var folderName = string.Empty;
        var result = DarkInputBox.ShowInformation(
            Strings.ShopEditor.folderprompt, Strings.ShopEditor.foldertitle, ref folderName,
            DarkDialogButton.OkCancel
        );

        if (result == DialogResult.OK && !string.IsNullOrEmpty(folderName))
        {
            if (!cmbFolder.Items.Contains(folderName))
            {
                mEditorItem.Folder = folderName;
                lstGameObjects.ExpandFolder(folderName);
                InitEditor();
                cmbFolder.Text = folderName;
            }
        }
    }

    private void cmbFolder_SelectedIndexChanged(object sender, EventArgs e)
    {
        mEditorItem.Folder = cmbFolder.Text;
        InitEditor();
    }

    private void btnAlphabetical_Click(object sender, EventArgs e)
    {
        btnAlphabetical.Checked = !btnAlphabetical.Checked;
        InitEditor();
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        InitEditor();
    }

    private void txtSearch_Leave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            txtSearch.Text = Strings.ShopEditor.searchplaceholder;
        }
    }

    private void txtSearch_Enter(object sender, EventArgs e)
    {
        txtSearch.SelectAll();
        txtSearch.Focus();
    }

    private void btnClearSearch_Click(object sender, EventArgs e)
    {
        txtSearch.Text = Strings.ShopEditor.searchplaceholder;
    }

    private bool CustomSearch()
    {
        return !string.IsNullOrWhiteSpace(txtSearch.Text) && txtSearch.Text != Strings.ShopEditor.searchplaceholder;
    }

    private void txtSearch_Click(object sender, EventArgs e)
    {
        if (txtSearch.Text == Strings.ShopEditor.searchplaceholder)
        {
            txtSearch.SelectAll();
        }
    }

    #endregion
}
