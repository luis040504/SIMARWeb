using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ClienteWeb.Pages.Manifest.Consult;

public class RegisterModel : PageModel
{
    [BindProperty]
    public RegisterViewModel Input { get; set; } = new();

    public ManifestDetailViewModel ManifestInfo { get; private set; } = new();

    public IActionResult OnGet(string id)
    {
        var found = DetailModel.SampleData.FirstOrDefault(m => m.Id == id);
        if (found is null)
            return NotFound();

        if (found.Status != ManifestStatus.EnTransito)
            return RedirectToPage("/Manifest/Consult/Detail", new { id });

        ManifestInfo = found;
        Input.Id = found.Id;
        Input.SignedDate = DateOnly.FromDateTime(DateTime.Today);

        return Page();
    }

    public IActionResult OnPost()
    {
        var found = DetailModel.SampleData.FirstOrDefault(m => m.Id == Input.Id);
        if (found is null)
            return NotFound();

        if (Input.SignedFile is null)
            ModelState.AddModelError(nameof(Input.SignedFile), "Debes subir el PDF firmado.");

        if (!ModelState.IsValid)
        {
            ManifestInfo = found;
            return Page();
        }

        found.Status = ManifestStatus.Completado;
        found.SignedDate = Input.SignedDate;
        found.SignedManifestFileName = Input.SignedFile!.FileName;

        TempData["SuccessMessage"] =
            $"El manifiesto {found.ManifestNumber} ha sido marcado como completado.";

        return RedirectToPage("/Manifest/Consult/Detail", new { id = found.Id });
    }
}

public class RegisterViewModel
{
    public string Id { get; set; } = string.Empty;

    public DateOnly SignedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public IFormFile? SignedFile { get; set; }
}
