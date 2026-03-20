using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Client_SimarUser.Client
{
    public class ClientRegisterModel : PageModel
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

            // --- PASO 2: Info General ---
            public string Name { get; set; }
            public string Address { get; set; }
            public string PhoneNumber { get; set; }
            public DateOnly RegisterDate { get; set; }
            public string RFC { get; set; }
            public string SemarnatGeneratedNumber { get; set; }


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

            TempData["Mensaje"] = "Cliente registrado correctamente";


            // AGREGAR esto a la pantalla anterior a esta para que se vea el mensaje de arriba luego de guardar
            /*
             * 
             * @if (TempData["Mensaje"] != null)
                    {
                        <div class="alerta-exito">
                            @TempData["Mensaje"]
                        </div>
                    }
             * 
             */

            // Por ahora, solo redireccionamos al Index tras "finalizar"
            return RedirectToPage("/Index");
        }
    }
}
