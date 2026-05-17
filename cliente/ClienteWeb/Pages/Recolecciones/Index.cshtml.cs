using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClienteWeb.Services;

namespace ClienteWeb.Pages.Recolecciones
{
    public class IndexModel : PageModel
    {
        private readonly RecoleccionesApiService _recoleccionesService;
        private readonly VehiculosApiService _vehiculosService;
        private readonly EmpleadosApiService _empleadosService;

        public IndexModel(RecoleccionesApiService recoleccionesService, VehiculosApiService vehiculosService, EmpleadosApiService empleadosService)
        {
            _recoleccionesService = recoleccionesService;
            _vehiculosService = vehiculosService;
            _empleadosService = empleadosService;
        }

        // ============ FILTROS ============
        [BindProperty(SupportsGet = true)]
        public int? FiltroIdContrato { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroCliente { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FiltroFechaInicio { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FiltroFechaFin { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroVehiculo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroChofer { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroTecnico { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroEstado { get; set; }

        // ============ DATOS PARA LA VISTA ============
        public List<RecoleccionDto> Recolecciones { get; set; } = new();
        public List<VehiculoDto> Vehiculos { get; set; } = new();
        public List<ContratoItemDto> Contratos { get; set; } = new();
        public List<EmpleadoItemDto> Choferes { get; set; } = new();
        public List<EmpleadoItemDto> Tecnicos { get; set; } = new();


        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            // Cargar vehículos para combobox
            Vehiculos = await _vehiculosService.GetAllAsync();

            // Cargar contratos para combobox
            Contratos = await _recoleccionesService.GetContratosActivosAsync();
            var choferesResult = await _empleadosService.GetChoferesAsync();
            Choferes = choferesResult.Select(c => new EmpleadoItemDto { UserId = c.UserId, FullName = c.FullName }).ToList();
            Tecnicos = await _empleadosService.GetTecnicosAsync();

            // Cargar recolecciones con filtros
            var filtro = new RecoleccionFilter
            {
                IdContrato = FiltroIdContrato,
                Cliente = FiltroCliente,
                FechaInicio = FiltroFechaInicio,
                FechaFin = FiltroFechaFin,
                Vehiculo = FiltroVehiculo,
                Chofer = FiltroChofer,
                Tecnico = FiltroTecnico,
                Estado = FiltroEstado
            };

            Recolecciones = await _recoleccionesService.GetAllAsync(filtro);
        }

        // ============ CRUD ============
        public async Task<IActionResult> OnPostCreateAsync(RecoleccionCreateDto recoleccion)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Datos inválidos. Verifica los campos requeridos.";
                await CargarDatosAsync();
                return Page();
            }

            // Validar que tenga al menos un vehículo
            if (recoleccion.Vehiculos == null || recoleccion.Vehiculos.Count == 0)
            {
                ErrorMessage = "Debe asignar al menos un vehículo con chofer";
                await CargarDatosAsync();
                return Page();
            }

            var (success, error) = await _recoleccionesService.CreateAsync(recoleccion);

            if (success)
                SuccessMessage = "Recolección programada exitosamente";
            else
                ErrorMessage = error;

            await CargarDatosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string id, RecoleccionUpdateDto recoleccion)
        {
            if (string.IsNullOrEmpty(id))
            {
                ErrorMessage = "ID de recolección inválido";
                await CargarDatosAsync();
                return Page();
            }

            var (success, error) = await _recoleccionesService.UpdateAsync(id, recoleccion);

            if (success)
                SuccessMessage = "Recolección actualizada exitosamente";
            else
                ErrorMessage = error;

            await CargarDatosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ErrorMessage = "ID de recolección inválido";
                await CargarDatosAsync();
                return Page();
            }

            var (success, error) = await _recoleccionesService.DeleteAsync(id);

            if (success)
                SuccessMessage = "Recolección eliminada exitosamente";
            else
                ErrorMessage = error;

            await CargarDatosAsync();
            return Page();
        }

        // ============ ENDPOINTS AJAX ============
        public async Task<IActionResult> OnGetObtenerEstados()
        {
            var estados = await _recoleccionesService.GetEstadosAsync();
            return new JsonResult(estados);
        }
    }
}