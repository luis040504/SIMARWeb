using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ClienteWeb.Pages.Contracts.Generate
{
    public class GenerateModel : PageModel
    {
        [BindProperty]
        public string AnexosJsonHidden { get; set; } = "";
        
        [BindProperty]
        public int QuotationId { get; set; }

        public List<Quotation> Quotations { get; set; } = new();

        [BindProperty] public string BusinessName { get; set; } = "";
        [BindProperty] public string RFC { get; set; } = "";
        [BindProperty] public string Address { get; set; } = "";
        [BindProperty] public string Representative { get; set; } = "";
        [BindProperty] public string ServiceDetails { get; set; } = "";
        [BindProperty] public decimal Price { get; set; }
        [BindProperty] public string PaymentMethod { get; set; } = "";
        [BindProperty] public string Validity { get; set; } = "";

        public bool ShowPreview { get; set; }

        // Standardized properties for the frontend to map correctly
        public List<Anexo1Scope> Anexo1Items { get; set; } = new();
        public List<Anexo2Payment> Anexo2Payments { get; set; } = new();
        public List<Anexo3Schedule> Anexo3Steps { get; set; } = new();
        public List<Anexo4Extra> Anexo4Extras { get; set; } = new();

        public void OnGet() => LoadQuotations();

        public void OnPost(string action)
        {
            LoadQuotations();

            if (action == "preview")
            {
                var quotation = GetQuotationFromDatabase(QuotationId); 
                if (quotation != null)
                {
                    BusinessName = quotation.BusinessName;
                    RFC = quotation.RFC;
                    Address = quotation.Address;
                    Representative = quotation.Representative;
                    ServiceDetails = quotation.ServiceDetails;
                    Price = quotation.Price;
                    PaymentMethod = quotation.PaymentMethod;
                    Validity = quotation.Validity;

                    // Initialize with empty lists (No mocks)
                    Anexo1Items = new List<Anexo1Scope>();
                    Anexo2Payments = new List<Anexo2Payment>();
                    Anexo3Steps = new List<Anexo3Schedule>();
                    Anexo4Extras = new List<Anexo4Extra>();

                    ShowPreview = true;
                }
            }
        }

        private void LoadQuotations()
        {
            // Logic to load dropdown items from Database
            Quotations = new List<Quotation> {
                new Quotation { Id = 1, Name = "Client A - Quotation" }
            };
        }

private QuotationDetail? GetQuotationFromDatabase(int id)
{
    // SI YA TIENES TU DB CONECTADA, USA ESTO:
    // return _dbContext.QuotationDetails.FirstOrDefault(q => q.Id == id);

    // SI AÚN NO LA TIENES, DEJA ESTO PARA QUE LA VISTA PREVIA FUNCIONE HOY:
    return new QuotationDetail {
        Id = id,
        BusinessName = "Empresa X",
        RFC = "XAXX010101000",
        Address = "Av. Principal 123",
        Representative = "JUAN PÉREZ",
        ServiceDetails = "Recolección general",
        Price = 12500,
        PaymentMethod = "Transferencia",
        Validity = "12 meses",
        // Iniciamos la lista vacía para que el frontend no marque error
        Anexo1Items = new List<Anexo1Scope>() 
    };
}
    }

    // --- DOMAIN CLASSES (Fully English to match Frontend JS) ---

    public class Quotation { public int Id { get; set; } public string Name { get; set; } = ""; }

    public class QuotationDetail {
        public int Id { get; set; }
        public string BusinessName { get; set; } = "";
        public string RFC { get; set; } = "";
        public string Address { get; set; } = "";
        public string Representative { get; set; } = "";
        public string ServiceDetails { get; set; } = "";
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Validity { get; set; } = "";
        // Adding this to avoid null errors when calling in OnPost
        public List<Anexo1Scope> Anexo1Items { get; set; } = new();
    }

    public class Anexo1Scope { 
        public int ExternalResiduoId { get; set; }
        public string NombreResiduo { get; set; } = ""; 
        public string EstadoFisico { get; set; } = ""; 
        public string FormaAlmacenado { get; set; } = ""; 
    }

    public class Anexo2Payment { 
        public string Concept { get; set; } = ""; 
        public decimal Amount { get; set; } 
        public DateTime PaymentDate { get; set; } 
    }

    public class Anexo3Schedule { 
        public string Phase { get; set; } = ""; 
        public DateTime StartDate { get; set; } 
    }

    public class Anexo4Extra { 
        public string Description { get; set; } = ""; 
        public decimal UnitCost { get; set; } 
        public int Quantity { get; set; } 
        public decimal Total => UnitCost * Quantity; 
    }
}