using System.Globalization;
using Intersect.Enums;
using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database;
using Intersect.Server.Database.GameData;
using Intersect.Server.Entities;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class SetVariableCommand : HelpableCommand
{
    public SetVariableCommand() : base(
        Strings.Commands.SetVariable,
        Strings.Commands.Arguments.Help,
        new VariableArgument<string>(
            Strings.Commands.Arguments.VariableId,
            RequiredIfNotHelp,
            true
        ),
        new VariableArgument<string>(
            Strings.Commands.Arguments.VariableValue,
            RequiredIfNotHelp,
            true
        )
    ) { }

    private VariableArgument<string> ServerVariableId => FindArgument<VariableArgument<string>>();
    private VariableArgument<string> ServerVariableValue => FindArgument<VariableArgument<string>>(1);

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        var serverVariableNameOrId = result.Find(ServerVariableId);
        var rawServerVariableValue = result.Find(ServerVariableValue);

        if (string.IsNullOrEmpty(serverVariableNameOrId))
        {
            throw new ArgumentNullException(
                Strings.Commands.Arguments.VariableId.Name,
                "No server variable specified."
            );
        }

        if (string.IsNullOrEmpty(rawServerVariableValue))
        {
            throw new ArgumentNullException(
                Strings.Commands.Arguments.VariableValue.Name,
                $"No value specified for server variable '{serverVariableNameOrId}'"
            );
        }

        var variable = Guid.TryParse(
            serverVariableNameOrId,
            out var serverVariableId
        ) ? GameContext.Queries.ServerVariableById(serverVariableId)
            : GameContext.Queries.ServerVariableByName(serverVariableNameOrId);

        if (variable == default)
        {
            Console.WriteLine(Strings.Commandoutput.VariableNotFound.ToString(serverVariableNameOrId));
            return;
        }

        var previousServerVariableValue = variable.Value?.Value;
        string formattedPreviousValue = previousServerVariableValue?.ToString() ?? string.Empty;
        string formattedValue = default;

        switch (variable.Value?.Type)
        {
            case VariableDataTypes.Boolean:
                variable.Value.Boolean = bool.Parse(rawServerVariableValue);
                break;

            case VariableDataTypes.Integer:
                variable.Value.Integer = int.Parse(
                    rawServerVariableValue,
                    NumberStyles.Integer,
                    CultureInfo.CurrentCulture
                );
                break;

            case VariableDataTypes.Number:
                variable.Value.Number = double.Parse(
                    rawServerVariableValue,
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture
                );
                break;

            case VariableDataTypes.String:
                variable.Value.String = rawServerVariableValue.ToString();
                formattedPreviousValue = $"\"{formattedPreviousValue}\"";
                formattedValue = $"\"{variable.Value.String}\"";
                break;

            default:
                throw new InvalidOperationException();
        }

        formattedValue ??= variable.Value?.ToString();

        Player.StartCommonEventsWithTriggerForAll(
            CommonEventTrigger.ServerVariableChange,
            string.Empty,
            variable.Id.ToString()
        );

        DbInterface.UpdatedServerVariables.AddOrUpdate(
            variable.Id,
            variable,
            (
                _,
                _
            ) => variable
        );

        Console.WriteLine(
            Strings.Commandoutput.VariableChanged.ToString(
                variable.Id,
                variable.Name,
                formattedValue,
                formattedPreviousValue
            )
        );
    }
}
