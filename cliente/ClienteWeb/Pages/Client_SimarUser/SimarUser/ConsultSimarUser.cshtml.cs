using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    public class ConsultSimarUserModel : PageModel
    {
        private readonly HttpClient _userClient;
        private readonly HttpClient _employeeClient;

        public ConsultSimarUserModel(IHttpClientFactory factory)
        {
            _userClient = factory.CreateClient("UserApi");
            _employeeClient = factory.CreateClient("EmpleadoApi");
        }

        public List<EmpleadoViewModel> EmpleadosCompletos { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var usuarios = await _userClient.GetFromJsonAsync<List<UsuarioApiDTO>>("/api/usuarios") ?? new List<UsuarioApiDTO>();

                var empleados = await _employeeClient.GetFromJsonAsync<List<EmpleadoApiDTO>>("/api/employees") ?? new List<EmpleadoApiDTO>();

                EmpleadosCompletos = empleados
                    .Where(e => e.State != 0) 
                    .Select(emp =>
                    {
                        var userMatch = usuarios.FirstOrDefault(u => u.Id_User == emp.UserId);

                        return new EmpleadoViewModel
                        {
                            UserId = emp.UserId,
                            FullName = emp.FullName,
                            Curp = emp.Curp ?? "N/A", 
                            Rfc = emp.Rfc ?? "N/A",
                            Phone = emp.Phone,
                            Address = emp.Address,
                            Genre = emp.Genre,
                            Salary = emp.Salary,
                            Birthday = emp.Birthday,
                            RoleName = emp.Role?.RoleName ?? "Sin Asignar",

                            Username = userMatch?.Username ?? "N/A",
                            Email = userMatch?.Email ?? "Sin Correo Registrado"
                        };
                    }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-----> Error de conexión: {ex.Message}");
            }
        }

        public class UsuarioApiDTO
        {
            public Guid Id_User { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class EmpleadoApiDTO
        {
            public Guid UserId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string? Curp { get; set; }
            public string? Rfc { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? Genre { get; set; }
            public decimal Salary { get; set; }
            public int State { get; set; }
            public DateTime? Birthday { get; set; }
            public RoleDTO? Role { get; set; }
        }

        public class RoleDTO { public string RoleName { get; set; } = string.Empty; }

        public class EmpleadoViewModel
        {
            public Guid UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Curp { get; set; } = string.Empty;
            public string Rfc { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? Genre { get; set; }
            public decimal Salary { get; set; }
            public DateTime? Birthday { get; set; }
            public string RoleName { get; set; } = string.Empty;
        }
    }
}