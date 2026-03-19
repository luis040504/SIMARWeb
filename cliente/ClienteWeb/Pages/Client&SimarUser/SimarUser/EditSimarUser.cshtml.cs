using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    // Clase de datos (Fuera de la clase principal para evitar ambigüedad)
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
        public RegistroInput DatosUsuario { get; set; } // Usamos "DatosUsuario" en lugar de "Input"

        public void OnGet(string id)
        {
            // Simulamos que cargamos los datos desde una base de datos usando el ID
            DatosUsuario = new RegistroInput
            {
                UserName = id,
                Email = "usuario@simar.com",
                NombreCompleto = "Luis Enrique López",
                Curp = "LOAL880505HDFRR01",
                Rfc = "LOAL880505XXX",
                Direccion = "Av. de los Catedráticos #102",
                Salario = 18000,
                FechaNac = "1988-05-05",
                Genero = "Masculino",
                RolSeleccionado = "Driver",
                NumLicencia = "LIC-VER-9922",
                TipoLicencia = "B"
            };
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            // Aquí procesarías la actualización en la DB
            // return _service.Update(DatosUsuario);

            return RedirectToPage("/consultar");
        }
    }
}