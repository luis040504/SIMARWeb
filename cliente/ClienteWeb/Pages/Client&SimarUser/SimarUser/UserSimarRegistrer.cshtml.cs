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
            [Required(ErrorMessage = "El nombre es obligatorio")]
            public string NombreCompleto { get; set; }

            public string Genero { get; set; }

            [StringLength(18, MinimumLength = 18, ErrorMessage = "La CURP debe tener 18 caracteres")]
            public string Curp { get; set; }

            [StringLength(13, ErrorMessage = "El RFC no puede exceder los 13 caracteres")]
            public string Rfc { get; set; }

            public string Direccion { get; set; }

            public float Salario { get; set; } // Nota: Cambié a Mayúscula 'S' por convención de C#

            [DataType(DataType.Date)]
            public string FechaNac { get; set; }

            [Required]
            public string RolSeleccionado { get; set; }

            // --- PASO 3: Campos específicos (Opcionales dependiendo del Rol) ---

            // ROL: Seller, Technical, Manager, Owner, Accountant
            public string ProfessionalID { get; set; }

            // ROL: Driver
            public string NumLicencia { get; set; } 
            public string TipoLicencia { get; set; }  

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
