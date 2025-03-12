using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using CommandLine;
using Intersect.Framework.Logging;
using Intersect.Framework.Reflection;
using Intersect.VersionManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Octokit;
using Serilog;
using Serilog.Core;
using Serilog.Events;

var entryAssembly = Assembly.GetExecutingAssembly();

var builder = Host.CreateDefaultBuilder();
builder.ConfigureHostConfiguration(
    cb =>
    {
        cb.AddUserSecrets(entryAssembly);
    }
);

builder.ConfigureServices(
    (hbc, services) =>
    {
        var pagingSection = hbc.Configuration.GetSection("Paging");
        services.Configure<ApiOptions>(pagingSection);

        var gitHubSection = hbc.Configuration.GetSection("GitHub");
        services.Configure<GitHubOptions>(gitHubSection);

        services.AddSingleton<GitHubClient>(
            sp =>
            {
                var githubOptions = sp.GetRequiredService<IOptions<GitHubOptions>>();
                var accessToken = githubOptions?.Value.AccessToken;
                ProductHeaderValue productInformation = new(
                    entryAssembly.GetName().Name,
                    entryAssembly.GetMetadataVersion()
                );
                GitHubClient client = new(productInformation)
                {
                    Credentials = string.IsNullOrWhiteSpace(accessToken)
                        ? Credentials.Anonymous
                        : new Credentials(accessToken),
                };
                return client;
            }
        );
    }
);

var app = builder.Build();

var pagingOptions = app.Services.GetService<IOptions<ApiOptions>>()?.Value ?? new ApiOptions();
pagingOptions.StartPage = Math.Max(1, pagingOptions.StartPage ?? 1);
pagingOptions.PageSize = Math.Max(10, pagingOptions.PageSize ?? 10);
pagingOptions.PageCount = Math.Max(1, pagingOptions.PageCount ?? 1);

var gitHubOptions = app.Services.GetRequiredService<IOptions<GitHubOptions>>();
var githubClient = app.Services.GetRequiredService<GitHubClient>();
var repositoryOptions = gitHubOptions.Value.Repository;

LoggingLevelSwitch loggingLevelSwitch =
    new(Debugger.IsAttached ? LogEventLevel.Debug : LogEventLevel.Information);

var (loggerFactory, logger) = new LoggerConfiguration().CreateLoggerForIntersect(
    entryAssembly,
    "VersionManager",
    loggingLevelSwitch
);

Parser parser = new(
    parserSettings =>
    {
        ArgumentNullException.ThrowIfNull(parserSettings, nameof(parserSettings));

        parserSettings.AutoHelp = true;
        parserSettings.AutoVersion = true;
        parserSettings.IgnoreUnknownArguments = true;
    }
);

var parsedArguments = parser.ParseArguments<DownloadOptions, FindOptions>(args);
var result = await parsedArguments.MapResult<DownloadOptions, FindOptions, Task<int>>(
    HandleDownloadOptions,
    HandleFindOptions,
    HandleParserErrors
);

return result;

async Task<int> HandleDownloadOptions(DownloadOptions options)
{
    return 0;
}

async Task<int> HandleFindOptions(FindOptions options)
{
    Release release;

    var tag = options.Tag;
    try
    {
        if (tag == ReleaseVerbOptions.TagLatest)
        {
            var releases = await githubClient.Repository.Release.GetAll(
                repositoryOptions.Owner,
                repositoryOptions.Repository,
                new ApiOptions
                {
                    StartPage = 1,
                    PageSize = 1,
                    PageCount = 1
                }
            );

            if (releases.Count < 1)
            {
                logger.LogError("No releases found");
                return -1;
            }

            release = releases[0];
        }
        else
        {
            release = await githubClient.Repository.Release.Get(
                repositoryOptions.Owner,
                repositoryOptions.Repository,
                tag
            );
        }
    }
    catch (Exception exception)
    {
        logger.LogError(
            exception,
            "Failed to get the {Tag} release from {Owner}/{Repository}",
            tag,
            repositoryOptions.Owner,
            repositoryOptions.Repository
        );
        return 0x1000;
    }

    logger.LogInformation("{ReleaseName} ({PublishedAt})", release.Name, release.PublishedAt);
    logger.LogInformation("Author: {Author}", release.Author.Login);
    logger.LogInformation("\t{ReleaseBody}", release.Body);
    logger.LogInformation("\nAssets ({AssetCount}):", release.Assets.Count);
    foreach (var asset in release.Assets)
    {
        logger.LogInformation("\t{AssetName} {AssetUrl}", asset.Name, asset.BrowserDownloadUrl);
    }

    return 0;
}

Task<int> HandleParserErrors(IEnumerable<Error> errors)
{
    var errorsAsList = errors.ToList();
    var fatalParsingError = errorsAsList.Any(error => error.StopsProcessing);
    var errorString = string.Join(", ", errorsAsList.ToList().Select(error => error.ToString()));

    var exception = new ArgumentException(
        $@"Error parsing command line arguments, received the following errors: {errorString}"
    );

    if (fatalParsingError)
    {
        logger.LogCritical(
            exception,
            "Critical error during command line argument parsing"
        );
    }
    else
    {
        logger.LogWarning(
            exception,
            "Error occurred during command line argument parsing"
        );
    }

    return Task.FromResult(fatalParsingError ? 1 : 0);
}