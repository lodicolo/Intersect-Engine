using Intersect.Client.Framework.Content;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.ControlInternal;
using Intersect.Client.Framework.Gwen.Skin.Texturing;
using Intersect.Client.Framework.Input;
using Newtonsoft.Json.Linq;

namespace Intersect.Client.Framework.Gwen.Control;


/// <summary>
///     Image container.
/// </summary>
public partial class ImagePanel : Base
{

    private readonly float[] _uv;

    //Sound Effects
    protected string mHoverSound;

    protected string mLeftMouseClickSound;

    protected string mRightMouseClickSound;

    private GameTexture? _texture { get; set; }
    private string? _textureName;
    private Rectangle _textureSourceBounds;
    private float _textureAspectRatio;

    private Margin? _textureNinePatchMargin;
    private Bordered? _ninepatchRenderer;
    private bool _maintainAspectRatio;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImagePanel" /> class.
    /// </summary>
    /// <param name="parent">Parent control.</param>
    /// <param name="name"></param>
    public ImagePanel(Base parent, string? name = default) : base(parent, name: name)
    {
        _uv = [0, 0, 1, 1];

        MouseInputEnabled = true;
    }

    public bool MaintainAspectRatio
    {
        get => _maintainAspectRatio;
        set => SetAndDoIfChanged(ref _maintainAspectRatio, value, Invalidate);
    }

    public Margin? TextureNinePatchMargin
    {
        get => _textureNinePatchMargin;
        set
        {
            if (value == _textureNinePatchMargin)
            {
                return;
            }

            _textureNinePatchMargin = value;
            _ninepatchRenderer = null;
        }
    }

    /// <summary>
    ///     Assign Existing Texture
    /// </summary>
    public GameTexture? Texture
    {
        get => _texture;
        set
        {
            if (value == _texture)
            {
                return;
            }

            _texture = value;
            RecomputeTextureSourceBounds();
            if (_texture != null)
            {
                TextureLoaded?.Invoke(this, EventArgs.Empty);
            }

            _ninepatchRenderer = null;
            this.InvalidateParent();
        }
    }

    private void RecomputeTextureSourceBounds()
    {
        if (_texture is not { } texture)
        {
            _textureAspectRatio = 0;
            _textureSourceBounds = default;
            return;
        }

        var uvs = _uv.ToArray();
        var u1 = uvs[0];
        var v1 = uvs[1];
        var u2 = uvs[2];
        var v2 = uvs[3];

        (u1, u2) = (Math.Min(u1, u2), Math.Max(u1, u2));
        (v1, v2) = (Math.Min(v1, v2), Math.Max(v1, v2));

        var textureSourceBounds = new Rectangle(
            (int)(u1 * texture.Width),
            (int)(v1 * texture.Height),
            (int)((u2 - u1) * texture.Width),
            (int)((v2 - v1) * texture.Height)
        );
        _textureSourceBounds = textureSourceBounds;

        _textureAspectRatio = textureSourceBounds.Width / (float)textureSourceBounds.Height;
    }

    public event GwenEventHandler<EventArgs>? TextureLoaded;

    public string? TextureFilename
    {
        get => _textureName;
        set
        {
            var textureFilename = value?.Trim();
            if (string.Equals(textureFilename, _textureName, StringComparison.Ordinal))
            {
                return;
            }

            _textureName = textureFilename;
            Texture = _textureName == null
                ? null
                : GameContentManager.Current?.GetTexture(TextureType.Gui, _textureName);
        }
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();
        PlaySound(mHoverSound);
    }

    protected override void OnMouseClicked(MouseButton mouseButton, Point mousePosition, bool userAction = true)
    {
        base.OnMouseClicked(mouseButton, mousePosition, userAction);
        PlaySound(
            mouseButton switch
            {
                MouseButton.Left => mLeftMouseClickSound,
                MouseButton.Right => mRightMouseClickSound,
                _ => null,
            }
        );
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
    }

