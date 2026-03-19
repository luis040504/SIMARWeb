using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    public class RegistroInput
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string NombreCompleto { get; set; }
        public string Curp { get; set; }
        public string Rfc { get; set; }
        public string Direccion { get; set; }
        public float Salario { get; set; }
        public string FechaNac { get; set; }
        public string Genero { get; set; }
        public string RolSeleccionado { get; set; }
        public string ProfessionalID { get; set; }
        public string NumLicencia { get; set; }
        public string TipoLicencia { get; set; }
    }

    public class EditSimarUserModel : PageModel
    {
        [BindProperty]
        public RegistroInput DatosUsuario { get; set; } 

        public void OnGet(string id)
        {
            // Datos de prueba (borrarlos despues)
            DatosUsuario = new RegistroInput
            {
                UserName = id,
                Email = "usuario@simar.com",
                NombreCompleto = "Luis Enrique López",
                Curp = "LOAL880505HDFRR01",
                Rfc = "LOAL880505XXX",
                Direccion = "3ra de Miguel Lerdo",
                Salario = 18000,
                FechaNac = "1988-05-05",
                Genero = "Masculino",
                RolSeleccionado = "Driver",
                NumLicencia = "LIC-VER-9922",
                TipoLicencia = "E"
            };
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            return RedirectToPage("/consultar");
        }
    }
}