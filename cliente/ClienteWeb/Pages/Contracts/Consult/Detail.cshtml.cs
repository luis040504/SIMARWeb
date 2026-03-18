using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Contracts.Consult
{
    public class DetailModel : PageModel
    {
        public ContractDetail Contract { get; set; } = new();

        public void OnGet(string id)
        {
            var contracts = new List<ContractDetail>
            {
                new ContractDetail
                {
                    Id = "CON-001",
                    Client = "Empresa X",
                    ServiceDetails = "Desarrollo de software",
                    Price = 10000,
                    PaymentMethod = "Transferencia",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(12),
                    Status = "Activo"
                },
                new ContractDetail
                {
                    Id = "CON-002",
                    Client = "Comercial Y",
                    ServiceDetails = "Mantenimiento de sistemas",
                    Price = 5000,
                    PaymentMethod = "Mensual",
                    StartDate = DateTime.Today.AddMonths(-2),
                    EndDate = DateTime.Today.AddMonths(10),
                    Status = "Pendiente de firma"
                }
            };

            Contract = contracts.FirstOrDefault(c => c.Id == id) ?? new ContractDetail();
        }
    }

    public class ContractDetail
    {
        public string Id { get; set; } = "";
        public string Client { get; set; } = "";
        public string ServiceDetails { get; set; } = "";
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
    }
}