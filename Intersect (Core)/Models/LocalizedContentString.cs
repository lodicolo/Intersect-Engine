using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersect.Models
{
    public partial class LocalizedContentString
    {
        public Guid Id { get; }

        public string Locale { get; }

        public string Value { get; set; }

        public LocalizedContentString(Guid id, string locale, string value)
        {
            Id = id;
            Locale = locale;
            Value = value;
        }
    }
}