    public override JObject? GetJson(bool isRoot = false, bool onlySerializeIfNotEmpty = false)
    {
        var serializedProperties = base.GetJson(isRoot, onlySerializeIfNotEmpty);
        if (serializedProperties is null)
        {
            return null;
        }

        serializedProperties.Add(nameof(Texture), TextureFilename);
        serializedProperties.Add(nameof(TextureNinePatchMargin), TextureNinePatchMargin?.ToString());
        serializedProperties.Add("HoverSound", mHoverSound);
        serializedProperties.Add("LeftMouseClickSound", mLeftMouseClickSound);
        serializedProperties.Add("RightMouseClickSound", mRightMouseClickSound);

        return base.FixJson(serializedProperties);
    }

    public override void LoadJson(JToken token, bool isRoot = default)
    {
        base.LoadJson(token);

        if (token is not JObject obj)
        {
            return;
        }

        if (obj["Texture"] != null)
        {
            Texture = GameContentManager.Current.GetTexture(
                TextureType.Gui, (string) obj["Texture"]
            );

            TextureFilename = (string) obj["Texture"];
        }

        if (obj.TryGetValue(nameof(TextureNinePatchMargin), out var textureNinePatchMarginToken))
        {
            if (textureNinePatchMarginToken is JValue { Type: JTokenType.String } textureNinePatchMarginValue)
            {
                if (textureNinePatchMarginValue.Value<string>() is { } textureNinePatchMarginString &&
                    !string.IsNullOrWhiteSpace(textureNinePatchMarginString))
                {
                    TextureNinePatchMargin = Margin.FromString(textureNinePatchMarginString);
                }
                else
                {
                    TextureNinePatchMargin = null;
                }
            }
            else
            {
                TextureNinePatchMargin = null;
            }
        }

        if (obj["HoverSound"] != null)
        {
            mHoverSound = (string) obj["HoverSound"];
        }

        if (obj["LeftMouseClickSound"] != null)
        {
            mLeftMouseClickSound = (string) obj["LeftMouseClickSound"];
        }

        if (obj["RightMouseClickSound"] != null)
        {
            mRightMouseClickSound = (string) obj["RightMouseClickSound"];
        }
    }

    /// <summary>
    ///     Sets the texture coordinates of the image.
    /// </summary>
    public virtual void SetUv(float u1, float v1, float u2, float v2)
    {
        _uv[0] = u1;
        _uv[1] = v1;
        _uv[2] = u2;
        _uv[3] = v2;

        RecomputeTextureSourceBounds();

        _ninepatchRenderer = null;
    }

    /// <summary>
    ///     Sets the texture coordinates of the image.
    /// </summary>
    public virtual void SetTextureRect(int x, int y, int w, int h)
    {
        if (_texture == null)
        {
            return;
        }

        if (x < 0)
        {
            x = 0;
        }

        if (y < 0)
        {
            y = 0;
        }

        if (w <= 0)
        {
            w = _texture.Width;
        }

        if (h <= 0)
        {
            h = _texture.Height;
        }

        if (x + w > _texture.Width || y + h > _texture.Height)
        {
            return;
        }

        SetUv(
            x / (float)_texture.Width,
            y / (float)_texture.Height,
            (x + w) / (float)_texture.Width,
            (y + h) / (float)_texture.Height
        );
    }

    protected override void Layout(Skin.Base skin)
    {
        base.Layout(skin);

        EnsureTextureLoaded();
    }

    protected override void Prelayout(Skin.Base skin)
    {
        base.Prelayout(skin);

        EnsureTextureLoaded();
    }

    private void EnsureTextureLoaded()
    {
        if (_texture != null)
        {
            return;
        }

        var textureFilename = _textureName;
        if (string.IsNullOrWhiteSpace(textureFilename))
        {
            return;
        }

        Texture = GameContentManager.Current?.GetTexture(TextureType.Gui, textureFilename);
    }

