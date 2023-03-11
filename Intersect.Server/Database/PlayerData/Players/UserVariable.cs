using Intersect.GameObjects;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intersect.Server.Database.PlayerData.Players
{
    public partial class UserVariable : Variable
    {
        public UserVariable() : base() { }

        public UserVariable(Guid id) : base(id) { }

        [NotMapped]
        public string VariableName => UserVariableBase.GetName(VariableId);

        [JsonIgnore]
        public Guid UserId { get; protected set; }

        [JsonIgnore]
        public virtual User User { get; protected set; }
    }
}
