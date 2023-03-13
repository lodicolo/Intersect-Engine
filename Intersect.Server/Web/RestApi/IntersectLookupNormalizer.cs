using Microsoft.AspNetCore.Identity;

namespace Intersect.Server.Web.RestApi;

public sealed class IntersectLookupNormalizer : ILookupNormalizer
{
    public string NormalizeName(string name) => name;

    public string NormalizeEmail(string email) => email;
}