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