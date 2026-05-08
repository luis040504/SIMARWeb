using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ClienteWeb.Pages.Contracts.Generate
{
    public class GenerateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public GenerateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ContractsApi");
        }

        [BindProperty] public int QuotationId { get; set; }
        public List<Quotation> Quotations { get; set; } = new();

        // DATOS DEL CLIENTE Y PORTADA LEGAL
        [BindProperty] public string BusinessName { get; set; } = "";
        [BindProperty] public string RFC { get; set; } = "";
        [BindProperty] public string Address { get; set; } = "";
        [BindProperty] public string Representative { get; set; } = "";
        
        // CAMPOS MANUALES (Modificables en el panel)
        [BindProperty] public string ClientObjetoSocial { get; set; } = "";
        [BindProperty] public string ClientDeclaraciones { get; set; } = "";
        [BindProperty] public string ContractDuration { get; set; } = "";
        [BindProperty] public string FirstServiceDate { get; set; } = "";
        [BindProperty] public decimal TotalPrice { get; set; }

        // CAMPOS OCULTOS PARA LAS TABLAS (JSON)
        [BindProperty] public string ServicesJsonHidden { get; set; } = "[]";
        [BindProperty] public string PaymentsJsonHidden { get; set; } = "[]";
        [BindProperty] public string ExtrasJsonHidden { get; set; } = "[]";

        public bool ShowPreview { get; set; }

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
                    TotalPrice = quotation.Price;
                    
                    // Valores por defecto para el Panel de Configuración
                    ClientObjetoSocial = "La administración y prestación de servicios de mantenimiento...";
                    ClientDeclaraciones = "a. Es una sociedad anónima de capital variable...\nb. Su apoderado legal, cuenta con las facultades...";
                    ContractDuration = "12 (Doce) meses contados a partir de la firma";
                    FirstServiceDate = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd");

                    // Mock Inicial de Tablas
                    var servicesList = new List<ContractServiceItem> {
                        new ContractServiceItem { WasteType = quotation.ServiceDetails, WasteUnit = "Kilogramos", Frequency = "Semanal", Vehicles = 1, Technicians = 2, ServiceAddress = quotation.Address, WarehouseAddress = "Bodega Central SIMAR" }
                    };
                    var paymentsList = new List<ContractPaymentItem> {
                        new ContractPaymentItem { Description = "Pago Mensual Operativo", Amount = quotation.Price / 12, PaymentDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd") }
                    };
                    var extrasList = new List<ContractExtra>(); // Usamos la clase nueva

                    ServicesJsonHidden = JsonSerializer.Serialize(servicesList);
                    PaymentsJsonHidden = JsonSerializer.Serialize(paymentsList);
                    ExtrasJsonHidden = JsonSerializer.Serialize(extrasList);

                    ShowPreview = true;
                }
            }
        }

        public async Task<IActionResult> OnPostDownloadPdfAsync()
        {
            try
            {
                // 1. Deserializamos con las clases correctas
                var services = string.IsNullOrEmpty(ServicesJsonHidden) ? new List<ContractServiceItem>() : JsonSerializer.Deserialize<List<ContractServiceItem>>(ServicesJsonHidden);
                var payments = string.IsNullOrEmpty(PaymentsJsonHidden) ? new List<ContractPaymentItem>() : JsonSerializer.Deserialize<List<ContractPaymentItem>>(PaymentsJsonHidden);
                var extras = string.IsNullOrEmpty(ExtrasJsonHidden) ? new List<ContractExtra>() : JsonSerializer.Deserialize<List<ContractExtra>>(ExtrasJsonHidden);

                DateTime? firstService = DateTime.TryParse(FirstServiceDate, out var fsd) ? fsd : null;

                // 2. Armamos el objeto EXACTAMENTE como lo espera la nueva API
                var newContract = new 
                {
                    ClientId = QuotationId > 0 ? QuotationId : 15,
                    TotalBasePrice = TotalPrice,
                    ClientObjetoSocial = ClientObjetoSocial,
                    ClientDeclaraciones = ClientDeclaraciones,
                    ContractDuration = ContractDuration,
                    FirstServiceDate = firstService,
                    Services = services,
                    Payments = payments,
                    Extras = extras
                };

                var response = await _httpClient.PostAsJsonAsync("/api/contracts", newContract);

                if (response.IsSuccessStatusCode)
                {
                    var createdContract = await response.Content.ReadFromJsonAsync<ContractResponseDto>();
                    if (createdContract != null && createdContract.Id > 0)
                    {
                        var pdfResponse = await _httpClient.GetAsync($"/api/contracts/{createdContract.Id}/download");
                        if (pdfResponse.IsSuccessStatusCode)
                        {
                            var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
                            return File(pdfBytes, "application/pdf", $"{createdContract.Folio}.pdf");
                        }
                    }
                    return RedirectToPage("/Contracts/Consult"); 
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error de la API: {error}");
                    LoadQuotations();
                    ShowPreview = true;
                    return Page();
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Hubo un error de conexión con el servidor.");
                LoadQuotations();
                return Page();
            }
        }

        private void LoadQuotations()
        {
            Quotations = new List<Quotation> { 
                new Quotation { Id = 10, ClientName = "Comercializadora del Sur S.A.", ServiceType = "Recolección RME", DateApproved = "2026-04-20" },
                new Quotation { Id = 11, ClientName = "Hospital General Xalapa", ServiceType = "Residuos Biológico Infecciosos", DateApproved = "2026-04-25" }
            };
        }

        private QuotationDetail? GetQuotationFromDatabase(int id)
        {
            if(id == 10) return new QuotationDetail { Id = 10, BusinessName = "Comercializadora del Sur S.A. de C.V.", RFC = "CSU010203XYZ", Address = "Av. Lázaro Cárdenas 100", Representative = "JUAN PÉREZ", ServiceDetails = "Cartón y Plástico PET", Price = 120000 };
            if(id == 11) return new QuotationDetail { Id = 11, BusinessName = "Hospital General Xalapa", RFC = "HGX990101ABC", Address = "Calle Salud Sur 45", Representative = "DRA. MARÍA LÓPEZ", ServiceDetails = "Residuos RPBI", Price = 250000 };
            return null;
        }
    }

    // --- NUEVAS CLASES DE DOMINIO QUE COINCIDEN CON LA API ---
    public class ContractServiceItem { public string WasteType { get; set; } = ""; public string WasteUnit { get; set; } = ""; public string Frequency { get; set; } = ""; public int Vehicles { get; set; } public int Technicians { get; set; } public string ServiceAddress { get; set; } = ""; public string WarehouseAddress { get; set; } = ""; }
    public class ContractPaymentItem { public string Description { get; set; } = ""; public decimal Amount { get; set; } public string PaymentDate { get; set; } = ""; }
    public class ContractExtra { public string Description { get; set; } = ""; public decimal UnitCost { get; set; } public int Quantity { get; set; } public decimal Total => UnitCost * Quantity; }
    
    public class Quotation { public int Id { get; set; } public string ClientName { get; set; } = ""; public string ServiceType { get; set; } = ""; public string DateApproved { get; set; } = ""; }
    public class QuotationDetail { public int Id { get; set; } public string BusinessName { get; set; } = ""; public string RFC { get; set; } = ""; public string Address { get; set; } = ""; public string Representative { get; set; } = ""; public string ServiceDetails { get; set; } = ""; public decimal Price { get; set; } }
    
    public class ContractResponseDto { public int Id { get; set; } public string Folio { get; set; } = ""; public string Message { get; set; } = ""; }
}