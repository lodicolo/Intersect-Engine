using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intersect.Server.Pages;

public abstract class IntersectPageModel : PageModel
{
    [ViewData] public string GameName => Options.Instance.GameName;
}