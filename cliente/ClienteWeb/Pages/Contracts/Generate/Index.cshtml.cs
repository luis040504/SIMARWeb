using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Contracts.Generate
{
    public class GenerateModel : PageModel
    {
        [BindProperty]
        public int QuotationId { get; set; }

        [BindProperty]
        public string BusinessName { get; set; }

        [BindProperty]
        public string RFC { get; set; }

        [BindProperty]
        public string FiscalAddress { get; set; }

        [BindProperty]
        public string ServiceDetails { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        public string PaymentMethod { get; set; }

        [BindProperty]
        public string Validity { get; set; }

        [BindProperty]
        public string LegalClauses { get; set; }

        public void OnGet()
        {
            // Cargar cotizaciones aprobadas
        }

        public bool ShowPreview { get; set; }

        public void OnPost(string action)
        {
            if (action == "preview")
            {
                BusinessName = "Empresa X";
                ServiceDetails = "Desarrollo de software";
                Price = 10000;
                PaymentMethod = "Transferencia";
                Validity = "12 meses";

                ShowPreview = true;
            }
            else if (action == "confirm")
            {
                // Generar pdf y redigir a la página de éxito
            }
            else if (action == "cancel")
            {
                ShowPreview = false;
            }
        }
    }
}