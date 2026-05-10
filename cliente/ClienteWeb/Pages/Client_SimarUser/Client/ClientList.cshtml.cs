using ClienteWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Pages.Client_SimarUser.Client
{
    [IgnoreAntiforgeryToken]
    public class ClientListModel : PageModel
    {

        private readonly HttpClient _clientesApi;
        private readonly HttpClient _usuariosApi;

        public List<ClienteOutput> Clientes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        public ClientListModel(IHttpClientFactory factory)
        {
            _clientesApi = factory.CreateClient("ClientesApi");
            _usuariosApi = factory.CreateClient("UsuariosApi");
        }

        [BindProperty]
        public RegisterClientInput Input { get; set; }

        [BindProperty]
        public EditClientInput EditInput { get; set; }

        [BindProperty]
        public IFormFile? CertificadoFile { get; set; }


        // consultar clientes al cargar la página
        public async Task<IActionResult> OnGetAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol == "cliente")
            {
                return RedirectToPage("/Index");
            }

            try
            {

                // 1. Obtener todos los clientes
                var clientesResponse = await _clientesApi.GetAsync("/client/all");

                if (!clientesResponse.IsSuccessStatusCode)
                {
                    return Page();
                }

                var clientes = await clientesResponse.Content
                    .ReadFromJsonAsync<List<ClienteOutput>>();

                if (clientes == null || !clientes.Any())
                {
                    Clientes = new List<ClienteOutput>();
                    return Page();
                }

                // 2. Obtener usuarios asociados a los clientes
                var userIds = clientes
                    .Where(c => !string.IsNullOrEmpty(c.IdUser))
                    .Select(c => c.IdUser)
                    .Distinct()
                    .ToList();

                var usuariosDict = new Dictionary<string, UsuarioSimpleResponseDto>();

                foreach (var userId in userIds)
                {
                    try
                    {
                        var userResponse = await _usuariosApi.GetAsync($"/api/usuarios/{userId}");

                        if (userResponse.IsSuccessStatusCode)
                        {
                            var usuario = await userResponse.Content
                                .ReadFromJsonAsync<UsuarioSimpleResponseDto>();

                            if (usuario != null)
                            {
                                usuariosDict[userId] = usuario;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener usuario {userId}: {ex.Message}");
                    }
                }

                // 3. Enriquecer los clientes con datos del usuario
                foreach (var cliente in clientes)
                {
                    if (!string.IsNullOrEmpty(cliente.IdUser) && usuariosDict.TryGetValue(cliente.IdUser, out var usuario))
                    {
                        cliente.UserName = usuario.Username;
                        cliente.Email = usuario.Email;
                    }
                }

                Clientes = clientes;

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Page();
            }
        }       


        // =========================================
        //  REGISTRAR NUEVO CLIENTE
        // =========================================

        public async Task<IActionResult> OnPostRegistrarClienteAsync([FromForm] RegisterClientInput input)
        {
            try
            {

                if (CertificadoFile != null)
                {
                    if (!ValidarArchivo(CertificadoFile, out string mensajeError))
                    {
                        return new JsonResult(new { success = false, message = mensajeError });
                    }
                }

                var token = HttpContext.Session.GetString("JWT");

                if (string.IsNullOrEmpty(token))
                {
                    return new JsonResult(new { success = false, expired = true });
                }

                _usuariosApi.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _clientesApi.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // 1. usuario
                var userData = new
                {
                    email = input.Email,
                    username = input.UserName,
                    password = input.Password,
                    role = "cliente"
                };

                var userResponse = await _usuariosApi.PostAsJsonAsync(
                    "/api/usuarios/registro-simple",
                    userData
                );

                if (!userResponse.IsSuccessStatusCode)
                {
                    var error = await userResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al crear usuario: {error}");

                    return new JsonResult(new
                    {
                        success = false,
                        message = $"Error al crear usuario: {error}"
                    });
                }

                var usuarioResponse = await userResponse.Content.ReadFromJsonAsync<UsuarioRegistroSimpleResponse>();

                if (usuarioResponse == null || usuarioResponse.IdUser == Guid.Empty)
                {
                    var responseContent = await userResponse.Content.ReadAsStringAsync();
                    return new JsonResult(new
                    {
                        success = false,
                        message = $"No se pudo obtener el ID del usuario. Respuesta: {responseContent}"
                    });
                }

                Guid userId = usuarioResponse.IdUser;


                // 2. cliente
                var clientData = new
                {
                    idUser = userId,
                    name = input.Name,
                    businessName = input.BusinessName,
                    contactEmail = input.ContactEmail,
                    phone = input.Phone,
                    address = input.Address,
                    rfc = input.RFC,
                    semarnatNum = input.SemarnatNum,
                    status = "activo"
                };

                var clientResponse = await _clientesApi.PostAsJsonAsync(
                    "/client",
                    clientData
                );

                if (!clientResponse.IsSuccessStatusCode)
                {
                    var error = await clientResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al crear cliente: {error}");

                    return new JsonResult(new
                    {
                        success = false,
                        message = $"Error al crear cliente: {error}"
                    });
                }

                var created = await clientResponse.Content
                    .ReadFromJsonAsync<ClienteOutput>();

                return new JsonResult(new
                {
                    success = true,
                    clientId = created?.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en registro: {ex.Message}");
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // =========================================
        // EDITAR CLIENTE
        // =========================================
        /*public async Task<IActionResult> OnPostEditarAsync()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);

                var body = await reader.ReadToEndAsync();

                Console.WriteLine(body);

                var input = System.Text.Json.JsonSerializer.Deserialize<EditClientInput>(
                    body,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (input == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Datos vacíos"
                    });
                }

                var token = HttpContext.Session.GetString("JWT");

                _clientesApi.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _clientesApi.PutAsJsonAsync(
                    $"/client/{input.Id}",
                    input
                );

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return new JsonResult(new
                    {
                        success = false,
                        expired = true,
                        message = "La sesión expiró"
                    });
                }

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Error al editar cliente";
                }

                TempData["Mensaje"] = "Cliente actualizado correctamente";

                return new JsonResult(new
                {
                    success = response.IsSuccessStatusCode,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }*/


        public async Task<IActionResult> OnPostEditarCompletoAsync()
        {
            try
            {
                Console.WriteLine("=== OnPostEditarCompletoAsync HA SIDO LLAMADO ===");

                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                Console.WriteLine($"Body recibido: {body}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var payload = JsonSerializer.Deserialize<EditarCompletoPayload>(body, options);

                Console.WriteLine($"HasClientChanges: {payload?.HasClientChanges}");
                Console.WriteLine($"HasUserChanges: {payload?.HasUserChanges}");
                Console.WriteLine($"IdUser: {payload?.IdUser}");

                if (payload == null)
                {
                    return new JsonResult(new { success = false, message = "Datos vacíos" });
                }

                var token = HttpContext.Session.GetString("JWT");
                _clientesApi.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // 1. Actualizar CLIENTE solo si hay datos
                if (payload.Cliente != null && payload.Cliente != null)
                {
                    var clientResponse = await _clientesApi.PutAsJsonAsync(
                        $"/client/{payload.Cliente.Id}",
                        payload.Cliente
                    );

                    if (!clientResponse.IsSuccessStatusCode)
                    {
                        var error = await clientResponse.Content.ReadAsStringAsync();
                        return new JsonResult(new { success = false, message = $"Error al actualizar cliente: {error}" });
                    }
                }

                // 2. Actualizar USUARIO solo si hay datos
                if (payload.Usuario != null && payload.Usuario != null  && !string.IsNullOrEmpty(payload.IdUser))
                {
                    var userResponse = await _usuariosApi.PutAsJsonAsync(
                        $"/api/usuarios/{payload.IdUser}",
                        payload.Usuario
                    );

                    if (!userResponse.IsSuccessStatusCode)
                    {
                        var error = await userResponse.Content.ReadAsStringAsync();
                        return new JsonResult(new { success = false, message = $"Error al actualizar usuario: {error}" });
                    }
                }

                return new JsonResult(new { success = true, message = "Actualizado correctamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en OnPostEditarCompletoAsync: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // =========================================
        // EDITAR Archivo SAT (CERTIFICADO)
        // =========================================
        public async Task<IActionResult> OnPostActualizarCertificadoAsync(int id, IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Archivo vacío"
                    });
                }

                var token = HttpContext.Session.GetString("JWT");

                if (string.IsNullOrEmpty(token))
                {
                    return new JsonResult(new
                    {
                        success = false,
                        expired = true
                    });
                }

                _clientesApi.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using var content = new MultipartFormDataContent();

                using var stream = file.OpenReadStream();

                var fileContent = new StreamContent(stream);

                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);


                content.Add(fileContent, "file", file.FileName);

                var response = await _clientesApi.PostAsync(
                    $"/client/id/{id}/certificate",
                    content
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    return new JsonResult(new
                    {
                        success = false,
                        message = error
                    });
                }

                return new JsonResult(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // =========================================
        // CAMBIAR ESTADO DE CLIENTE (ACTIVAR/DESACTIVAR)
        // =========================================

        public async Task<IActionResult> OnPostCambiarEstadoAsync(
            int id,
            string accion)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWT");

                if (string.IsNullOrEmpty(token))
                {
                    return new JsonResult(new
                    {
                        success = false,
                        expired = true
                    });
                }

                _clientesApi.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );

                HttpResponseMessage response;

                if (accion == "desactivar")
                {
                    response = await _clientesApi.PatchAsync(
                        $"/client/{id}/deactivate",
                        null
                    );
                }
                else
                {
                    response = await _clientesApi.PatchAsync(
                        $"/client/{id}/activate",
                        null
                    );
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    return new JsonResult(new
                    {
                        success = false,
                        message = error
                    });
                }

                return new JsonResult(new
                {
                    success = true,
                    message = accion == "desactivar"
                        ? "Cliente desactivado"
                        : "Cliente activado"
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public async Task<IActionResult> OnPostSubirCertificadoAsync(int id, IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return new JsonResult(new { success = false, message = "No se seleccionó ningún archivo" });
                }

                // Validar tamaño
                if (!ValidarArchivo(file, out string mensajeError))
                {
                    return new JsonResult(new { success = false, message = mensajeError });
                }

                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();

                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.FileName);

                var response = await _clientesApi.PostAsync(
                    $"/client/id/{id}/certificate",
                    content
                );

                return new JsonResult(new
                {
                    success = response.IsSuccessStatusCode
                });
            }
            catch
            {
                return new JsonResult(new { success = false });
            }
        }


        // Máximo 5 MB
        private const long MaxFileSize = 5 * 1024 * 1024;

        // Tipos de archivo permitidos
        private readonly string[] AllowedFileTypes = { ".pdf" };

        private bool ValidarArchivo(IFormFile file, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (file == null) return true;

            // Validar tamaño
            if (file.Length > MaxFileSize)
            {
                var sizeMB = file.Length / (1024.0 * 1024.0);
                mensajeError = $"El archivo excede el tamaño máximo permitido de 5 MB (actual: {sizeMB:F2} MB)";
                return false;
            }

            // Validar extensión
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedFileTypes.Contains(extension))
            {
                mensajeError = $"Tipo de archivo no permitido. Solo se permite: {string.Join(", ", AllowedFileTypes)}";
                return false;
            }

            return true;
        }
    }


    public class EditarCompletoPayload
        {
            public EditClientInput Cliente { get; set; }
            public EditUserInput Usuario { get; set; }
            public string IdUser { get; set; }
            public bool HasClientChanges { get; set; }
            public bool HasUserChanges { get; set; }
            public bool SoloCertificado { get; set; }
            public int Id { get; set; }
        }

        public class EditUserInput
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? Password { get; set; }
        }
    public class RegistrarClienteRequest
    {
        public UserDto User { get; set; }
        public ClientDto Client { get; set; }
    }

    public class ClienteOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string BusinessName { get; set; }
        public string Phone { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string RFC { get; set; }
        public string UrlSatCertificate { get; set; }
        public string SemarnatNum { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ContactEmail { get; set; }

        public string IdUser { get; set; }

    }

    public class RegisterClientInput
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }

        public string Name { get; set; }
        public string BusinessName { get; set; }
        public string ContactEmail { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string RFC { get; set; }
        public string SemarnatNum { get; set; }
        public string Status { get; set; } = "activo";

        public string IdUser { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }

        public string Email { get; set; }
    }

    public class EditClientInput
    {

        //public string Email { get; set; }
        //public string UserName { get; set; }
        //public string Password { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string BusinessName { get; set; }

        public string ContactEmail { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string RFC { get; set; }

        public string SemarnatNum { get; set; }

        public string Status { get; set; }

        public string IdUser { get; set; }
    }

  

    public class UserDto
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class UsuarioRegistroSimpleResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("id_user")]
        public Guid IdUser { get; set; }
    }

    public class UsuarioSimpleResponseDto
    {
        public Guid IdUser { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UsuarioUpdateDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
