using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CommanCenter.Portal.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet() { }
}
