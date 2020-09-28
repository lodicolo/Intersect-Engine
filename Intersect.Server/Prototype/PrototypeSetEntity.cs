using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intersect.Server.Prototype
{
    public class PrototypeSetEntity : IPrototypeEntity
    {
        /// <inheritdoc />
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public Guid Id { get; private set; }

        public string Name { get; set; }

        public virtual List<PrototypeJunctionEntity> Junctions { get; private set; } =
            new List<PrototypeJunctionEntity>();

        public void Add(PrototypeSimpleEntity simple)
        {
            if (Junctions.Exists(j => j.SimpleId == simple.Id))
            {
                return;
            }

            var junction = new PrototypeJunctionEntity
            {
                JunctionMetaProperty = new Random().Next().ToString()
            };

            simple.Junctions.Add(junction);
            Junctions.Add(junction);
        }
    }
}
