using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClienteWeb.Services;

namespace ClienteWeb.Pages.Vehiculos
{
    public class IndexModel : PageModel
    {
        private readonly VehiculosApiService _vehiculosService;

        public IndexModel(VehiculosApiService vehiculosService)
        {
            _vehiculosService = vehiculosService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public List<VehiculoDto> Vehiculos { get; set; } = new();
        
        [TempData]
        public string? SuccessMessage { get; set; }
        
        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Vehiculos = await _vehiculosService.GetAllAsync(Search);
        }

        public async Task<IActionResult> OnPostCreateAsync(VehiculoCreateDto vehiculo)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Datos inválidos. Verifica los campos requeridos.";
                await OnGetAsync();
                return Page();
            }

            var (success, error) = await _vehiculosService.CreateAsync(vehiculo);
            
            if (success)
                SuccessMessage = "Vehículo creado exitosamente";
            else
                ErrorMessage = error;
            
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, VehiculoCreateDto vehiculo)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Datos inválidos.";
                await OnGetAsync();
                return Page();
            }

            var (success, error) = await _vehiculosService.UpdateAsync(id, vehiculo);
            
            if (success)
                SuccessMessage = "Vehículo actualizado exitosamente";
            else
                ErrorMessage = error;
            
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (success, error) = await _vehiculosService.DeleteAsync(id);
            
            if (success)
                SuccessMessage = "Vehículo eliminado exitosamente";
            else
                ErrorMessage = error;
            
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetTiposDesechoAsync()
        {
            var tipos = await _vehiculosService.GetTiposDesechoAsync();
            return new JsonResult(tipos);
        }

        public async Task<IActionResult> OnGetTiposGasolinaAsync()
        {
            var tipos = await _vehiculosService.GetTiposGasolinaAsync();
            return new JsonResult(tipos);
        }
    }
}