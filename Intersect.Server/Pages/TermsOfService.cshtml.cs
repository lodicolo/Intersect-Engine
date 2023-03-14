using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intersect.Server.Pages;

public class TermsOfServiceModel : PageModel
{
    private readonly ILogger<TermsOfServiceModel> _logger;

    public TermsOfServiceModel(ILogger<TermsOfServiceModel> logger)
    {
        _logger = logger;
    }
}