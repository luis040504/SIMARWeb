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
        public List<QuotationListItem> Quotations { get; set; } = new();

        [BindProperty] public string BusinessName { get; set; } = "";
        [BindProperty] public string RFC { get; set; } = "";
        [BindProperty] public string Address { get; set; } = "";
        [BindProperty] public string Representative { get; set; } = "";
        
        [BindProperty] public string ClientObjetoSocial { get; set; } = "";
        [BindProperty] public string ClientDeclaraciones { get; set; } = "";
        [BindProperty] public string ContractDuration { get; set; } = "";
        [BindProperty] public string FirstServiceDate { get; set; } = "";
        [BindProperty] public decimal TotalPrice { get; set; }

        [BindProperty] public string ServicesJsonHidden { get; set; } = "[]";
        [BindProperty] public string PaymentsJsonHidden { get; set; } = "[]";
        [BindProperty] public string ExtrasJsonHidden { get; set; } = "[]";

        public bool ShowPreview { get; set; }

        public async Task OnGetAsync()
        {
            await LoadQuotationsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            // LIMPIAMOS LOS ERRORES DE VALIDACIÓN INVISIBLES
            ModelState.Clear(); 
            
            await LoadQuotationsAsync();

            if (action == "preview")
            {
                var quote = await GetQuotationFromApiAsync(QuotationId); 
                if (quote != null)
                {
                    BusinessName = quote.ClientName;
                    RFC = quote.ClientRfc;
                    Address = "Pendiente de captura (Llenar manualmente)"; 
                    Representative = quote.ContactName;
                    TotalPrice = quote.Total;
                    
                    ClientObjetoSocial = "La administración y prestación de servicios de su sector industrial...";
                    ClientDeclaraciones = "a. Es una sociedad legalmente constituida...\nb. Su apoderado legal...";
                    ContractDuration = $"{quote.ValidityDays} días";
                    FirstServiceDate = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd");

                    var servicesList = new List<ContractServiceItem> {
                        new ContractServiceItem { WasteType = "Recolección según cotización", WasteUnit = "Varios", Frequency = "Según calendario", Vehicles = 1, Technicians = 2, ServiceAddress = "A definir", WarehouseAddress = "Bodega Central SIMAR" }
                    };
                    var paymentsList = new List<ContractPaymentItem> {
                        new ContractPaymentItem { Description = "Pago del Servicio", Amount = quote.Total, PaymentDate = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd") }
                    };

                    ServicesJsonHidden = JsonSerializer.Serialize(servicesList);
                    PaymentsJsonHidden = JsonSerializer.Serialize(paymentsList);
                    ExtrasJsonHidden = "[]";

                    ShowPreview = true;
                }
            }
            else if (action == "cancel")
            {
                ShowPreview = false;
            }

            return Page();
        }

        // SOLO GUARDAR
        // BOTÓN 1: Solo Guardar en BD
        public async Task<IActionResult> OnPostSaveOnlyAsync()
        {
            ModelState.Clear(); // <--- Limpiamos errores fantasma

            var savedContractId = await SaveContractToApiAsync();
            if (savedContractId > 0)
            {
                return Redirect("/Contracts/Consult");
            }
            return Page(); 
        }

        // BOTÓN 2: Guardar y Descargar PDF desde el backend
        public async Task<IActionResult> OnPostDownloadPdfAsync()
        {
            ModelState.Clear();

            var savedContractId = await SaveContractToApiAsync();
            if (savedContractId > 0)
            {
                try
                {
                    // Descargar el PDF directamente desde el endpoint del backend
                    var pdfResponse = await _httpClient.GetAsync($"/api/contracts/{savedContractId}/download");
                    if (pdfResponse.IsSuccessStatusCode)
                    {
                        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
                        var fileName = pdfResponse.Content.Headers.ContentDisposition?.FileName?.Trim('"') 
                                       ?? $"Contrato_{savedContractId}.pdf";
                        
                        return File(pdfBytes, "application/pdf", fileName);
                    }
                }
                catch (Exception)
                {
                    // Si falla la descarga del PDF, al menos el contrato ya se guardó
                }

                // Si no se pudo descargar el PDF, al menos redirigir a consulta
                return Redirect("/Contracts/Consult");
            }
            
            // Si falló el guardado, mostrar errores en la misma página
            ShowPreview = true;
            await LoadQuotationsAsync();
            return Page(); 
        }

        private async Task<int> SaveContractToApiAsync()
        {
            try
            {
                var services = string.IsNullOrEmpty(ServicesJsonHidden) ? new List<ContractServiceItem>() : JsonSerializer.Deserialize<List<ContractServiceItem>>(ServicesJsonHidden);
                var payments = string.IsNullOrEmpty(PaymentsJsonHidden) ? new List<ContractPaymentItem>() : JsonSerializer.Deserialize<List<ContractPaymentItem>>(PaymentsJsonHidden);
                var extras = string.IsNullOrEmpty(ExtrasJsonHidden) ? new List<ContractExtra>() : JsonSerializer.Deserialize<List<ContractExtra>>(ExtrasJsonHidden);

                DateTime? firstService = DateTime.TryParse(FirstServiceDate, out var fsd) ? fsd : null;

                var newContract = new 
                {
                    ClientId = QuotationId,
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
                    return createdContract?.Id ?? 0;
                }
                else
                {
                    var errorStr = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Rechazado por el servidor: {errorStr}");
                    await LoadQuotationsAsync();
                    ShowPreview = true;
                    return 0;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error de red: No se pudo conectar con el servidor en Docker.");
                await LoadQuotationsAsync();
                ShowPreview = true;
                return 0;
            }
        }

        private async Task LoadQuotationsAsync()
        {
            try 
            {
                var apiQuotes = await _httpClient.GetFromJsonAsync<List<QuotationListItem>>("/api/quotations");
                Quotations = apiQuotes ?? new List<QuotationListItem>();
            } 
            catch 
            {
                ModelState.AddModelError(string.Empty, "No se pudieron cargar las cotizaciones. Verifique la conexión con el servidor.");
                Quotations = new List<QuotationListItem>();
            }
        }

        private async Task<MirroredQuotationDto?> GetQuotationFromApiAsync(int id)
        {
            try 
            {
                return await _httpClient.GetFromJsonAsync<MirroredQuotationDto>($"/api/quotations/{id}");
            } 
            catch
            {
                ModelState.AddModelError(string.Empty, "No se pudo obtener la información de la cotización seleccionada.");
                return null; 
            }
        }
    }

    public class QuotationListItem { public int Id { get; set; } public string ClientName { get; set; } = ""; public string ServiceType { get; set; } = ""; public string DateApproved { get; set; } = ""; }
    public class MirroredQuotationDto { public int Id { get; set; } public string ClientName { get; set; } = ""; public string ClientRfc { get; set; } = ""; public string ContactName { get; set; } = ""; public decimal Total { get; set; } public int ValidityDays { get; set; } }
    public class ContractServiceItem { public string WasteType { get; set; } = ""; public string WasteUnit { get; set; } = ""; public string Frequency { get; set; } = ""; public int Vehicles { get; set; } public int Technicians { get; set; } public string ServiceAddress { get; set; } = ""; public string WarehouseAddress { get; set; } = ""; }
    public class ContractPaymentItem { public string Description { get; set; } = ""; public decimal Amount { get; set; } public string PaymentDate { get; set; } = ""; }
    public class ContractExtra { public string Description { get; set; } = ""; public decimal UnitCost { get; set; } public int Quantity { get; set; } public decimal Total => UnitCost * Quantity; }
    public class ContractResponseDto { public int Id { get; set; } public string Folio { get; set; } = ""; public string Message { get; set; } = ""; }
}