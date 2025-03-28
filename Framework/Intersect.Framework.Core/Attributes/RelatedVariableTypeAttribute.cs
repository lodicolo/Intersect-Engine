using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Events;

namespace Intersect.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class RelatedVariableTypeAttribute : Attribute
{
    public VariableType VariableType { get; }

    public RelatedVariableTypeAttribute(VariableType variableType) { VariableType = variableType; }
}
