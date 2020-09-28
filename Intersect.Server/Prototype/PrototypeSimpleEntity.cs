using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intersect.Server.Prototype
{
    public class PrototypeSimpleEntity : IPrototypeEntity
    {
        /// <inheritdoc />
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public Guid Id { get; private set; }

        public string Name { get; set; }

        [ForeignKey(nameof(Description))]
        public Guid? DescriptionId { get; private set; }

        public ContentString Description { get; private set; }

        public virtual List<PrototypeJunctionEntity> Junctions { get; private set; } =
            new List<PrototypeJunctionEntity>();
    }
}
