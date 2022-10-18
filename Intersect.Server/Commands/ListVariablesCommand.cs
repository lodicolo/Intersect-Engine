using Intersect.Enums;
using Intersect.Extensions;
using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database.GameData;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class ListVariablesCommand : HelpableCommand
{
    public ListVariablesCommand() : base(
        Strings.Commands.ListVariables,
        Strings.Commands.Arguments.Help,
        new VariableArgument<int>(Strings.Commands.Arguments.Page, positional: true, defaultValue: 0),
        new VariableArgument<int>(Strings.Commands.Arguments.PageSize, positional: true, defaultValue: 10)
    ) { }

    private VariableArgument<int> Page => FindArgument<VariableArgument<int>>();
    private VariableArgument<int> PageSize => FindArgument<VariableArgument<int>>();

    protected override void HandleValue(ICommandContext commandContext, ParserResult result)
    {
        var page = Math.Max(0, result.Find(Page));
        var pageSize = Math.Clamp(result.Find(PageSize), 10, 100);

        var variables = GameContext.Queries.ServerVariables(page, pageSize).ToList();

        if (variables.Count < 1)
        {
            Console.WriteLine(Strings.Commandoutput.VariableListEmpty);
            return;
        }

        var padding = new string(' ', variables.Aggregate(0, (size, variable) => Math.Max(size, variable.Name.Length)));

        foreach (var variable in variables)
        {
            string formattedValue = variable.Value.Value.ToString();

            if (variable.Value.Type == VariableDataTypes.String)
            {
                formattedValue = $"\"{variable.Value.String}\"";
            }

            Console.WriteLine(
                Strings.Commandoutput.VariablePrint.ToString(
                    variable.Id,
                    (padding + variable.Name).Slice(-padding.Length),
                    formattedValue
                )
            );
        }
    }
}
