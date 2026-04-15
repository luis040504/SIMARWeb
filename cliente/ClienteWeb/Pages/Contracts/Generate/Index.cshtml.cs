using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Contracts.Generate
{
    public class GenerateModel : PageModel
    {
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

        // --- PROPIEDADES PARA MOCKS DE ANEXOS (NUEVO) ---
        public List<Anexo1Scope> Anexo1Items { get; set; } = new();
        public List<Anexo2Payment> Anexo2Payments { get; set; } = new();
        public List<Anexo3Schedule> Anexo3Steps { get; set; } = new();
        public List<Anexo4Extra> Anexo4Extras { get; set; } = new();

        public void OnGet()
        {
            LoadQuotations();
        }

        public void OnPost(string action)
        {
            LoadQuotations();

            if (action == "preview")
            {
                var quotation = GetMockQuotation(QuotationId);
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
                    
                    // --- CARGAR DATOS INICIALES (MOCKS) ---
                    Anexo1Items = new List<Anexo1Scope> {
                        new Anexo1Scope { Residuo = "Aceite Usado", Servicio = "Recolección", Frecuencia = "Mensual" }
                    };
                    Anexo2Payments = new List<Anexo2Payment> {
                        new Anexo2Payment { Concepto = "Anticipo", Monto = 5000m, Fecha = DateTime.Now.AddDays(5) }
                    };
                    Anexo3Steps = new List<Anexo3Schedule> {
                        new Anexo3Schedule { Fase = "Instalación de Contenedores", Duracion = "1 Semana" }
                    };
                    Anexo4Extras = new List<Anexo4Extra> {
                        new Anexo4Extra { Descripcion = "Maniobras de Carga", Costo = 800m, Cantidad = 1 }
                    };

                    ShowPreview = true;
                }
            }
            else if (action == "cancel")
            {
                ShowPreview = false;
            }
        }

        private void LoadQuotations()
        {
            Quotations = new List<Quotation>
            {
                new Quotation { Id = 1, Name = "Cotización Empresa X" },
                new Quotation { Id = 2, Name = "Cotización Comercial Y" }
            };
        }

        private QuotationDetail? GetMockQuotation(int id)
        {
            var data = new List<QuotationDetail>
            {
                new QuotationDetail {
                    Id = 1, BusinessName = "Empresa X", RFC = "XAXX010101000", 
                    Address = "Av. Principal 123, Xalapa, Ver.", 
                    Representative = "JUAN PÉREZ LÓPEZ",
                    ServiceDetails = "Recolección de residuos peligrosos biológico-infecciosos", 
                    Price = 12500, PaymentMethod = "Transferencia bancaria", Validity = "12 meses"
                },
                new QuotationDetail {
                    Id = 2, BusinessName = "Comercial Y", RFC = "YAYY020202000", 
                    Address = "Calle Secundaria 456, Veracruz, Ver.", 
                    Representative = "MARÍA GARCÍA SOLÍS",
                    ServiceDetails = "Manejo de residuos industriales no peligrosos", 
                    Price = 8400, PaymentMethod = "Efectivo / Cheque", Validity = "6 meses"
                }
            };
            return data.FirstOrDefault(q => q.Id == id);
        }
    }

    // Clases del modelo
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
    }

    // --- CLASES PARA LOS ANEXOS (NUEVO) ---
    public class Anexo1Scope { public string Residuo { get; set; } = ""; public string Servicio { get; set; } = ""; public string Frecuencia { get; set; } = ""; }
    public class Anexo2Payment { public string Concepto { get; set; } = ""; public decimal Monto { get; set; } public DateTime Fecha { get; set; } }
    public class Anexo3Schedule { public string Fase { get; set; } = ""; public string Duracion { get; set; } = ""; }
    public class Anexo4Extra { public string Descripcion { get; set; } = ""; public decimal Costo { get; set; } public int Cantidad { get; set; } public decimal Total => Costo * Cantidad; }
}