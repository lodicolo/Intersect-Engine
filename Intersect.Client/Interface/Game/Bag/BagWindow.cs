﻿using System;
using System.Collections.Generic;

using Intersect.Client.Core;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.General;
using Intersect.Client.Localization;
using Intersect.GameObjects;

namespace Intersect.Client.Interface.Game.Bag
{

    public partial class BagWindow
    {

        private static int sItemXPadding = 4;

        private static int sItemYPadding = 4;

        public List<BagItem> Items = new List<BagItem>();

        //Controls
        private WindowControl mBagWindow;

        private ScrollControl mItemContainer;

        private List<Label> mValues = new List<Label>();

        // Context menu
        private Framework.Gwen.Control.Menu mContextMenu;

        private MenuItem mWithdrawContextItem;

        //Init
        public BagWindow(Canvas gameCanvas)
        {
            mBagWindow = new WindowControl(GameContentManager.UI.InGame, gameCanvas, Strings.Bags.title, false, "BagWindow");
            mBagWindow.DisableResizing();
            Interface.InputBlockingElements.Add(mBagWindow);

            mItemContainer = new ScrollControl(mBagWindow, "ItemContainer");
            mItemContainer.EnableScroll(false, true);

            mBagWindow.LoadJsonUi(GameContentManager.UI.InGame, Graphics.Renderer.GetResolutionString());

            InitItemContainer();

            // Generate our context menu with basic options.
            mContextMenu = new Framework.Gwen.Control.Menu(gameCanvas, "BagContextMenu");
            mContextMenu.IsHidden = true;
            mContextMenu.IconMarginDisabled = true;
            //TODO: Is this a memory leak?
            mContextMenu.Children.Clear();
            mWithdrawContextItem = mContextMenu.AddItem(Strings.BagContextMenu.Withdraw);
            mWithdrawContextItem.Clicked += MWithdrawContextItem_Clicked;
            mContextMenu.LoadJsonUi(GameContentManager.UI.InGame, Graphics.Renderer.GetResolutionString());
        }

        public void OpenContextMenu(int slot)
        {
            var item = ItemBase.Get(Globals.Bag[slot].ItemId);

            // No point showing a menu for blank space.
            if (item == null)
            {
                return;
            }

            mWithdrawContextItem.SetText(Strings.BagContextMenu.Withdraw.ToString(item.Name));

            // Set our spell slot as userdata for future reference.
            mContextMenu.UserData = slot;

            mContextMenu.SizeToChildren();
            mContextMenu.Open(Framework.Gwen.Pos.None);
        }

        private void MWithdrawContextItem_Clicked(Base sender, Framework.Gwen.Control.EventArguments.ClickedEventArgs arguments)
        {
            if (Globals.InBag)
            {
                var slot = (int) sender.Parent.UserData;
                Globals.Me.TryRetreiveBagItem(slot, -1);
            }
        }

        //Location
        //Location
        public int X => mBagWindow.X;

        public int Y => mBagWindow.Y;

        public void Close()
        {
            mContextMenu?.Close();
            mBagWindow.Close();
        }

        public bool IsVisible()
        {
            return !mBagWindow.IsHidden;
        }

        public void Hide()
        {
            mBagWindow.IsHidden = true;
        }

        public void Update()
        {
            if (mBagWindow.IsHidden == true || Globals.Bag == null)
            {
                return;
            }

            for (var i = 0; i < Globals.Bag.Length; i++)
            {
                if (Globals.Bag[i] != null && Globals.Bag[i].ItemId != Guid.Empty)
                {
                    var item = ItemBase.Get(Globals.Bag[i].ItemId);
                    if (item != null)
                    {
                        Items[i].Pnl.IsHidden = false;

                        if (item.IsStackable)
                        {
                            mValues[i].IsHidden = Globals.Bag[i].Quantity <= 1;
                            mValues[i].Text = Strings.FormatQuantityAbbreviated(Globals.Bag[i].Quantity);
                        }
                        else
                        {
                            mValues[i].IsHidden = true;
                        }

                        if (Items[i].IsDragging)
                        {
                            Items[i].Pnl.IsHidden = true;
                            mValues[i].IsHidden = true;
                        }

                        Items[i].Update();
                    }
                }
                else
                {
                    Items[i].Pnl.IsHidden = true;
                    mValues[i].IsHidden = true;
                }
            }
        }

        private void InitItemContainer()
        {
            for (var i = 0; i < Globals.Bag.Length; i++)
            {
                Items.Add(new BagItem(this, i));
                Items[i].Container = new ImagePanel(mItemContainer, "BagItem");
                Items[i].Setup();

                mValues.Add(new Label(Items[i].Container, "BagItemValue"));
                mValues[i].Text = "";
                Items[i].Container.LoadJsonUi(GameContentManager.UI.InGame, Graphics.Renderer.GetResolutionString());

                var xPadding = Items[i].Container.Margin.Left + Items[i].Container.Margin.Right;
                var yPadding = Items[i].Container.Margin.Top + Items[i].Container.Margin.Bottom;
                Items[i]
                    .Container.SetPosition(
                        i %
                        (mItemContainer.Width / (Items[i].Container.Width + xPadding)) *
                        (Items[i].Container.Width + xPadding) +
                        xPadding,
                        i /
                        (mItemContainer.Width / (Items[i].Container.Width + xPadding)) *
                        (Items[i].Container.Height + yPadding) +
                        yPadding
                    );
            }
        }

        public FloatRect RenderBounds()
        {
            var rect = new FloatRect()
            {
                X = mBagWindow.LocalPosToCanvas(new Point(0, 0)).X - sItemXPadding / 2,
                Y = mBagWindow.LocalPosToCanvas(new Point(0, 0)).Y - sItemYPadding / 2,
                Width = mBagWindow.Width + sItemXPadding,
                Height = mBagWindow.Height + sItemYPadding
            };

            return rect;
        }

    }

}
