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
        if (found == null) return NotFound();

        ManifestInfo = found;
        
        Input.Id = found.Id;
        Input.Status = found.Status;
        Input.SignedDate = found.SignedDate ?? DateOnly.FromDateTime(DateTime.Today);
        
        return Page();
    }

    public IActionResult OnPost()
{
    var found = DetailModel.SampleData.FirstOrDefault(m => m.Id == Input.Id);
    if (found == null) return NotFound();

    if (!ModelState.IsValid)
    {
        ManifestInfo = found;
        return Page();
    }

    found.Status = Input.Status;

    if (Input.Status == ManifestStatus.Completado)
    {
        found.SignedDate = Input.SignedDate;

        if (Input.SignedFile != null)
        {
            found.SignedManifestFileName = Input.SignedFile.FileName;
        }
        else if (string.IsNullOrEmpty(found.SignedManifestFileName))
        {
            found.SignedManifestFileName = $"{found.ManifestNumber}_firmado.pdf";
        }
    }

    TempData["SuccessMessage"] = $"El estado del manifiesto {found.ManifestNumber} ha sido actualizado correctamente a {Input.Status}.";
    return RedirectToPage("/Manifest/Consult/Index");
}
}

public class RegisterViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    public ManifestStatus Status { get; set; }

    public DateOnly SignedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public IFormFile? SignedFile { get; set; }
}
