using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Intersect.Logging;

using JetBrains.Annotations;

using Microsoft;

using Newtonsoft.Json;

namespace Intersect.Localization
{
    public sealed class NamespaceMap<TNamespace> where TNamespace : LocaleNamespace
    {
        private readonly string mDirectory;
        private readonly string mName;

        private readonly IDictionary<int, TNamespace> mLocaleNamespaces;

        public IList<string> Available { get; }

        public NamespaceMap([NotNull, ValidatedNotNull] string directory, [NotNull, ValidatedNotNull] string name)
        {
            mDirectory = directory;
            if (!Directory.Exists(directory))
            {
                throw new ArgumentException($"Invalid directory: {directory}", nameof(directory));
            }

            mName = name;
            Available = ImmutableList.CreateRange<string>(
                Directory.EnumerateFiles(directory)
                    .Where(fileName => fileName.StartsWith(mName, StringComparison.Ordinal))
            );

            if (Available.Count < 1)
            {
                throw new ArgumentException($"No localizations available for '{name}' in '{directory}'.", nameof(name));
            }

            mLocaleNamespaces = new Dictionary<int, TNamespace>();
        }

        public TNamespace For(int localeId) => For(new CultureInfo(localeId));

        public TNamespace For(string localeName) => For(new CultureInfo(localeName));

        public TNamespace For(CultureInfo cultureInfo)
        {
            if (mLocaleNamespaces.TryGetValue(cultureInfo.LCID, out var localeNamespace))
            {
                return localeNamespace;
            }

            if (!TryLoad(cultureInfo, out localeNamespace))
            {
                if (cultureInfo.Parent.LCID == cultureInfo.LCID)
                {
                    return mLocaleNamespaces.Values.FirstOrDefault();
                }

                return For(cultureInfo.Parent);
            }

            return localeNamespace;
        }

        private bool TryLoad(CultureInfo cultureInfo, out TNamespace localeNamespace)
        {
            localeNamespace = default;

            var fileName = $"{mName}.{(string.IsNullOrEmpty(cultureInfo.Name) ? "" : $"{cultureInfo.Name}.")}.json";
            if (!Available.Contains(fileName))
            {
                return false;
            }

            var filePath = Path.Combine(mDirectory, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Missing expected translation file for {cultureInfo.Name}.", filePath);
            }

            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        var contents = reader.ReadToEnd();
                        localeNamespace = JsonConvert.DeserializeObject<TNamespace>(contents);
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }
        }
    }
}
