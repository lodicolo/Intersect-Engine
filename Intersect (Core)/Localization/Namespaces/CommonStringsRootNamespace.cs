using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace Intersect.Localization.Namespaces
{
    public sealed class CommonStringsRootNamespace : LocaleNamespace
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore), NotNull]
        public readonly PluginsNamespace Plugins = new PluginsNamespace();
    }
}
