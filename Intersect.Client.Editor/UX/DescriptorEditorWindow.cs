using System.Diagnostics;
using System.Globalization;
using Intersect.Client.Editor.Properties;
using Intersect.Client.Framework.Gwen;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Control.Layout;
using Intersect.Client.General;
using Intersect.Client.Interface;
using Intersect.Enums;
using Intersect.Extensions;
using Intersect.Framework.Reflection;
using Intersect.Models;

namespace Intersect.Client.Editor.UX;

public class DescriptorEditorWindow<TDescriptorType> : Window
    where TDescriptorType : DatabaseObject<TDescriptorType>
{
    public DescriptorEditorWindow(Base? parent) :
        base(
            parent,
            title: EditorStrings.DescriptorEditorWindowTitle.Format(
                typeof(TDescriptorType).GetLocalizedName(culture: CultureInfo.CurrentUICulture)
            ),
            modal: false,
            name: $"{typeof(DescriptorEditorWindow<TDescriptorType>).GetName(qualified: true)}"
        )
    {
        MinimumSize = new Point(200, 200);
        InnerPanelPadding = Padding.Four;
        IsVisible = true;

        TitleLabel.Alignment = Pos.Center;
        TitleLabel.FontSize += 2;
        TitleLabel.AutoSizeToContents = true;
        TitleLabel.MouseInputEnabled = true;

        Font = Globals.ContentManager?.GetFont("sourcesanspro", 10);

        var buttonDebug = new Button(this, "buttonDebug")
        {
            Dock = Pos.Auto,
            // RenderColor = Color.White,
            // TextColor = Color.White,
            // TextColorOverride = Color.White,
            Text = "Debug",
        };

        buttonDebug.Clicked += (sender, arguments) =>
        {
            MutableInterface._debugWindow?.SizeToChildren();
        };

        var buttonDebug2 = new Button(this, "buttonDebug2")
        {
            Dock = Pos.Auto,
            Height = 30,
            Width = 50,
            // RenderColor = Color.White,
            // TextColor = Color.White,
            // TextColorOverride = Color.White,
            Text = "Debug2",
        };

        buttonDebug2.Clicked += (sender, arguments) =>
        {
            MutableInterface._debugWindow?.FindChildByName("InnerPanel")?.SizeToChildren();
        };

        var buttonDebug3 = new Button(this, "buttonDebug3")
        {
            Dock = Pos.Auto,
            Width = 43,
            // RenderColor = Color.White,
            // TextColor = Color.White,
            // TextColorOverride = Color.White,
            Text = "Debug3",
        };

        buttonDebug3.Clicked += (sender, arguments) =>
        {
            var table = MutableInterface._debugWindow?.FindChildByName<Table>("TableDebugStats", recursive: true);
            table?.SizeToContents(350);
        };

        var buttonDebug4 = new Button(this, "buttonDebug4")
        {
            Dock = Pos.Auto,
            Width = 148,
            // RenderColor = Color.White,
            // TextColor = Color.White,
            // TextColorOverride = Color.White,
            Text = "Debug4",
        };

        buttonDebug4.Clicked += (sender, arguments) =>
        {
        };
    }

    protected override void EnsureInitialized()
    {
    }
}