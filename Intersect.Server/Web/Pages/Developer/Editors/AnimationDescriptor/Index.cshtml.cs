using Intersect.Framework.Core.Models;
using Intersect.GameObjects;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intersect.Server.Web.Pages.Developer.Editors.AnimationDescriptor;

public class AnimationDescriptorEditorModel : PageModel
{
    private readonly TypeEditorModel _descriptorModel = TypeEditorModel.CreateFrom(typeof(AnimationBase));

    public void OnGet()
    {

    }
}