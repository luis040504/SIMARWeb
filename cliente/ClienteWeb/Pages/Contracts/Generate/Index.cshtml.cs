using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ClienteWeb.Pages.Contracts.Generate
{
    public class GenerateModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public string ApiBaseUrl { get; private set; } = "";

        public GenerateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ContractsApi");
            ApiBaseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";
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
                    
                    // --- 1. LÓGICA EXACTA PARA EL PRÓXIMO MARTES ---
                    DateTime today = DateTime.Now;
                    // Formula matemática para encontrar los días faltantes hasta el martes (Día 2)
                    int daysUntilTuesday = ((int)DayOfWeek.Tuesday - (int)today.DayOfWeek + 7) % 7;
                    if (daysUntilTuesday == 0) daysUntilTuesday = 7; // Si hoy es martes, programar para la sig. semana
                    
                    DateTime nextTuesday = today.AddDays(daysUntilTuesday);
                    string firstDateStr = nextTuesday.ToString("yyyy-MM-dd");
                    // Los pagos los programamos 15 días después del primer servicio
                    string paymentDateStr = nextTuesday.AddDays(15).ToString("yyyy-MM-dd"); 
                    FirstServiceDate = firstDateStr;
                    // -----------------------------------------------

                    var servicesList = new List<ContractServiceItem>();
                    var paymentsList = new List<ContractPaymentItem>();
                    var extrasList = new List<ContractExtra>(); 

                    if (!string.IsNullOrEmpty(quote.ServicesRawJson))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(quote.ServicesRawJson);
                            foreach (var srv in doc.RootElement.EnumerateArray())
                            {
                                var loc = srv.GetProperty("location");
                                string muni = loc.GetProperty("municipality").GetString() ?? "Local";
                                string serviceAddress = $"{loc.GetProperty("street").GetString()}, {loc.GetProperty("neighborhood").GetString()}, {muni}, CP {loc.GetProperty("cp").GetString()}";
                                
                                int techs = srv.GetProperty("crew").GetArrayLength();
                                int vehicles = srv.GetProperty("vehicles").GetArrayLength();

                                // Variable para sumar el total de esta locación específica
                                decimal srvSubtotal = 0;

                                decimal srvWastesSubtotal = 0;
                                foreach (var waste in srv.GetProperty("wastes").EnumerateArray())
                                {
                                    decimal wPrice = waste.GetProperty("quantity").GetDecimal() * waste.GetProperty("pricePerUnit").GetDecimal();
                                    srvWastesSubtotal += wPrice;
                                    
                                    servicesList.Add(new ContractServiceItem 
                                    { 
                                        WasteType = waste.GetProperty("name").GetString() ?? "Residuo", 
                                        WasteUnit = waste.GetProperty("unit").GetString() ?? "kg", 
                                        Frequency = quote.Frequency,
                                        Vehicles = vehicles > 0 ? vehicles : 1, 
                                        Technicians = techs > 0 ? techs : 2, 
                                        ServiceAddress = serviceAddress, 
                                        WarehouseAddress = srv.GetProperty("logistics").GetProperty("primaryDestination").GetString() ?? "Planta de Tratamiento",
                                        Subtotal = wPrice // Asignamos el subtotal proporcional al residuo
                                    });
                                }

                                if (srvWastesSubtotal > 0) paymentsList.Add(new ContractPaymentItem { Description = $"Tratamiento y Disposición ({muni})", Amount = srvWastesSubtotal, PaymentDate = paymentDateStr });

                                // B) Logística (Vehículos + Gasolina + Casetas + Viáticos)
                                decimal logCost = 0;
                                foreach (var v in srv.GetProperty("vehicles").EnumerateArray())
                                    logCost += v.GetProperty("price").GetDecimal(); 
                                
                                var logNode = srv.GetProperty("logistics");
                                logCost += (logNode.GetProperty("fuelLiters").GetDecimal() * logNode.GetProperty("fuelPricePerLiter").GetDecimal());
                                logCost += logNode.GetProperty("totalTollCost").GetDecimal();
                                logCost += logNode.GetProperty("viaticos").GetDecimal();
                                if (logCost > 0) paymentsList.Add(new ContractPaymentItem { Description = $"Logística y Transporte ({muni})", Amount = logCost, PaymentDate = paymentDateStr });

                                // C) Cuadrilla (Mano de obra)
                                decimal crewCost = 0;
                                foreach (var c in srv.GetProperty("crew").EnumerateArray())
                                    crewCost += c.GetProperty("cost").GetDecimal();
                                if (crewCost > 0) paymentsList.Add(new ContractPaymentItem { Description = $"Mano de Obra Operativa ({muni})", Amount = crewCost, PaymentDate = paymentDateStr });

                                // D) Insumos
                                decimal supCost = 0;
                                List<string> suppliesNames = new List<string>();
                                foreach (var s in srv.GetProperty("supplies").EnumerateArray())
                                {
                                    decimal itemTotal = s.GetProperty("quantity").GetDecimal() * s.GetProperty("price").GetDecimal();
                                    supCost += itemTotal;
                                    suppliesNames.Add($"{s.GetProperty("quantity").GetDecimal()}x {s.GetProperty("name").GetString()}");
                                }
                                if (supCost > 0) {
                                    paymentsList.Add(new ContractPaymentItem { Description = $"Insumos ({muni}): {string.Join(", ", suppliesNames)}", Amount = supCost, PaymentDate = paymentDateStr });
                                }

                                foreach (var extra in srv.GetProperty("extraCosts").EnumerateArray())
                                {
                                    decimal exCost = extra.GetProperty("amount").GetDecimal();
                                    paymentsList.Add(new ContractPaymentItem { Description = $"Maniobra/Extra ({muni}): {extra.GetProperty("description").GetString()}", Amount = exCost, PaymentDate = paymentDateStr });
                                }
                            }
                        }
                        catch 
                        {
                            servicesList.Add(new ContractServiceItem { WasteType = "Error al leer cotización", WasteUnit = "N/A", Frequency = "N/A", Vehicles = 0, Technicians = 0, ServiceAddress = "N/A", WarehouseAddress = "N/A" });
                        }
                    }

                    if (!servicesList.Any())
                        servicesList.Add(new ContractServiceItem { WasteType = "Recolección general", WasteUnit = "Varios", Frequency = "Según calendario", Vehicles = 1, Technicians = 2, ServiceAddress = "A definir", WarehouseAddress = "Bodega Central SIMAR" });

                    ServicesJsonHidden = JsonSerializer.Serialize(servicesList);
                    PaymentsJsonHidden = JsonSerializer.Serialize(paymentsList);
                    ExtrasJsonHidden = JsonSerializer.Serialize(extrasList); 

                    ShowPreview = true;
                }
            }
            else if (action == "cancel")
            {
                ShowPreview = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveOnlyAsync()
        {
            ModelState.Clear();

            var savedContractId = await SaveContractToApiAsync();
            if (savedContractId > 0)
            {
                return Redirect("/Contracts/Consult");
            }
            return Page(); 
        }

        public async Task<IActionResult> OnPostDownloadPdfAsync()
        {
            ModelState.Clear();

            var savedContractId = await SaveContractToApiAsync();
            if (savedContractId > 0)
            {
                return new JsonResult(new { success = true, contractId = savedContractId });
            }
            
            return new JsonResult(new { success = false, error = "Error al guardar el contrato." });
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
                    ClientName = BusinessName,
                    ClientRfc = RFC,
                    Representative = Representative,
                    ClientAddress = Address,
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
    public class MirroredQuotationDto { public int Id { get; set; } public string ClientName { get; set; } = ""; public string ClientRfc { get; set; } = ""; public string ContactName { get; set; } = ""; public decimal Subtotal { get; set; } public decimal Total { get; set; } public int ValidityDays { get; set; } public string ServicesRawJson { get; set; } = "[]"; public string Frequency { get; set; } = ""; }    
    public class ContractServiceItem { public string WasteType { get; set; } = ""; public string WasteUnit { get; set; } = ""; public string Frequency { get; set; } = ""; public int Vehicles { get; set; } public int Technicians { get; set; } public string ServiceAddress { get; set; } = ""; public string WarehouseAddress { get; set; } = ""; public decimal Subtotal { get; set; } }
    public class ContractPaymentItem { public string Description { get; set; } = ""; public decimal Amount { get; set; } public string PaymentDate { get; set; } = ""; }
    public class ContractExtra { public string Description { get; set; } = ""; public decimal UnitCost { get; set; } public int Quantity { get; set; } public decimal Total => UnitCost * Quantity; }
    public class ContractResponseDto { public int Id { get; set; } public string Folio { get; set; } = ""; public string Message { get; set; } = ""; }
}