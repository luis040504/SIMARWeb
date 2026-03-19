using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    public class ConsultSimarUserModel : PageModel
    {
        // Aquí podrías definir una Lista de Usuarios para mostrar
        // public List<Usuario> Usuarios { get; set; }

        public void OnGet()
        {
            // Lógica para obtener usuarios de la DB o API
        }

        public IActionResult OnPostDelete(string username)
        {
            // Lógica para dar de baja al usuario
            return RedirectToPage();
        }
    }
}
