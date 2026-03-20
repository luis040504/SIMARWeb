using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Client_SimarUser.Client
{
    public class EditClientModel : PageModel
    {
        [BindProperty]
        public ClienteOutput Cliente { get; set; }

        [BindProperty]
        public IFormFile CertificadoSAT { get; set; }

        public void OnGet(int id)
        {
            // Simulación (luego BD)
            Cliente = new ClienteOutput
            {
                Id = id,
                UserName = "cliente01",
                Email = "cliente@test.com",
                Password = "123456",
                Name = "Juan Pérez",
                Address = "Xalapa, Veracruz",
                PhoneNumber = "1234567890",
                RFC = "XAXX010101000",
                SemarnatGeneratedNumber = "SEM-12345"
            };
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            

            // Aquí guardarías cambios en BD

            if (CertificadoSAT != null)
            {
                // Aquí procesas archivo (más adelante)
            }

            TempData["Mensaje"] = "Cliente actualizado correctamente";

            return RedirectToPage("/Client&SimarUser/Client/ClientList");
        }

    }

}
