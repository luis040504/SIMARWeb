using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ClienteWeb.Pages.Contracts.Consult
{
    public class ConsultModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public string ApiBaseUrl { get; private set; } = "";

        public ConsultModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ContractsApi");
            ApiBaseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";
        }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; }

        public List<ConsultContract> Contracts { get; set; } = new();

        [BindProperty] public int UpdateDbId { get; set; }
        [BindProperty] public string UpdateId { get; set; } = "";
        [BindProperty] public string UpdateClient { get; set; } = "";
        [BindProperty] public DateTime UpdateStartDate { get; set; }
        [BindProperty] public string AdminObservations { get; set; } = "";
        [BindProperty] public DateTime? UpdateEndDate { get; set; }
        [BindProperty] public Microsoft.AspNetCore.Http.IFormFile? PdfFile { get; set; }
        [BindProperty] public string ServicesJson { get; set; } = "[]";
        [BindProperty] public string PaymentsJson { get; set; } = "[]";
        [BindProperty] public string ExtrasJson { get; set; } = "[]";

        public bool ShowSuccessMessage { get; set; }
        public string ErrorMessage { get; set; } = "";
        public List<string> AuditTrail { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadContractsAsync();
        }

        private async Task LoadContractsAsync()
        {
            var url = "/api/contracts?";
            
            if (!string.IsNullOrEmpty(Search)) 
                url += $"search={Uri.EscapeDataString(Search)}&";
                
            if (!string.IsNullOrEmpty(Status)) 
                url += $"status={Uri.EscapeDataString(Status)}&";
                
            if (DateFilter.HasValue) 
                url += $"dateFilter={DateFilter.Value:yyyy-MM-dd}";

            try
            {
                var apiResponse = await _httpClient.GetFromJsonAsync<List<ApiContractDto>>(url);

                if (apiResponse != null)
                {
                    Contracts = apiResponse.Select(c => new ConsultContract
                    {
                        Id = c.Folio,
                        DbId = c.Id,
                        Client = !string.IsNullOrEmpty(c.ClientName) ? c.ClientName : "Cliente Desconocido", 
                        StartDate = c.CreatedAt,
                        EndDate = c.ExpirationDate,
                        Status = c.Status,
                        SignedContractPath = c.SignedContractPath
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Contracts = new List<ConsultContract>();
                // Descomenta esto si quieres ver el error real temporalmente
                // ErrorMessage = $"Error técnico: {ex.Message}"; 
                ErrorMessage = "No se pudo conectar con el servidor de contratos.";
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            try
            {
                if (UpdateEndDate.HasValue && UpdateEndDate.Value <= UpdateStartDate)
                {
                    ErrorMessage = "La nueva fecha de término debe ser posterior a la fecha de inicio.";
                    await LoadContractsAsync();
                    return Page();
                }

                var detailUrl = $"/api/contracts/{UpdateDbId}/detail";
                var current = await _httpClient.GetFromJsonAsync<JsonElement>(detailUrl);
                
                var services = JsonSerializer.Deserialize<List<object>>(ServicesJson) ?? new();
                var payments = JsonSerializer.Deserialize<List<object>>(PaymentsJson) ?? new();
                var extras = JsonSerializer.Deserialize<List<object>>(ExtrasJson) ?? new();

                var updateData = new {
                    id = UpdateDbId,
                    folio = current.GetProperty("folio").GetString(),
                    clientId = current.GetProperty("clientId").GetInt32(),
                    totalBasePrice = current.GetProperty("totalBasePrice").GetDecimal(),
                    clientName = UpdateClient,
                    clientRfc = current.GetProperty("clientRfc").GetString(),
                    representative = current.GetProperty("representative").GetString(),
                    clientAddress = current.GetProperty("clientAddress").GetString(),
                    clientObjetoSocial = current.GetProperty("clientObjetoSocial").GetString(),
                    clientDeclaraciones = current.GetProperty("clientDeclaraciones").GetString(),
                    contractDuration = current.GetProperty("contractDuration").GetString(),
                    firstServiceDate = UpdateStartDate,
                    endDate = UpdateEndDate,
                    status = current.GetProperty("status").GetString(),
                    services = services,
                    payments = payments,
                    extras = extras
                };

                var putResponse = await _httpClient.PutAsJsonAsync($"/api/contracts/{UpdateDbId}", updateData);
                if (!putResponse.IsSuccessStatusCode) 
                {
                    var errorContent = await putResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Error del servidor (PUT): {putResponse.StatusCode} - {errorContent}");
                }

                if (PdfFile != null && PdfFile.Length > 0)
                {
                    using var content = new MultipartFormDataContent();
                    using var fileStream = PdfFile.OpenReadStream();
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    content.Add(fileContent, "file", PdfFile.FileName);
                    
                    var pdfResponse = await _httpClient.PostAsync($"/api/contracts/{UpdateDbId}/upload-pdf", content);
                    if (!pdfResponse.IsSuccessStatusCode)
                    {
                        var pdfError = await pdfResponse.Content.ReadAsStringAsync();
                        throw new Exception($"Error al subir PDF: {pdfResponse.StatusCode} - {pdfError}");
                    }
                }

                ShowSuccessMessage = true;
                AuditTrail.Add($"Usuario: Administrador | Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
                AuditTrail.Add($"Cambios persistidos correctamente.");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error crítico al guardar: " + ex.Message;
                Console.WriteLine($"[ERROR UPDATE] {ex}");
            }

            await LoadContractsAsync();
            return Page();
        }
    }

    // Clases auxiliares para mapear la respuesta de la API
    public class ConsultContract
    {
        public int DbId { get; set; }
        public string Id { get; set; } = "";
        public string Client { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
        public string? SignedContractPath { get; set; }
    }

    public class ApiContractDto
    {
        public int Id { get; set; }
        public string Folio { get; set; } = "";
        public int ClientId { get; set; }
        
        public string ClientName { get; set; } = ""; 
        
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? SignedContractPath { get; set; }
    }
}