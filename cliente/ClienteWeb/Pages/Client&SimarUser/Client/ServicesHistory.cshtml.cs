using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Client_SimarUser.Client
{
    public class ServicesHistoryModel : PageModel
    {
        public List<Servicio> Servicios { get; set; }
        public int ClientId { get; set; }

        public void OnGet(int id)
        {
            ClientId = id;

            // Simulación (luego BD)
            Servicios = new List<Servicio>
        {
            new Servicio {
                ServiceDate = DateTime.Now.AddDays(-10),
                Address = "Xalapa, Veracruz",
                Technical = "Carlos López",
                StateOfService = "Completado"
            },
            new Servicio {
                ServiceDate = DateTime.Now.AddDays(-5),
                Address = "Coatepec, Veracruz",
                Technical = "Ana Martínez",
                StateOfService = "Pendiente"
            }
        };
        }
    }

    public class Servicio
    {
        public DateTime ServiceDate { get; set; }
        public string Address { get; set; }
        public string Technical { get; set; }
        public string StateOfService { get; set; }
    }
}
