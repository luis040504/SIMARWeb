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

        public List<ClienteWeb.Services.VehiculoDto> Vehiculos { get; set; } = new();
        
        public List<ClienteWeb.Services.TipoResiduoCatalogoDto> TiposResiduoDisponibles { get; set; } = new();
        
        [TempData]
        public string? SuccessMessage { get; set; }
        
        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Cargar vehículos
            Vehiculos = await _vehiculosService.GetAllAsync(Search);
            
            // Cargar tipos de residuo disponibles del catálogo
            TiposResiduoDisponibles = await _vehiculosService.GetTiposResiduoDisponiblesAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            [FromForm] string? NumeroEconomico,
            [FromForm] string Marca,
            [FromForm] string Modelo,
            [FromForm] int? Anio,
            [FromForm] string? Color,
            [FromForm] string Placas,
            [FromForm] decimal? PesoToneladas,
            [FromForm] string LicenciaRequerida,
            [FromForm] string TipoGasolina,
            [FromForm] string? Descripcion,
            [FromForm] List<int> TiposResiduoIds,
            IFormFile? FotoArchivo)
        {
            if (string.IsNullOrWhiteSpace(Marca) || 
                string.IsNullOrWhiteSpace(Modelo) || 
                string.IsNullOrWhiteSpace(Placas) || 
                string.IsNullOrWhiteSpace(LicenciaRequerida) || 
                string.IsNullOrWhiteSpace(TipoGasolina))
            {
                ErrorMessage = "Datos inválidos. Verifica los campos requeridos.";
                await OnGetAsync();
                return Page();
            }

            byte[]? fotoBytes = null;
            if (FotoArchivo != null && FotoArchivo.Length > 0)
            {
                using var ms = new MemoryStream();
                await FotoArchivo.CopyToAsync(ms);
                fotoBytes = ms.ToArray();
                
                // Validar tamaño máximo 20MB
                if (fotoBytes.Length > 20 * 1024 * 1024)
                {
                    ErrorMessage = "La foto no puede superar los 20MB";
                    await OnGetAsync();
                    return Page();
                }
            }
            
            var vehiculo = new ClienteWeb.Services.VehiculoCreateDto
            {
                NumeroEconomico = NumeroEconomico,
                Marca = Marca,
                Modelo = Modelo,
                Anio = Anio,
                Color = Color,
                Placas = Placas,
                PesoToneladas = PesoToneladas,
                LicenciaRequerida = LicenciaRequerida,
                TipoGasolina = TipoGasolina,
                Descripcion = Descripcion,
                TiposResiduoIds = TiposResiduoIds ?? new List<int>(),
                Foto = fotoBytes
            };

            var (success, error) = await _vehiculosService.CreateAsync(vehiculo);
            
            if (success)
                SuccessMessage = "Vehículo creado exitosamente";
            else
                ErrorMessage = error;
            
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int id,
            [FromForm] string? NumeroEconomico,
            [FromForm] string Marca,
            [FromForm] string Modelo,
            [FromForm] int? Anio,
            [FromForm] string? Color,
            [FromForm] string Placas,
            [FromForm] decimal? PesoToneladas,
            [FromForm] string LicenciaRequerida,
            [FromForm] string TipoGasolina,
            [FromForm] string? Descripcion,
            [FromForm] List<int> TiposResiduoIds,
            IFormFile? FotoArchivo)
        {
            if (string.IsNullOrWhiteSpace(Marca) || 
                string.IsNullOrWhiteSpace(Modelo) || 
                string.IsNullOrWhiteSpace(Placas) || 
                string.IsNullOrWhiteSpace(LicenciaRequerida) || 
                string.IsNullOrWhiteSpace(TipoGasolina))
            {
                ErrorMessage = "Datos inválidos. Verifica los campos requeridos.";
                await OnGetAsync();
                return Page();
            }

            byte[]? fotoBytes = null;
            if (FotoArchivo != null && FotoArchivo.Length > 0)
            {
                using var ms = new MemoryStream();
                await FotoArchivo.CopyToAsync(ms);
                fotoBytes = ms.ToArray();
                
                // Validar tamaño máximo 20MB
                if (fotoBytes.Length > 20 * 1024 * 1024)
                {
                    ErrorMessage = "La foto no puede superar los 20MB";
                    await OnGetAsync();
                    return Page();
                }
            }
            
            var vehiculo = new ClienteWeb.Services.VehiculoCreateDto
            {
                NumeroEconomico = NumeroEconomico,
                Marca = Marca,
                Modelo = Modelo,
                Anio = Anio,
                Color = Color,
                Placas = Placas,
                PesoToneladas = PesoToneladas,
                LicenciaRequerida = LicenciaRequerida,
                TipoGasolina = TipoGasolina,
                Descripcion = Descripcion,
                TiposResiduoIds = TiposResiduoIds ?? new List<int>(),
                Foto = fotoBytes
            };

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
    }
}