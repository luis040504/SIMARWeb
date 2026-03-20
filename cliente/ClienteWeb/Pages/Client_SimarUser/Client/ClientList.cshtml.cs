using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Client_SimarUser.Client
{
    public class ClientListModel : PageModel
    {
        public List<ClienteOutput> Clientes { get; set; }

        public void OnGet()
        {
            Clientes = new List<ClienteOutput>
        {
            new ClienteOutput { Id = 1, Name = "Juan Pérez", Email = "juan@test.com", PhoneNumber = "1234567890", RegisterDate = DateTime.Now, Status = "Activo" },
            new ClienteOutput { Id = 2, Name = "Ana López", Email = "ana@test.com", PhoneNumber = "9876543210", RegisterDate = DateTime.Now, Status = "Inactivo" }
        };
        }
    }

    public class ClienteOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string RFC { get; set; }
        public FileStream SatCertificate { get; set; }
        public string SemarnatGeneratedNumber { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

    }
}
