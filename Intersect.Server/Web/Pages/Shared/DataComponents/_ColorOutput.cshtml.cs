using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intersect.Server.Web.Pages.Shared.DataComponents;

public class ColorOutputModel : PageModel
{
    public Color Value { get; set; }
}