namespace Intersect.Framework.Application;

public sealed partial class IntersectApplicationBuilder
{
    private readonly string[]? _args;

    public IntersectApplicationBuilder(string[]? args)
    {
        _args = args;
    }

    public IntersectApplication Build()
    {
        Bootstrap();

        var host = BuildApplication();
        var intersectApplication = new IntersectApplication(host);
        return intersectApplication;
    }
}