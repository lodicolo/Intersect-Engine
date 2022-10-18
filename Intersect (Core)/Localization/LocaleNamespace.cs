using System.Reflection;
using Intersect.Framework.Reflection;

namespace Intersect.Localization;

/// <summary>
/// Namespace of localizations.
/// </summary>
[Serializable]
public abstract class LocaleNamespace : Localized
{
    /// <summary>
    /// Constructs an instance of this <see cref="LocaleNamespace"/>
    /// </summary>
    protected LocaleNamespace() => ValidateNamespaceType(GetType());

    private static void ValidateNamespaceType(Type type)
    {
        if (!type.Extends<LocaleNamespace>())
        {
            throw new ArgumentException(
                string.Format(
                    LocalizationResources.LocaleNamespace_ValidateNamespaceType_NotSubclass,
                    type.FullName,
                    typeof(LocaleNamespace).FullName
                ),
                nameof(type)
            );
        }

        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        foreach (var fieldInfo in fieldInfos)
        {
            if (!fieldInfo.FieldType.Extends<Localized>())
            {
                throw new ArgumentException(
                    string.Format(
                        LocalizationResources.LocaleNamespace_ValidateNamespaceType_DisallowedFieldType,
                        type.FullName,
                        fieldInfo.Name,
                        fieldInfo.FieldType.FullName
                    ),
                    nameof(type)
                );
            }
        }
    }
}
