using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    public class EditSimarUserModel : PageModel
    {
        private readonly HttpClient _userClient;
        private readonly HttpClient _employeeClient;

        public EditSimarUserModel(IHttpClientFactory factory)
        {
            _userClient = factory.CreateClient("UserApi");
            _employeeClient = factory.CreateClient("EmpleadoApi");
        }

        [BindProperty]
        public SimarUserViewModel DatosUsuario { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var usuario = await _userClient.GetFromJsonAsync<UsuarioApiDTO>($"/api/usuarios/{id}");
                if (usuario == null) return Redirect("/dashboard");

                var employeeData = await _employeeClient.GetFromJsonAsync<EmpleadoDetailDTO>($"/api/employees/{id}");
                if (employeeData == null || employeeData.BaseInfo == null) return Redirect("/dashboard");

                var emp = employeeData.BaseInfo;
                var driver = employeeData.DriverInfo;

                string rolFormateado = "Sin Asignar";
                if (!string.IsNullOrWhiteSpace(usuario.Role))
                {
                    rolFormateado = char.ToUpper(usuario.Role[0]) + usuario.Role.Substring(1).ToLower();
                }

                DatosUsuario = new SimarUserViewModel
                {
                    UserName = usuario.Username ?? "",
                    Email = usuario.Email ?? "",
                    RolSeleccionado = rolFormateado, 
                    NombreCompleto = emp.FullName ?? "",
                    Genero = emp.Genre ?? "Otro",
                    Curp = emp.Curp ?? "",
                    Rfc = emp.Rfc ?? "",
                    Direccion = emp.Address ?? "",
                    Salario = emp.Salary,
                    NumLicencia = driver?.LicenseNumber ?? ""
                };

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el expediente: {ex.Message}");
                return Redirect("/dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var usuarioOriginal = await _userClient.GetFromJsonAsync<UsuarioApiDTO>($"/api/usuarios/{id}");
                if (usuarioOriginal == null) return Redirect("/dashboard");

                var userUpdateDto = new
                {
                    username = usuarioOriginal.Username,
                    email = DatosUsuario.Email,
                    role = usuarioOriginal.Role
                };

                var userResponse = await _userClient.PutAsJsonAsync($"/api/usuarios/{id}", userUpdateDto);
                userResponse.EnsureSuccessStatusCode();

                string rolOriginalSeguro = usuarioOriginal.Role ?? "";
                bool esChofer = rolOriginalSeguro.ToLower() == "driver" || rolOriginalSeguro.ToLower() == "chofer";

                var employeeUpdateDto = new
                {
                    fullName = DatosUsuario.NombreCompleto,
                    address = DatosUsuario.Direccion,
                    genre = DatosUsuario.Genero,
                    salary = DatosUsuario.Salario,
                    licenseNumber = esChofer ? DatosUsuario.NumLicencia : null
                };

                var empResponse = await _employeeClient.PutAsJsonAsync($"/api/employees/{id}", employeeUpdateDto);
                empResponse.EnsureSuccessStatusCode();

                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al contactar los servidores. Intente más tarde.");
                return Page();
            }
        }

        public class SimarUserViewModel
        {
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingrese un formato de correo válido (ej. usuario@simar.com).")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "El nombre completo es obligatorio.")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
            public string NombreCompleto { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe seleccionar un género.")]
            public string Genero { get; set; } = string.Empty;

            public string Curp { get; set; } = string.Empty;

            public string Rfc { get; set; } = string.Empty;

            [Required(ErrorMessage = "El domicilio es obligatorio.")]
            [StringLength(200, ErrorMessage = "El domicilio no puede exceder los 200 caracteres.")]
            public string Direccion { get; set; } = string.Empty;

            public string RolSeleccionado { get; set; } = string.Empty;

            [Required(ErrorMessage = "El salario es obligatorio.")]
            [Range(0.01, 1000000, ErrorMessage = "Debe ingresar un salario mayor a 0.")]
            public decimal Salario { get; set; }

            public string? NumLicencia { get; set; }
        }

        public class UsuarioApiDTO
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public class EmpleadoDetailDTO
        {
            public BaseInfoDTO BaseInfo { get; set; } = new();
            public DriverInfoDTO? DriverInfo { get; set; }
        }

        public class BaseInfoDTO
        {
            public string FullName { get; set; } = string.Empty;
            public string? Curp { get; set; }
            public string? Rfc { get; set; }
            public string? Address { get; set; }
            public string? Genre { get; set; }
            public decimal Salary { get; set; }
        }

        public class DriverInfoDTO
        {
            public string? LicenseNumber { get; set; }
        }
    }
}