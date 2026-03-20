using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Client_SimarUser.Client
{
    public class ConsultedClientModel : PageModel
    {
        public ClienteOutput Cliente { get; set; }

        public void OnGet(int id)
        {
            // Simulación (luego lo conectas a BD)
            Cliente = new ClienteOutput
            {
                Id = id,
                Email = "cliente@test.com",
                Password = "123456",
                UserName = "cliente01",
                Name = "Juan Pérez",
                Address = "Xalapa, Veracruz",
                PhoneNumber = "1234567890",
                RegisterDate = DateTime.Now,
                RFC = "XAXX010101000",
                SemarnatGeneratedNumber = "SEM-12345",
                Status = "Activo"
            };

        }
    }


}
