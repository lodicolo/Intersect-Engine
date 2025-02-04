using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using Newtonsoft.Json;

namespace Intersect.Framework.Resources;

public static class ResourceManagerExtensions
{
    public static string? GetStringWithFallback(
        this ResourceManager resourceManager,
        string name,
        CultureInfo? cultureInfo = null,
        bool fallbackToResourceName = false
    )
    {
        cultureInfo ??= CultureInfo.CurrentUICulture;

        while (cultureInfo.LCID != CultureInfo.InvariantCulture.LCID)
        {
            var value = resourceManager.GetString(name, cultureInfo);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            cultureInfo = cultureInfo.Parent;
        }

        return fallbackToResourceName ? name : null;
    }

    public static bool TryDumpResourcesToJson(this ResourceManager resourceManager, string baseName, CultureInfo? cultureInfo = null)
    {
        try
        {
            cultureInfo ??= CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
            var resourceSet = resourceManager.GetResourceSet(
                cultureInfo,
                createIfNotExists: true,
                tryParents: true
            );

            if (resourceSet == null)
            {
                return false;
            }

            Dictionary<string, object> resources = new();
            var enumerator = resourceSet.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Value is not {} value)
                {
                    continue;
                }

                if (enumerator.Key is not string key)
                {
                    continue;
                }

                resources[key] = value;
            }

            var source = JsonConvert.SerializeObject(resources, Formatting.Indented);

            var targetName = baseName;
            if (cultureInfo.LCID != CultureInfo.InvariantCulture.LCID)
            {
                targetName = $"{targetName}.{cultureInfo.IetfLanguageTag}";
            }

            targetName = $"{targetName}.json";

            File.WriteAllText(targetName, source, Encoding.UTF8);

            return true;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return false;
        }
    }

    public static bool TryInjectResourceSetFromJson(
        this ResourceManager resourceManager,
        CultureInfo cultureInfo,
        string baseName
    )
    {
        try
        {
            var localeName = $"{baseName}.{cultureInfo.IetfLanguageTag}.json";
            var memberInfoResourceSets = typeof(ResourceManager).GetMember(
                    "_resourceSets",
                    BindingFlags.Instance | BindingFlags.NonPublic
                )
                .FirstOrDefault();
            var resourceSetsObject = (memberInfoResourceSets as FieldInfo)?.GetValue(resourceManager);
            if (resourceSetsObject is not Dictionary<string, ResourceSet> resourceSets)
            {
                return false;
            }

            if (resourceSets.TryGetValue(cultureInfo.Name, out var existingResourceSet))
            {
                return false;
            }

            IResourceReader resourceReader = new JsonResourceReader(new FileInfo(localeName));
            ResourceSet resourceSet = new(resourceReader);
            resourceSets[cultureInfo.Name] = resourceSet;
            return true;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return false;
        }
    }
}