using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersect.Models
{
    public interface ILocalizedObject : IObject
    {
        [NotMapped] string Name { get; }

        IDictionary<string, string> LocalizedNames { get; }
    }
}
