using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;


namespace ClienteWeb.Pages.Client_SimarUser.Client
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient _authApi;

        public LoginModel(IHttpClientFactory factory)
        {
            _authApi = factory.CreateClient("AuthApi");
        }

        [BindProperty]
        public UserLoginInput Input { get; set; }
        public void OnGet() 
        {
            // Limpiar mensajes anteriores al cargar la página
            if (TempData["Mensaje"] != null)
            {
                TempData.Remove("Mensaje");
            }
        }


        // Iniciar sesión
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {

                var loginData = new
                {
                    identifier = Input.Identifier,
                    password = Input.Password
                };

                var tokenResponse = await _authApi.PostAsJsonAsync(
                    "/login",
                    loginData
                );

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Credenciales incorrectas. Por favor, verifique su usuario y contraseña.";
                    return Page();
                }


                var token = await tokenResponse.Content
                    .ReadFromJsonAsync<LoginResponse>();

                if (token == null || string.IsNullOrEmpty(token.AccessToken))
                {
                    TempData["Mensaje"] = "No se recibió el token";
                    return RedirectToPage();
                }

                HttpContext.Session.SetString("JWT", token.AccessToken);

                var handler = new JwtSecurityTokenHandler();

                var jwtToken = handler.ReadJwtToken(token.AccessToken);

                var rol = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == "role")?.Value;

                var userId = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == "sub")?.Value;

                HttpContext.Session.SetString("Rol", rol ?? "");
                HttpContext.Session.SetString("UserId", userId ?? "");

                

                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error: {ex.Message}";
                return RedirectToPage();
            }
        }
    }

}

public class UserLoginInput
{
        public string Identifier { get; set; }

        public string Password { get; set; }

        

}

public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}

