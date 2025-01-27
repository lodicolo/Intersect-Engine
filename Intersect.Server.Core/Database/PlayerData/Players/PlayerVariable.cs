using System.ComponentModel.DataAnnotations.Schema;
using Intersect.Framework.Core.GameObjects.Variables;
using Intersect.Server.Entities;
using Newtonsoft.Json;

namespace Intersect.Server.Database.PlayerData.Players;


public partial class PlayerVariable : Variable, IPlayerOwned
{

    public PlayerVariable() : this(Guid.Empty) { }

    public PlayerVariable(Guid id)
    {
        VariableId = id;
    }

    [NotMapped]
    public string VariableName => PlayerVariableDescriptor.GetName(VariableId);

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [JsonIgnore]
    public Guid PlayerId { get; protected set; }

    [JsonIgnore]
    [ForeignKey(nameof(PlayerId))]
    public virtual Player Player { get; protected set; }

}
