using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Intersect.Server.Web.RestApi.Routes;

[ApiController]
[Route("info")]
public class RootInfoController : ControllerBase
{
    static RootInfoController()
    {
        var rootNamespace = typeof(RootInfoController).Namespace ?? throw new InvalidOperationException();
        DiscoveredVersions = typeof(RootInfoController).Assembly.GetTypes()
            .Select(type => type.Namespace)
            .Distinct()
            .Where(ns => ns?.StartsWith(rootNamespace) ?? false)
            .Select(ns => ns.Substring(Math.Min(ns.Length, rootNamespace.Length + 1)).ToLowerInvariant())
            .Where(ns => !string.IsNullOrWhiteSpace(ns))
            .ToArray();
    }

    private static string[] DiscoveredVersions { get; }

    [HttpGet]
    public object Default()
    {
        return new
        {
#if DEBUG
            debug = true,
#endif
            versions = Versions()
        };
    }

    [HttpGet("versions")]
    public IEnumerable<string> Versions()
    {
        return DiscoveredVersions;
    }
}