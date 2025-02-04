using System.Collections;
using System.Resources;
using Newtonsoft.Json;

namespace Intersect.Framework.Resources;

public sealed class JsonResourceReader : IResourceReader
{
    private readonly string _source;

    public JsonResourceReader(string source)
    {
        _source = source;
    }

    public JsonResourceReader(FileInfo fileInfo)
    {
        _source = File.ReadAllText(fileInfo.FullName);
    }

    public void Close()
    {
    }

    public IDictionaryEnumerator GetEnumerator() =>
        JsonConvert.DeserializeObject<Dictionary<string, object?>>(_source)?.GetEnumerator() ??
        throw new InvalidOperationException("Invalid JSON resource structure");

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
    }
}