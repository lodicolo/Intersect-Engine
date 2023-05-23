using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Serilog;
using Serilog.Events;

namespace Intersect.Update.Generator
{
    internal sealed class UpdateOptionsBinder : BinderBase<UpdateOptions>
    {
        private readonly Argument<string> _argumentSourceDirectory;
        private readonly Argument<string> _argumentTargetDirectory;

        private readonly Option<string> _optionClientExecutableName;
        private readonly Option<bool> _optionDebug;
        private readonly Option<string> _optionEditorExecutableName;
        private readonly Option<uint> _optionMusicTrackPackSize;
        private readonly Option<uint> _optionSoundEffectPackSize;
        private readonly Option<uint> _optionTextureAtlasPackSize;

        private UpdateOptionsBinder()
        {
            _argumentSourceDirectory = new Argument<string>("source-directory", ParseSourceDirectory);
            _argumentTargetDirectory = new Argument<string>("target-directory", ParseTargetDirectory);

            _optionClientExecutableName = new Option<string>(
                new[] { "-C", "--client-executable" },
                "The name of the client executable, e.g. \"Intersect Client\"."
            );
            _optionClientExecutableName.SetDefaultValue("Intersect Client");

            _optionDebug = new Option<bool>(new[] { "-d", "--debug" }, "Enable debugging mode");

            _optionEditorExecutableName = new Option<string>(
                new[] { "-E", "--editor-executable" },
                "The name of the editor executable, e.g. \"Intersect Editor\"."
            );
            _optionEditorExecutableName.SetDefaultValue("Intersect Editor");
            
            _optionMusicTrackPackSize = new Option<uint>(
                new[] { "-m", "--music-tracks" },
                "The maximum size of a single music track pack file in MiB (default 8 MiB), 0 to disable packing music tracks."
            );
            _optionMusicTrackPackSize.SetDefaultValue(8);

            _optionSoundEffectPackSize = new Option<uint>(
                new[] { "-s", "--sound-effects" },
                "The maximum size of a single sound effect pack file in MiB (default 8 MiB), 0 to disable packing sound effects."
            );
            _optionSoundEffectPackSize.SetDefaultValue(8);

            _optionTextureAtlasPackSize = new Option<uint>(
                new[] { "-t", "--texture-atlas" },
                "The maximum dimension in pixels (default 2048px) of a square texture atlas, 0 to disable packing textures, must be a power of 2 and at least 256."
            );
            _optionTextureAtlasPackSize.AddValidator(ValidateTexturePackSize);
            _optionTextureAtlasPackSize.SetDefaultValue(2048);
        }

        private static string ParseSourceDirectory(ArgumentResult argumentResult)
        {
            return ParseDirectory(argumentResult, false);
        }

        private static string ParseTargetDirectory(ArgumentResult argumentResult)
        {
            return ParseDirectory(argumentResult, true);
        }

        private static string ParseDirectory(SymbolResult argumentResult, bool createIfMissing)
        {
            var value = argumentResult.Tokens.Single().Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                argumentResult.ErrorMessage = "Expected a valid path to a directory but received an empty string.";
                return default;
            }

            if (Directory.Exists(value))
            {
                return new DirectoryInfo(value).FullName;
            }

            if (createIfMissing)
            {
                return Directory.CreateDirectory(value).FullName;
            }

            argumentResult.ErrorMessage = $"Expected a valid path to a directory but received '{value}'.";
            return default;
        }

        private void ValidateTexturePackSize(OptionResult optionResult)
        {
            var value = optionResult.GetValueForOption(_optionTextureAtlasPackSize);
            if (value == 0)
            {
                return;
            }

            if ((value & (value - 1)) == 0 || value < 256)
            {
                return;
            }

            optionResult.ErrorMessage = $"Expected a power of 2 that is at least 256 but received {value}.";
        }

        protected override UpdateOptions GetBoundValue(BindingContext bindingContext)
        {
            return new UpdateOptions
            {
                ClientExecutableName = bindingContext.ParseResult.GetValueForOption(_optionClientExecutableName),
                Debug = bindingContext.ParseResult.GetValueForOption(_optionDebug),
                EditorExecutableName = bindingContext.ParseResult.GetValueForOption(_optionEditorExecutableName),
                PackingOptions =
                    new PackingOptions
                    {
                        MusicTrackPackSize =
                            bindingContext.ParseResult.GetValueForOption(_optionMusicTrackPackSize),
                        SoundEffectPackSize =
                            bindingContext.ParseResult.GetValueForOption(_optionSoundEffectPackSize),
                        TextureAtlasSize =
                            bindingContext.ParseResult.GetValueForOption(_optionTextureAtlasPackSize)
                    },
                SourceDirectory = bindingContext.ParseResult.GetValueForArgument(_argumentSourceDirectory),
                TargetDirectory = bindingContext.ParseResult.GetValueForArgument(_argumentTargetDirectory)
            };
        }

        public static void Configure<TCommand>(TCommand command) where TCommand : Command
        {
            var updateOptionsBinder = new UpdateOptionsBinder();

            command.AddOption(updateOptionsBinder._optionClientExecutableName);
            command.AddOption(updateOptionsBinder._optionDebug);
            command.AddOption(updateOptionsBinder._optionEditorExecutableName);
            command.AddOption(updateOptionsBinder._optionMusicTrackPackSize);
            command.AddOption(updateOptionsBinder._optionSoundEffectPackSize);
            command.AddOption(updateOptionsBinder._optionTextureAtlasPackSize);

            command.AddArgument(updateOptionsBinder._argumentSourceDirectory);
            command.AddArgument(updateOptionsBinder._argumentTargetDirectory);

            command.SetHandler(
                updateOptions =>
                {
                    var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext()
                        .WriteTo.Console()
                        .MinimumLevel.Is(updateOptions.Debug ? LogEventLevel.Debug : LogEventLevel.Information);
                    var updateGenerator = new UpdateGenerator(loggerConfiguration.CreateLogger(), updateOptions);
                    return updateGenerator.Run();
                },
                updateOptionsBinder
            );
        }
    }
}