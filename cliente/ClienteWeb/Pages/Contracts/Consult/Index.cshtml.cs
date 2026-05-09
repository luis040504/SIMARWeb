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

        public List<Contract> Contracts { get; set; } = new();

        [BindProperty] public string UpdateId { get; set; } = "";
        [BindProperty] public string UpdateClient { get; set; } = "";
        [BindProperty] public DateTime UpdateStartDate { get; set; }
        [BindProperty] public DateTime UpdateEndDate { get; set; }
        [BindProperty] public string UpdateStatus { get; set; } = "";
        [BindProperty] public string ServiceConditions { get; set; } = "";
        [BindProperty] public string AdminObservations { get; set; } = "";
        [BindProperty] public Microsoft.AspNetCore.Http.IFormFile? PdfFile { get; set; }

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
            if (!string.IsNullOrEmpty(Search)) url += $"search={Search}&";
            if (!string.IsNullOrEmpty(Status)) url += $"status={Status}&";
            if (DateFilter.HasValue) url += $"dateFilter={DateFilter.Value:yyyy-MM-dd}";

            try
            {
                var apiResponse = await _httpClient.GetFromJsonAsync<List<ApiContractDto>>(url);

                if (apiResponse != null)
                {
                    Contracts = apiResponse.Select(c => new Contract
                    {
                        Id = c.Folio,
                        DbId = c.Id,
                        Client = c.ClientId.ToString(),
                        StartDate = c.CreatedAt,
                        EndDate = c.ExpirationDate,
                        Status = c.Status
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Contracts = new List<Contract>();
                ErrorMessage = "No se pudo conectar con el servidor de contratos.";
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (UpdateEndDate <= UpdateStartDate)
            {
                ErrorMessage = "La nueva fecha de término debe ser posterior a la fecha de inicio.";
                await LoadContractsAsync();
                return Page();
            }

            ShowSuccessMessage = true;
            AuditTrail.Add($"Usuario: Administrador | Fecha de modificación: {DateTime.Now}");
            AuditTrail.Add($"Campos modificados: Fecha de término, Condiciones del servicio, Observaciones administrativas.");

            await LoadContractsAsync();
            return Page();
        }
    }

    // Clases auxiliares para mapear la respuesta de la API
    public class Contract
    {
        public int DbId { get; set; }
        public string Id { get; set; } = "";
        public string Client { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class ApiContractDto
    {
        public int Id { get; set; }
        public string Folio { get; set; } = "";
        public int ClientId { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}