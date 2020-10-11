﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Intersect.Reflection
{
    public static class TypeExtensions
    {
        public static string QualifiedGenericName(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return
                $"{type.Name}<{string.Join(", ", type.GenericTypeArguments.Select(parameterType => parameterType.Name))}>";
        }

        public static PropertyInfo FindProperty(
            this Type type,
            string propertyName,
            StringComparison stringComparison = StringComparison.Ordinal
        )
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetProperties()
                .FirstOrDefault(propertyInfo => string.Equals(propertyInfo.Name, propertyName, stringComparison));
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type, bool publicSetter = true)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetProperties()
                .Where(
                    propertyInfo => (propertyInfo.GetMethod?.IsPublic ?? false) &&
                                    (!publicSetter || (propertyInfo.SetMethod?.IsPublic ?? false))
                );
        }

        public static IEnumerable<ConstructorInfo> FindConstructors(this Type type, params object[] parameters)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetConstructors()
                .Where(
                    constructor =>
                    {
                        var constructorParameters = constructor.GetParameters();
                        if (constructorParameters.Length < parameters.Length)
                        {
                            return false;
                        }

                        for (var index = 0; index < constructorParameters.Length; ++index)
                        {
                            var constructorParameter = constructorParameters[index];
                            Debug.Assert(constructorParameter != null, nameof(constructorParameter) + " != null");

                            if (index >= parameters.Length)
                            {
                                return constructorParameter.IsOptional;
                            }

                            var parameter = parameters[index];

                            if (parameter == null)
                            {
                                if (constructorParameter.ParameterType.IsValueType)
                                {
                                    return false;
                                }

                                continue;
                            }

                            var parameterType = parameter.GetType();
                            if (!constructorParameter.ParameterType.IsAssignableFrom(parameterType))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                );
        }
    }
}