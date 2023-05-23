using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intersect.Update.Generator
{
    public interface IAssetPacker
    {
        Task<List<string>> Run();
    }
}