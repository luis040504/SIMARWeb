using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Consult;

public class EditModel : PageModel
{
    private readonly ManifestApiService _api;

    public EditModel(ManifestApiService api) => _api = api;

    [BindProperty]
    public ManifestDetailViewModel Manifest { get; set; } = new();

    public string? ApiError { get; private set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        try
        {
            var found = await _api.GetByIdAsync(id);
            if (found is null) return NotFound();
            Manifest = found;
            return Page();
        }
        catch (BillingApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
        catch (Exception)
        {
            ApiError = "No se pudo cargar el manifiesto.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _api.UpdateAsync(Manifest.Id, Manifest);
            TempData["SuccessMessage"] = $"Manifiesto {Manifest.ManifestNumber} actualizado correctamente.";
            return RedirectToPage("/Manifest/Consult/Detail", new { id = Manifest.Id });
        }
        catch (BillingApiException ex)
        {
            ApiError = ex.Message;
            return Page();
        }
        catch (Exception)
        {
            ApiError = "No se pudo conectar con el servidor. Intente de nuevo.";
            return Page();
        }
    }
}
