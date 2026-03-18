using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    public class UserSimarRegistrerModel : PageModel
    {
        // El atributo [BindProperty] permite que los datos del formulario 
        // se conecten automáticamente con este objeto al hacer POST.
        [BindProperty]
        public RegistroInput Input { get; set; }

        public class RegistroInput
        {
            // --- PASO 1: Credenciales ---
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            // --- PASO 2: Info General y Rol ---
            public string NombreCompleto { get; set; }

            [Required]
            public string RolSeleccionado { get; set; }

            // --- PASO 3: Campos específicos (Opcionales dependiendo del Rol) ---
            public string LicenciaConducir { get; set; } // Para Chofer
            public string Especialidad { get; set; }    // Para Tecnico
            public string AreaVentas { get; set; }      // Para Vendedor

            public string ZonaVentas { get; set; }
        }

        public void OnGet()
        {
            Input = new RegistroInput();
        }

        public IActionResult OnPost()
        {
            // Validamos que los campos obligatorios estén llenos
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // AQUÍ IRÍA LA LÓGICA DE GUARDADO
            // Por ejemplo:
            // if (Input.RolSeleccionado == "Chofer") { ... guardar en tabla choferes ... }

            // Por ahora, solo redireccionamos al Index tras "finalizar"
            return RedirectToPage("/Index");
        }
    }
}
