using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Parsing;
using Intersect.Framework.Commands.Parsing.Errors;
using Intersect.Framework.Services;
using Intersect.Server.Commands;
using Intersect.Server.Localization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Services;

using Console = System.Console;

public sealed class ConsoleService
    : IntersectBackgroundService<ConsoleService, ConsoleServiceOptions, ConsoleServiceOptionsSetup>
{
    private const string WaitPrefix = "> ";

    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ICommandContext _commandContext;
    private readonly CommandParser _parser;

    public ConsoleService(
        IOptions<ConsoleServiceOptions> options,
        ILogger<ConsoleService> logger,
        IHostApplicationLifetime applicationLifetime,
        IServiceProvider serviceProvider
    ) : base(options, logger)
    {
        _applicationLifetime = applicationLifetime;

        _commandContext = new ConsoleServiceCommandContext(_applicationLifetime, serviceProvider);

        _parser = new(new(localization: Strings.Commands.Parsing));

        _parser.Register<AnnouncementCommand>();
        _parser.Register<ApiCommand>();
        _parser.Register<ApiGrantCommand>();
        _parser.Register<ApiRevokeCommand>();
        _parser.Register<ApiRolesCommand>();
        _parser.Register<BanCommand>();
        _parser.Register<CpsCommand>();
        _parser.Register<ExitCommand>();
        _parser.Register<ExperimentsCommand>();
        _parser.Register<GetVariableCommand>();
        _parser.Register<HelpCommand>(_parser.Settings);
        _parser.Register<KickCommand>();
        _parser.Register<KillCommand>();
        _parser.Register<ListVariablesCommand>();
        _parser.Register<MetricsCommand>();
        _parser.Register<MakePrivateCommand>();
        _parser.Register<MakePublicCommand>();
        _parser.Register<MigrateCommand>();
        _parser.Register<MuteCommand>();
        _parser.Register<NetDebugCommand>();
        _parser.Register<OnlineListCommand>();
        _parser.Register<PowerAccountCommand>();
        _parser.Register<PowerCommand>();
        _parser.Register<SetVariableCommand>();
        _parser.Register<UnbanCommand>();
        _parser.Register<UnmuteCommand>();
    }

    /// <inheritdoc />
    protected override Task ExecuteServiceAsync(CancellationToken cancellationToken)
    {
        return Task.Run(
            () =>
            {
                Console.WriteLine(Strings.Intro.consoleactive);

                // ReSharper disable once TooWideLocalVariableScope
                string? inputLine;
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.Write(WaitPrefix);
                    inputLine = Console.ReadLine()?.Trim();

                    if (inputLine == default)
                    {
                        _applicationLifetime.StopApplication();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(inputLine))
                    {
                        continue;
                    }

                    var result = _parser.Parse(inputLine);
                    var shouldHelp = result.Command is IHelpableCommand helpable && result.Find(helpable.Help);
                    if (result.Missing.Count < 1)
                    {
                        var fatalError = false;
                        foreach (var error in result.Errors)
                        {
                            fatalError = error.IsFatal;
                            if (error is MissingArgumentError)
                            {
                                return false;
                            }

                            Logger.LogWarning(error.Exception, ConsoleServiceStrings.ConsoleService_NonFatalErrorOccurredDuringCommandParsing);

                            Console.WriteLine(error.Message);
                        }

                        if (!fatalError)
                        {
                            if (!shouldHelp)
                            {
                                try
                                {
                                    result.Command.Handle(_commandContext, result);

                                    if (_commandContext.IsShutdownRequested)
                                    {
                                        return cancellationToken.WaitHandle.WaitOne();
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Logger.LogError(exception, message: ConsoleServiceStrings.ConsoleService_NonFatalErrorOccurredDuringCommandParsing);
                                }

                                continue;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(
                            Strings.Commands.Parsing.Errors.MissingArguments.ToString(
                                string.Join(
                                    Strings.Commands.Parsing.Errors.MissingArgumentsDelimeter,
                                    result.Missing.Select(
                                        argument =>
                                        {
                                            var typeName = argument.ValueType.Name;

                                            if (Strings.Commands.Parsing.TypeNames.TryGetValue(
                                                    typeName,
                                                    out var localizedType
                                                ))
                                            {
                                                typeName = localizedType;
                                            }

                                            return argument.Name +
                                                   Strings.Commands.Parsing.Formatting.Type.ToString(typeName);
                                        }
                                    )
                                )
                            )
                        );
                    }

                    var command = result.Command;
                    Console.WriteLine(command.FormatUsage(_parser.Settings, result.AsContext(true), true));

                    if (!shouldHelp)
                    {
                        continue;
                    }

                    Console.WriteLine($@"    {command.Description}");
                    Console.WriteLine();

                    var requiredBuffer = command.Arguments.Count == 1 ? string.Empty : new(
                        ' ',
                        Strings.Commands.RequiredInfo.ToString().Length
                    );

                    foreach (var argument in command.UnsortedArguments)
                    {
                        var shortName = argument.HasShortName ? argument.ShortName.ToString() : null;
                        var name = argument.Name;

                        var typeName = argument.ValueType.Name;
                        if (argument.IsFlag)
                        {
                            typeName = Strings.Commands.FlagInfo;
                        }
                        else if (Strings.Commands.Parsing.TypeNames.TryGetValue(typeName, out var localizedType))
                        {
                            typeName = localizedType;
                        }

                        if (!argument.IsPositional)
                        {
                            shortName = _parser.Settings.PrefixShort + shortName;
                            name = _parser.Settings.PrefixLong + name;
                        }

                        var names = string.Join(
                            ", ",
                            new[] { shortName, name }.Where(nameString => !string.IsNullOrWhiteSpace(nameString))
                        );

                        var required = argument.IsRequiredByDefault ? Strings.Commands.RequiredInfo.ToString()
                            : requiredBuffer;

                        var descriptionSegment = string.IsNullOrEmpty(argument.Description) ? string.Empty
                            : $@" - {argument.Description}";

                        Console.WriteLine($@"    {names,-16} {typeName,-12} {required}{descriptionSegment}");
                    }

                    Console.WriteLine();

                    Thread.Yield();
                }

                return true;
            },
            cancellationToken
        );
    }
}
