﻿using System.Collections.Immutable;
using Intersect.Localization;

namespace Intersect.Framework.Commands.Arguments;

public class EnumArgument<TValue> : CommandArgument<TValue>
{
    public EnumArgument(
        LocaleArgument localization,
        bool required = false,
        bool positional = false,
        params TValue[] allowedValues
    ) : base(localization, required, positional)
    {
        AllowedValues = (allowedValues ?? Array.Empty<TValue>()).ToImmutableArray();
    }

    public EnumArgument(
        LocaleArgument localization,
        bool required = false,
        bool positional = false,
        bool allowsMultiple = false,
        params TValue[] allowedValues
    ) : base(localization, required, positional, allowsMultiple)
    {
        AllowedValues = (allowedValues ?? Array.Empty<TValue>()).ToImmutableArray();
    }

    public EnumArgument(
        LocaleArgument localization,
        bool required = false,
        bool positional = false,
        bool allowsMultiple = false,
        TValue defaultValue = default,
        params TValue[] allowedValues
    ) : base(localization, required, positional, allowsMultiple, defaultValue)
    {
        AllowedValues = (allowedValues ?? Array.Empty<TValue>()).ToImmutableArray();
    }

    public EnumArgument(
        LocaleArgument localization,
        ArgumentRequiredPredicate requiredPredicate,
        bool positional = false,
        params TValue[] allowedValues
    ) : base(localization, requiredPredicate, positional)
    {
        AllowedValues = (allowedValues ?? Array.Empty<TValue>()).ToImmutableArray();
    }

    public EnumArgument(
        LocaleArgument localization,
        ArgumentRequiredPredicate requiredPredicate,
        bool positional = false,
        bool allowsMultiple = false,
        params TValue[] allowedValues
    ) : base(localization, requiredPredicate, positional, allowsMultiple)
    {
        AllowedValues = (allowedValues ?? Array.Empty<TValue>()).ToImmutableArray();
    }

    public EnumArgument(
        LocaleArgument localization,
        ArgumentRequiredPredicate requiredPredicate,
        bool positional = false,
        bool allowsMultiple = false,
        TValue defaultValue = default,
        params TValue[] allowedValues
    ) : base(localization, requiredPredicate, positional, allowsMultiple, defaultValue)
    {
        AllowedValues = (allowedValues ?? Array.Empty<TValue>()).ToImmutableArray();
    }

    public ImmutableArray<TValue> AllowedValues { get; }

    public override bool IsValueAllowed(object value) =>
        value is TValue castedValue && AllowedValues.Contains(castedValue);
}