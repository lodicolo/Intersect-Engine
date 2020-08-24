using System;

namespace Intersect.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class NetworkVisibilityAttribute : Attribute
    {
        public bool Hidden { get; set; }
    }
}
