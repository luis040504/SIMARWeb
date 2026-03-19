using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Consult;

public class EditModel : PageModel
{
    [BindProperty]
    public ManifestDetailViewModel Manifest { get; set; } = new();

    public IActionResult OnGet(string id)
    {
        var found = DetailModel.SampleData.FirstOrDefault(m => m.Id == id);
        if (found is null) return NotFound();

        Manifest = found;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        // TODO: Llamar a la API de SIMAR para actualizar el manifiesto

        TempData["SuccessMessage"] = $"Manifiesto {Manifest.ManifestNumber} actualizado correctamente.";
        return RedirectToPage("/Manifest/Consult/Index");
    }
}
