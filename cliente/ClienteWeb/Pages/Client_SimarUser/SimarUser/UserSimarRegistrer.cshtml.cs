using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    [IgnoreAntiforgeryToken]
    public class UserSimarRegistrerModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public UserSimarRegistrerModel(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("UserApi");
        }

        [BindProperty]
        public RegistroInput Input { get; set; }

        public class RegistroInput
        {
            [Required(ErrorMessage = "El correo es obligatorio")]
            [EmailAddress(ErrorMessage = "Formato de correo inválido")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "El usuario es obligatorio")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "El nombre es obligatorio")]
            public string NombreCompleto { get; set; } = string.Empty;

            public string? Genre { get; set; }

            [Required(ErrorMessage = "La CURP es obligatoria")]
            [StringLength(18, MinimumLength = 18, ErrorMessage = "La CURP debe tener 18 caracteres")]
            public string Curp { get; set; } = string.Empty;

            [Required(ErrorMessage = "El RFC es obligatorio")]
            [StringLength(13, MinimumLength = 12, ErrorMessage = "RFC inválido (12 o 13 caracteres)")]
            public string Rfc { get; set; } = string.Empty;

            public string? Direccion { get; set; }

            public string? Phone { get; set; }

            [Required(ErrorMessage = "El salario es obligatorio")]
            public decimal Salario { get; set; }

            [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
            [DataType(DataType.Date)]
            [MayorDeEdad]
            public DateTime? Birthday { get; set; }

            [Required(ErrorMessage = "Debe seleccionar un rol")]
            public string RolSeleccionado { get; set; } = string.Empty;

            public string? LicenseNumber { get; set; }
            public string? LicenseType { get; set; }
        }

        public void OnGet()
        {
            Input = new RegistroInput();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine($"ERROR EN: {modelStateKey} -> {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            var registroData = new
            {
                username = Input.UserName,
                email = Input.Email,
                password = Input.Password,
                rolSeleccionado = Input.RolSeleccionado.ToLower(),
                nombreCompleto = Input.NombreCompleto,
                direccion = Input.Direccion,
                birthday = Input.Birthday,
                curp = Input.Curp,
                rfc = Input.Rfc,
                phone = Input.Phone,
                genre = Input.Genre,
                salario = Input.Salario,
                licenseNumber = Input.LicenseNumber,
                licenseType = Input.LicenseType
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/usuarios/registro-completo", registroData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Empleado registrado con éxito.";
                    return RedirectToPage("/Index");
                }

                // === Validacion Duplicados ===
                var errorContent = await response.Content.ReadFromJsonAsync<ErrorApiDto>();

                string mensajeError = errorContent?.mensaje ?? "El CURP, RFC, Usuario o Correo ya existen en el sistema.";

                ModelState.AddModelError(string.Empty, mensajeError);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión con el servidor: {ex.Message}");
                return Page();
            }
        }
    }

    public class ErrorApiDto
    {
        public string mensaje { get; set; } = string.Empty;
    }

    public class MayorDeEdadAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime fechaNacimiento)
            {
                var fechaMinima = DateTime.Today.AddYears(-18);

                if (fechaNacimiento > fechaMinima)
                {
                    return new ValidationResult("El empleado debe tener al menos 18 años de edad.");
                }
            }
            return ValidationResult.Success;
        }
    }
} 
