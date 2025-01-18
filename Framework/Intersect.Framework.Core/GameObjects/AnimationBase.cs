using System.ComponentModel.DataAnnotations.Schema;
using Intersect.Framework.Core.Content;
using Intersect.GameObjects.Annotations;
using Intersect.Models;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace Intersect.GameObjects;

[Owned]
public partial class AnimationLayer
{
    public AnimationLayer()
    {
        Lights = new LightBase[FrameCount];

        for (var frame = 0; frame < FrameCount; ++frame)
        {
            Lights[frame] = new LightBase();
        }
    }

    [EditorTexture(Type = TextureType.Animation)]
    public string Sprite { get; set; } = string.Empty;

    [EditorRange<int>(Min = 1)]
    [EditorGroup("FrameOptions")]
    public int FrameCount { get; set; } = 1;

    [EditorRange<int>(Min = 1)]
    [EditorGroup("FrameOptions")]
    public int XFrames { get; set; } = 1;

    [EditorRange<int>(Min = 1)]
    [EditorGroup("FrameOptions")]
    public int YFrames { get; set; } = 1;

    [EditorRange<int>(Min = 1)]
    [EditorUnit(UnitHint.TimeMilliseconds)]
    [EditorGroup("FrameOptions")]
    public int FrameSpeed { get; set; } = 100;

    [EditorRange<int>(Min = 0)]
    [EditorGroup("FrameOptions")]
    public int LoopCount { get; set; }

    [EditorGroup("ExtraOptions")]
    public bool DisableRotations { get; set; }

    [EditorGroup("ExtraOptions")]
    public bool AlternateRenderLayer { get; set; }

    [JsonIgnore]
    public string Light
    {
        get => JsonConvert.SerializeObject(Lights);
        set => Lights = JsonConvert.DeserializeObject<LightBase[]>(value);
    }

    [NotMapped]
    public LightBase[] Lights { get; set; }
}

public partial class AnimationBase : DatabaseObject<AnimationBase>, IFolderable
{
    [JsonConstructor]
    public AnimationBase(Guid id) : base(id)
    {
        // TODO: localize this
        Name = "New Animation";
        Lower = new AnimationLayer();
        Upper = new AnimationLayer();
    }

    //EF Parameterless Constructor
    public AnimationBase()
    {
        // TODO: localize this
        Name = "New Animation";
        Lower = new AnimationLayer();
        Upper = new AnimationLayer();
    }

    [EditorGroup]
    public AnimationLayer Lower { get; set; }

    [EditorGroup]
    public AnimationLayer Upper { get; set; }

    //Misc
    [EditorGroup("Audio")]
    public string Sound { get; set; }

    [EditorGroup("Audio")]
    public bool CompleteSound { get; set; }

    /// <inheritdoc />
    [EditorGroup("General")]
    public string Folder { get; set; } = string.Empty;
}
