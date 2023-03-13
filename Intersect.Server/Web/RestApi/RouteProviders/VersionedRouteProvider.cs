using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

using Intersect.Server.Web.RestApi.Routes;

namespace Intersect.Server.Web.RestApi.RouteProviders
{

    internal sealed partial class VersionedRouteProvider : DefaultDirectRouteProvider
    {

        public VersionedRouteProvider() : this(DefaultRootNamespace)
        {
        }

        public VersionedRouteProvider(string rootNamespace, string prefix = "api")
        {
            RootNamespace = rootNamespace;
            Prefix = prefix;
        }

        public static string DefaultRootNamespace =>
            typeof(RootInfoController).Namespace ?? throw new InvalidOperationException();

        public string RootNamespace { get; }

        public string Prefix { get; }

        protected override string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            var prefixBuilder = new StringBuilder(Prefix);

            var controllerType = controllerDescriptor.ControllerType;
            var namespaceSegments =
                controllerType?.Namespace?.Replace(RootNamespace, "").Split('.') ?? new string[] { };

            namespaceSegments.Where(segment => !string.IsNullOrWhiteSpace(segment))
                .ToList()
                .ForEach(
                    segment =>
                    {
                        prefixBuilder.Append('/');
                        prefixBuilder.Append(segment?.ToLower());
                    }
                );

            var prefix = base.GetRoutePrefix(controllerDescriptor)?.Trim();

            // More readable with less return points
            // ReSharper disable once InvertIf
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                prefixBuilder.Append('/');
                prefixBuilder.Append(prefix);
            }

            return prefixBuilder.ToString();
        }

    }

}
