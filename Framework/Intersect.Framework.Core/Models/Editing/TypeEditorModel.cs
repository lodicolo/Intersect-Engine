using System.Reflection;
using Intersect.GameObjects.Annotations;

namespace Intersect.Framework.Core.Models;

public record EditorModelTreeNode
{
    public MemberEditorModel Model { get; init; }

    public EditorModelTreeNode? Parent { get; init; }
}

public abstract record EditorModel;

public abstract record MemberEditorModel
{
    public MemberInfo Info { get; init; }

    public EditorLabelAttribute? LabelOverride { get; init; }
}

public record PropertyEditorModel : MemberEditorModel
{
    public new PropertyInfo Info
    {
        get => base.Info as PropertyInfo;
        init => base.Info = value;
    }

    public TypeEditorModel TypeModel { get; init; }

    public static PropertyEditorModel CreateFrom(PropertyInfo propertyInfo)
    {
        var editorAttributes = propertyInfo.GetCustomAttributes<EditorAttribute>().ToArray();
        var labelOverride = editorAttributes.OfType<EditorLabelAttribute>().FirstOrDefault();

        return new PropertyEditorModel
        {
            Info = propertyInfo,
            LabelOverride = labelOverride,
            TypeModel = TypeEditorModel.CreateFrom(propertyInfo.PropertyType),
        };
    }
}

public record TypeEditorModel : MemberEditorModel
{
    public Type Type
    {
        get => base.Info as Type;
        init => base.Info = value;
    }

    public PropertyEditorModel[] Members { get; init; }

    private static readonly Dictionary<Type, TypeEditorModel> CachedModels = [];

    public static TypeEditorModel CreateFrom(Type type)
    {
        if (CachedModels.TryGetValue(type, out var typeModel))
        {
            return typeModel;
        }

        var editorAttributes = type.GetCustomAttributes<EditorAttribute>().ToArray();
        var labelOverride = editorAttributes.OfType<EditorLabelAttribute>().FirstOrDefault();

        var publicPropertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(pi => pi.GetCustomAttribute<EditorIgnoreAttribute>() == null).ToArray();
        var nonPublicPropertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Where(
            pi => pi.GetCustomAttribute<EditorIgnoreAttribute>() == null &&
                  pi.GetCustomAttribute<EditorAttribute>() != null
        ).ToArray();

        PropertyInfo[] allValidPropertyInfos = [..publicPropertyInfos, ..nonPublicPropertyInfos];
        var memberModels = allValidPropertyInfos.Select(PropertyEditorModel.CreateFrom).ToArray();

        typeModel = new TypeEditorModel
        {
            LabelOverride = labelOverride, Members = memberModels, Type = type,
        };

        CachedModels[type] = typeModel;
        return typeModel;
    }
}
