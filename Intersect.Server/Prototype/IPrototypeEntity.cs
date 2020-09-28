using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intersect.Server.Prototype
{
    public interface IPrototypeEntity
    {
        [Key, Column(Order = 0)]
        Guid Id { get; }
    }
}
