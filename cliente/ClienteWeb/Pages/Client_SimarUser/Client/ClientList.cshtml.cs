using ClienteWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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
                
                var response = await _clientesApi.GetAsync("/client/all");

                if (response.IsSuccessStatusCode)
                {
                    var clientes = await response.Content
                        .ReadFromJsonAsync<List<ClienteOutput>>();

                    if (clientes != null)
                    {
                        Clientes = clientes;
                    }
                }

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

        public async Task<IActionResult> OnPostRegistrarClienteAsync([FromBody] RegisterClientInput input)
        {
            try
            {
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
                    "/api/usuarios/registro-completo",
                    userData
                );

                if (!userResponse.IsSuccessStatusCode)
                {
                    var error = await userResponse.Content.ReadAsStringAsync();

                    return new JsonResult(new
                    {
                        success = false,
                        message = error
                    });
                }

                // 2. obtener id
                var idResponse = await _usuariosApi.GetAsync(
                    $"/api/usuarios/buscar-id/{input.UserName}"
                );

                var userId = await idResponse.Content
                    .ReadFromJsonAsync<UserIdResponse>();

                // 3. cliente
                var clientData = new
                {
                    idUser = userId.IdUser,
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
                    return new JsonResult(new { success = false, message = "Error cliente" });
                }

                var created = await clientResponse.Content
                    .ReadFromJsonAsync<ClienteOutput>();

                return new JsonResult(new
                {
                    success = true,
                    clientId = created.Id
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
        // EDITAR CLIENTE
        // =========================================
        public async Task<IActionResult> OnPostEditarAsync()
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
                    return new JsonResult(new { success = false });
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

    public class UserIdResponse
    {
        public string IdUser { get; set; }
    }

    public class UserDto
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
