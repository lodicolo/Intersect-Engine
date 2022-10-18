using Intersect.Enums;
using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.GameObjects;
using Intersect.Server.Database.GameData;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class GetVariableCommand : HelpableCommand
{
    public GetVariableCommand() : base(
        Strings.Commands.GetVariable,
        Strings.Commands.Arguments.Help,
        new VariableArgument<string>(
            Strings.Commands.Arguments.VariableId,
            RequiredIfNotHelp,
            true
        )
    ) { }

    private VariableArgument<string> ServerVariableId => FindArgument<VariableArgument<string>>();

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        var serverVariableNameOrId = result.Find(ServerVariableId);

        if (string.IsNullOrEmpty(serverVariableNameOrId))
        {
            throw new ArgumentNullException(
                Strings.Commands.Arguments.VariableId.Name,
                "No server variable specified."
            );
        }

        ServerVariableBase variable;
        if (Guid.TryParse(
                serverVariableNameOrId,
                out var serverVariableId
            ))
        {
            variable = GameContext.Queries.ServerVariableById(serverVariableId);
        }
        else
        {
            variable = GameContext.Queries.ServerVariableByName(serverVariableNameOrId);
        }

        if (variable == default)
        {
            Console.WriteLine(Strings.Commandoutput.VariableNotFound.ToString(serverVariableNameOrId));
            return;
        }

        string formattedValue = variable.Value.Value?.ToString();

        if (variable.Value.Type == VariableDataTypes.String)
        {
            formattedValue = $"\"{variable.Value.String}\"";
        }

        Console.WriteLine(
            Strings.Commandoutput.VariablePrint.ToString(
                variable.Id,
                variable.Name,
                formattedValue
            )
        );
    }
}