    /// <summary>
    ///     Renders the control using specified skin.
    /// </summary>
    /// <param name="skin">Skin to use.</param>
    protected override void Render(Skin.Base skin)
    {
        base.Render(skin);

        if (_texture is not { } texture)
        {
            return;
        }

        skin.Renderer.DrawColor = base.RenderColor;

        var renderBounds = RenderBounds;

        if (TextureNinePatchMargin is { } textureNinePatchMargin)
        {
            var sourceBounds = _textureSourceBounds;
            _ninepatchRenderer ??= new Bordered(
                texture,
                sourceBounds.X,
                sourceBounds.Y,
                sourceBounds.Width,
                sourceBounds.Height,
                textureNinePatchMargin
            );

            if (_ninepatchRenderer is { } ninepatchRenderer)
            {
                ninepatchRenderer.Draw(skin.Renderer, renderBounds, Color.White);
            }
        }
        else if (_textureSourceBounds is { Width: > 0, Height: > 0 } textureSourceBounds)
        {
            if (_maintainAspectRatio)
            {
                var widthRatio = renderBounds.Width / (float)textureSourceBounds.Width;
                var heightRatio = renderBounds.Height / (float)textureSourceBounds.Height;

                if (widthRatio > heightRatio)
                {
                    renderBounds.Width = (int)(textureSourceBounds.Width * heightRatio);
                }
                else if (heightRatio > widthRatio)
                {
                    renderBounds.Height = (int)(textureSourceBounds.Height * widthRatio);
                }
            }

            skin.Renderer.DrawTexturedRect(
                texture,
                renderBounds,
                base.RenderColor,
                _uv[0],
                _uv[1],
                _uv[2],
                _uv[3]
            );
        }
    }

    /// <summary>
    ///     Sizes the control to its contents.
    /// </summary>
    public virtual void SizeToContents() => SizeToChildren();

    public override bool SizeToChildren(bool resizeX = true, bool resizeY = true, bool recursive = false)
    {
        return base.SizeToChildren(resizeX, resizeY, recursive);
    }

    public override bool SetBounds(int x, int y, int width, int height)
    {
        if (MaintainAspectRatio)
        {
            var bounds = Bounds;
            var aspectRatio = _textureAspectRatio;
            if (width == bounds.Width)
            {
                if (height != bounds.Height)
                {
                    var aspectRatioWidth = (int)(height * aspectRatio);
                    if (aspectRatioWidth > width)
                    {
                        width = aspectRatioWidth;
                    }
                }
            }
            else if (height == bounds.Height)
            {
                var aspectRatioHeight = (int)(width / aspectRatio);
                if (aspectRatioHeight > height)
                {
                    height = aspectRatioHeight;
                }
            }
        }

        return base.SetBounds(x, y, width, height);
    }

    public override Point GetChildrenSize()
    {
        var textureSize = _textureSourceBounds.Size;
        if (TextureNinePatchMargin != null)
        {
            textureSize = default;
        }

        var elementChildrenSize = base.GetChildrenSize();
        var childrenSize = new Point(
            Math.Max(elementChildrenSize.X, textureSize.X),
            Math.Max(elementChildrenSize.Y, textureSize.Y)
        );
        return childrenSize;
    }

    /// <summary>
    ///     Control has been clicked - invoked by input system. Windows use it to propagate activation.
    /// </summary>
    public override void Touch()
    {
        base.Touch();
    }

    /// <summary>
    ///     Handler for Space keyboard event.
    /// </summary>
    /// <param name="down">Indicates whether the key was pressed or released.</param>
    /// <returns>
    ///     True if handled.
    /// </returns>
    protected override bool OnKeySpace(bool down)
    {
        if (down)
        {
            base.OnMouseDown(MouseButton.Left, default);
        }

        return true;
    }

}
