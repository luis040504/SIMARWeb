using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Contracts.Consult
{
    public class ConsultModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        public List<Contract> Contracts { get; set; } = new();

        public void OnGet()
        {
            LoadContracts();
        }

        private void LoadContracts()
        {
            var allContracts = new List<Contract>
            {
                new Contract
                {
                    Id = "CON-001",
                    Client = "Empresa X",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(12),
                    Status = "Activo"
                },
                new Contract
                {
                    Id = "CON-002",
                    Client = "Comercial Y",
                    StartDate = DateTime.Today.AddMonths(-2),
                    EndDate = DateTime.Today.AddMonths(10),
                    Status = "Pendiente de firma"
                },
                new Contract
                {
                    Id = "CON-003",
                    Client = "Industrias Z",
                    StartDate = DateTime.Today.AddYears(-1),
                    EndDate = DateTime.Today.AddMonths(-1),
                    Status = "Vencido"
                }
            };

            if (!string.IsNullOrEmpty(Search))
            {
                allContracts = allContracts
                    .Where(c => c.Id.Contains(Search, StringComparison.OrdinalIgnoreCase)
                             || c.Client.Contains(Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(Status))
            {
                allContracts = allContracts
                    .Where(c => c.Status == Status)
                    .ToList();
            }

            Contracts = allContracts;
        }

        [BindProperty]
        public string UpdateId { get; set; } = "";

        [BindProperty]
        public string UpdateClient { get; set; } = "";

        [BindProperty]
        public DateTime UpdateStartDate { get; set; }

        [BindProperty]
        public DateTime UpdateEndDate { get; set; }

        [BindProperty]
        public string UpdateStatus { get; set; } = "";

        [BindProperty]
        public string ServiceConditions { get; set; } = "";

        [BindProperty]
        public string AdminObservations { get; set; } = "";

        [BindProperty]
        public Microsoft.AspNetCore.Http.IFormFile? PdfFile { get; set; }

        public bool ShowSuccessMessage { get; set; }
        public string ErrorMessage { get; set; } = "";
        public List<string> AuditTrail { get; set; } = new();

        public IActionResult OnPostUpdate()
        {
            LoadContracts();

            if (UpdateEndDate <= UpdateStartDate)
            {
                ErrorMessage = "La nueva fecha de término debe ser posterior a la fecha de inicio.";
                return Page();
            }

            ShowSuccessMessage = true;

            AuditTrail.Add($"Usuario: Administrador | Fecha de modificación: {DateTime.Now}");
            AuditTrail.Add($"Campos modificados: Fecha de término, Condiciones del servicio, Observaciones administrativas.");

            if (PdfFile != null)
            {
                var extension = System.IO.Path.GetExtension(PdfFile.FileName).ToLower();
                if (extension == ".pdf")
                {
                    AuditTrail.Add($"Archivo adjunto validado: {PdfFile.FileName} (Tamaño: {PdfFile.Length / 1024} KB).");
                }
                else
                {
                    ErrorMessage = "El archivo adjunto debe ser un PDF válido.";
                    ShowSuccessMessage = false;
                    AuditTrail.Clear();
                    return Page();
                }
            }

            return Page();
        }
    }

    public class Contract
    {
        public string Id { get; set; } = "";
        public string Client { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
    }
}