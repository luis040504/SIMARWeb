using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace ClienteWeb.Pages.Client_SimarUser.SimarUser
{
    public class ConsultSimarUserModel : PageModel
    {

        public void OnGet()
        {
            // TODO - Lógica para obtener usuarios de la DB o API
        }

        public IActionResult OnPostDelete(string username)
        {
            // TODO - Lógica para dar de baja al usuario
            return RedirectToPage();
        }
    }
}
