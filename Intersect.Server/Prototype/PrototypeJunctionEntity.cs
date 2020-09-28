using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intersect.Server.Prototype
{
    public class PrototypeJunctionEntity : IPrototypeEntity
    {
        /// <inheritdoc />
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public Guid Id { get; private set; }

        [ForeignKey(nameof(Set))]
        public Guid SetId { get; private set; }

        public virtual PrototypeSetEntity Set { get; private set; }
        
        [ForeignKey(nameof(Simple))]
        public Guid SimpleId { get; private set; }

        public virtual PrototypeSimpleEntity Simple { get; private set; }

        public string JunctionMetaProperty { get; set; }
    }
}
