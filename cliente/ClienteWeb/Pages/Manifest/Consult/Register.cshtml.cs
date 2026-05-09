using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Consult;

public class RegisterModel : PageModel
{
    private readonly ManifestApiService _api;

    public RegisterModel(ManifestApiService api) => _api = api;

    [BindProperty]
    public RegisterViewModel Input { get; set; } = new();

    public ManifestDetailViewModel ManifestInfo { get; private set; } = new();

    public string? ApiError { get; private set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        try
        {
            var found = await _api.GetByIdAsync(id);
            if (found is null) return NotFound();
            if (found.Status != ManifestStatus.EnTransito)
                return RedirectToPage("/Manifest/Consult/Detail", new { id });

            ManifestInfo = found;
            Input.Id = found.Id;
            Input.SignedDate = DateOnly.FromDateTime(DateTime.Today);
            return Page();
        }
        catch (BillingApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
        catch (Exception)
        {
            ApiError = "No se pudo cargar el manifiesto. Verifique que el servicio esté en línea.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.SignedFile is null)
            ModelState.AddModelError(nameof(Input.SignedFile), "Debes subir el PDF firmado.");

        if (!ModelState.IsValid)
        {
            var current = await _api.GetByIdAsync(Input.Id);
            if (current is not null) ManifestInfo = current;
            return Page();
        }

        try
        {
            await _api.UploadFirmaAsync(Input.Id, Input.SignedFile!);
            TempData["SuccessMessage"] = "El manifiesto ha sido marcado como completado con el PDF firmado.";
            return RedirectToPage("/Manifest/Consult/Detail", new { id = Input.Id });
        }
        catch (BillingApiException ex)
        {
            ApiError = ex.Message;
            var current = await _api.GetByIdAsync(Input.Id);
            if (current is not null) ManifestInfo = current;
            return Page();
        }
    }
}
