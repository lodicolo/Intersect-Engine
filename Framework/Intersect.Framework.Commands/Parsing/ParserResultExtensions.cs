using System.Collections.Immutable;
using Intersect.Framework.Commands.Arguments;

namespace Intersect.Framework.Commands.Parsing;

public static class ParserResultExtensions
{
    public static TValue Find<TValue>(
        this ParserResult result,
        CommandArgument<TValue>? argument,
        int index = 0,
        bool allowImplicit = true
    ) => result.Parsed.Find<TValue>(argument, index, allowImplicit);

    public static IEnumerable<TValue> FindAll<TValue>(
        this ParserResult result,
        ArrayCommandArgument<TValue> argument
    ) => result.Parsed.FindAll(argument);

    public static bool TryFind<TValue>(
        this ParserResult result,
        CommandArgument<TValue>? argument,
        out TValue value,
        int index = 0,
        bool allowImplicit = true
    ) => result.Parsed.TryFind(argument, out value, index, allowImplicit);

    public static bool TryFindAll<TValue>(
        this ParserResult result,
        ArrayCommandArgument<TValue> argument,
        out IEnumerable<TValue> values
    ) => result.Parsed.TryFindAll(argument, out values);

    public static ParserContext AsContext(
        this ParserResult result,
        bool filterOmitted = false,
        IEnumerable<ICommandArgument>? filterOut = default
    ) => new()
    {
        Command = result.Command,
        Errors = result.Errors,
        Parsed = result.Parsed.Values.Where(
            pair => !filterOmitted || !result.Omitted.Contains(pair.Key) ||
                    !(filterOut?.Contains(pair.Key) ?? false)).ToImmutableDictionary()
    };
}
