using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteWeb.Pages.Contracts.Update
{
    public class UpdateModel : PageModel
    {
        [BindProperty]
        public string Id { get; set; } = "";

        [BindProperty]
        public string Client { get; set; } = "";

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        [BindProperty]
        public string ServiceConditions { get; set; } = "";

        [BindProperty]
        public string AdminObservations { get; set; } = "";

        [BindProperty]
        public IFormFile? PdfFile { get; set; }

        public string Status { get; set; } = "";

        public bool ShowSuccessMessage { get; set; }
        public string ErrorMessage { get; set; } = "";

        public List<string> AuditTrail { get; set; } = new();

        public IActionResult OnGet(string id)
        {
            // Redirigir a la vista de Consulta donde ahora vive el modal
            return RedirectToPage("/Contracts/Consult/Index");
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("/Contracts/Consult/Index");
        }
    }
}
