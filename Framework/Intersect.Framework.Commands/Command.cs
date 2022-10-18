using System.Collections.Immutable;
using System.Text;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Localization;

namespace Intersect.Framework.Commands;

public abstract class Command : ICommand
{
    protected Command(
        LocaleCommand localization,
        params ICommandArgument[] arguments
    )
    {
        Localization = localization;

        var argumentList = new List<ICommandArgument>(arguments);

        UnsortedArguments = argumentList.ToImmutableList() ?? throw new InvalidOperationException();

        argumentList.Sort(
            (
                a,
                b
            ) => a.IsRequiredByDefault switch
            {
                true when !b.IsRequiredByDefault => -1,
                false when b.IsRequiredByDefault => 1,
                _ => a.IsPositional switch
                {
                    true => b.IsPositional ? 0 : -1,
                    _ => b.IsPositional ? 1 : 0
                }
            }
        );

        Arguments = argumentList.ToImmutableList() ?? throw new InvalidOperationException();

        NamedArguments =
            argumentList.Where(
                argument => !argument?.IsPositional ??
                            throw new InvalidOperationException(@"No null arguments should be in the list.")
            ).ToImmutableList() ?? throw new InvalidOperationException();

        PositionalArguments =
            argumentList.Where(
                argument => argument?.IsPositional ??
                            throw new InvalidOperationException(@"No null arguments should be in the list.")
            ).ToImmutableList() ?? throw new InvalidOperationException();
    }

    public LocaleCommand Localization { get; }

    public ImmutableList<ICommandArgument> Arguments { get; }

    public ImmutableList<ICommandArgument> UnsortedArguments { get; }

    public ImmutableList<ICommandArgument> NamedArguments { get; }

    public ImmutableList<ICommandArgument> PositionalArguments { get; }

    public string Name => Localization.Name;

    public string Description => Localization.Description;

    public ICommandArgument FindArgument(char shortName) =>
        Arguments.FirstOrDefault(argument => argument?.ShortName == shortName);

    public ICommandArgument FindArgument(string name) => Arguments.FirstOrDefault(argument => argument?.Name == name);

    public string FormatUsage(
        ParserSettings parserSettings,
        ParserContext parserContext,
        bool formatPrint = false
    )
    {
        var usageBuilder = new StringBuilder(Name);

        foreach (var argument in Arguments)
        {
            var argumentUsageBuilder = new StringBuilder(argument.Name);
            if (!argument.IsPositional)
            {
                argumentUsageBuilder.Insert(
                    0,
                    parserSettings.PrefixLong
                );
            }

            if (!argument.IsFlag)
            {
                var typeName = argument.ValueType.Name;
                if (parserSettings.Localization.TypeNames.TryGetValue(
                        typeName,
                        out var localizedType
                    ))
                {
                    typeName = localizedType;
                }

                if (argument.IsPositional)
                {
                    argumentUsageBuilder.Append(parserSettings.Localization.Formatting.Type.ToString(typeName));
                }
                else
                {
                    argumentUsageBuilder.Append('=');
                    argumentUsageBuilder.Append(typeName);
                }
            }

            var argumentUsage = argumentUsageBuilder.ToString();
            if (!argument.IsRequired(parserContext))
            {
                argumentUsage = parserSettings.Localization.Formatting.Optional.ToString(argumentUsage);
            }

            usageBuilder.Append(' ');
            usageBuilder.Append(argumentUsage);
        }

        var usage = usageBuilder.ToString().Trim();

        if (formatPrint)
        {
            usage = parserSettings.Localization.Formatting.Usage.ToString(usage);
        }

        return usage;
    }

    public void Handle(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        if (result.Errors.Any(error => error?.IsFatal ?? false))
        {
            throw new InvalidOperationException(@"Errors should have been handled before invoking ICommand.Handle().");
        }

        HandleSuccess(
            commandContext,
            result
        );
    }

    protected TArgument? FindArgument<TArgument>(int index = 0) =>
        Arguments.OfType<TArgument>().ElementAtOrDefault(index);

    protected TArgument FindArgumentOrThrow<TArgument>(int index = 0)
    {
        var argument = FindArgument<TArgument>(index);

        if (argument == null)
        {
            throw new InvalidOperationException($@"Unable to find argument type {typeof(TArgument).FullName}.");
        }

        return argument;
    }

    protected abstract void HandleSuccess(
        ICommandContext commandContext,
        ParserResult result
    );
}
