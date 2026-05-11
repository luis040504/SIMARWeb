using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.WasteCatalog;

public class IndexModel : PageModel
{
    private readonly WasteCatalogApiService _api;

    public IndexModel(WasteCatalogApiService api) => _api = api;

    // ── Listado ───────────────────────────────────────────────────────────────
    public WasteCatalogPagedResult Result { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search    { get; set; }
    [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int     PageNumber { get; set; } = 1;

    // ── Mensajes ──────────────────────────────────────────────────────────────
    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage   { get; set; }

    // ── Formulario de creación ────────────────────────────────────────────────
    [BindProperty] public WasteCatalogUpsertDto CreateForm { get; set; } = new();

    // ── Formulario de edición ─────────────────────────────────────────────────
    [BindProperty] public WasteCatalogUpsertDto EditForm   { get; set; } = new();
    [BindProperty] public int EditId { get; set; }

    public bool HasSearched { get; private set; }
    public string? ApiError { get; private set; }

    // ── Handler GET ───────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        HasSearched = !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(TypeFilter);
        if (!HasSearched) return Page();

        try
        {
            Result = await _api.GetAllAsync(Search, TypeFilter, PageNumber);
        }
        catch (Exception)
        {
            ApiError = "No se pudo conectar con el servidor. Verifique que el servicio esté en línea.";
        }

        return Page();
    }

    // ── Handler POST Crear ────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        var (ok, error, _) = await _api.CreateAsync(CreateForm);
        if (ok)
            TempData["SuccessMessage"] = $"Residuo '{CreateForm.Name}' creado correctamente.";
        else
            TempData["ErrorMessage"] = error ?? "No se pudo crear el residuo.";

        return RedirectToPage(new { Search, TypeFilter, PageNumber });
    }

    // ── Handler POST Editar ───────────────────────────────────────────────────
    public async Task<IActionResult> OnPostEditAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        var (ok, error) = await _api.UpdateAsync(EditId, EditForm);
        if (ok)
            TempData["SuccessMessage"] = $"Residuo '{EditForm.Name}' actualizado correctamente.";
        else
            TempData["ErrorMessage"] = error ?? "No se pudo actualizar el residuo.";

        return RedirectToPage(new { Search, TypeFilter, PageNumber });
    }

    // ── Handler POST Eliminar ─────────────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        var (ok, error) = await _api.DeleteAsync(id);
        if (ok)
            TempData["SuccessMessage"] = "Residuo eliminado del catálogo.";
        else
            TempData["ErrorMessage"] = error ?? "No se pudo eliminar el residuo.";

        return RedirectToPage(new { Search, TypeFilter, PageNumber });
    }
}
