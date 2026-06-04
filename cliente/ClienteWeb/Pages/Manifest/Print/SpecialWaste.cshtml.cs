using ClienteWeb.Pages.Manifest.Consult;
using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Print;

public class SpecialWastePrintModel : PageModel
{
    private readonly ManifestApiService _api;
    public SpecialWastePrintModel(ManifestApiService api) => _api = api;

    public ManifestDetailViewModel Manifest { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        var found = await _api.GetByIdAsync(id);
        if (found is null) return NotFound();
        Manifest = found;
        return Page();
    }
}
