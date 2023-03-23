using Intersect.Framework.Application;

namespace Intersect.Server;

internal partial class ProgramHost
{
    private readonly string[] _args;

    private readonly IntersectApplicationBuilder _builder;

    private IntersectApplication _application;

    internal ProgramHost(string[] args)
    {
        _args = args;
        _builder = IntersectApplication.CreateDefaultBuilder(args, typeof(ProgramHost).Assembly);
    }

    public void Run()
    {
        _builder.ConfigureApplicationBuilder(ConfigureApplicationBuilder);

        _application = _builder.Build();

        _application.Run();
    }
}