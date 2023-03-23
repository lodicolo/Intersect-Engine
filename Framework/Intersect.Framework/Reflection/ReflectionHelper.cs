﻿using System.Reflection;

namespace Intersect.Framework.Reflection;

public static class ReflectionHelper
{
    private static Assembly[]? _cachedAssemblies;
    private static Type[]? _cachedTypes;

    public static Assembly[] GetLoadedAssemblies(bool ignoreCache = false, bool includeDynamic = false)
    {
        if (!ignoreCache && _cachedAssemblies != default)
        {
            return _cachedAssemblies;
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        int indexPlace = 0;
        for (var indexSeek = 0; indexSeek < assemblies.Length; ++indexSeek)
        {
            var assembly = assemblies[indexSeek];

            try
            {
                // This is just to make sure it's actually valid/useful
                // On some assemblies this will throw an exception
                _ = assembly.ExportedTypes;
            }
            catch
            {
                continue;
            }

            if (indexSeek != indexPlace)
            {
                assemblies[indexPlace] = assembly;
            }

            if (includeDynamic || !assembly.IsDynamic)
            {
                ++indexPlace;
            }
        }

        _cachedAssemblies = assemblies[..indexPlace].ToArray();
        _cachedTypes = default;
        return _cachedAssemblies;
    }

    public static Type[] GetLoadedTypes(bool ignoreCache = false)
    {
        if (!ignoreCache && _cachedTypes != default)
        {
            return _cachedTypes;
        }

        var assemblies = GetLoadedAssemblies(ignoreCache: ignoreCache);
        _cachedTypes = assemblies.SelectMany(assembly => assembly.ExportedTypes).ToArray();
        return _cachedTypes;
    }
}
