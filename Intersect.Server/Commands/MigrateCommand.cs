using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class MigrateCommand : HelpableCommand
{
    public MigrateCommand() : base(
        Strings.Commands.Migrate,
        Strings.Commands.Arguments.Help
    ) { }

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    ) => DbInterface.HandleMigrationCommand();
}
