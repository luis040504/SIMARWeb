using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClienteWeb.Services;
using ClienteWeb.Models;
using System.Text.Json;

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
        public List<ClienteWeb.Services.VehiculoDto> Vehiculos { get; set; } = new();
        public List<ContratoItemDto> Contratos { get; set; } = new();
        public List<EmpleadoItemDto> Choferes { get; set; } = new();
        public List<EmpleadoItemDto> Tecnicos { get; set; } = new();
        public List<ClienteWeb.Models.TipoResiduoCatalogoDto> TiposResiduoDisponibles { get; set; } = new();

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

            // Cargar tipos de residuo disponibles desde el catálogo de vehículos
            var tiposResiduoServices = await _vehiculosService.GetTiposResiduoDisponiblesAsync();
            TiposResiduoDisponibles = tiposResiduoServices.Select(t => new ClienteWeb.Models.TipoResiduoCatalogoDto
            {
                Id = t.Id,
                CodigoCatalogo = t.CodigoCatalogo,
                Nombre = t.Nombre,
                TipoResiduo = t.TipoResiduo,
                Descripcion = t.Descripcion
            }).ToList();

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
        public async Task<IActionResult> OnPostCreateAsync(
            int idContrato,
            string cliente,
            DateTime fecha,
            string direccion,
            string tiposResiduo,
            string vehiculos,
            string? observaciones,
            string estado = "Programada")
        {
            // Validar campos requeridos
            if (idContrato <= 0 || string.IsNullOrWhiteSpace(cliente) || string.IsNullOrWhiteSpace(direccion))
            {
                ErrorMessage = "Datos inválidos. Verifica los campos requeridos.";
                await CargarDatosAsync();
                return Page();
            }

            // Deserializar vehículos desde JSON
            var vehiculosList = new List<VehiculoAsignadoInput>();
            if (!string.IsNullOrEmpty(vehiculos))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    vehiculosList = JsonSerializer.Deserialize<List<VehiculoAsignadoInput>>(vehiculos, options) ?? new List<VehiculoAsignadoInput>();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al procesar los vehículos: {ex.Message}";
                    await CargarDatosAsync();
                    return Page();
                }
            }

            // Validar vehículos
            if (vehiculosList == null || vehiculosList.Count == 0)
            {
                ErrorMessage = "Debe asignar al menos un vehículo";
                await CargarDatosAsync();
                return Page();
            }

            // Deserializar los tipos de residuo desde JSON
            var tiposResiduoList = new List<ClienteWeb.Models.TipoResiduoRecoleccionDto>();
            if (!string.IsNullOrEmpty(tiposResiduo))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    tiposResiduoList = JsonSerializer.Deserialize<List<ClienteWeb.Models.TipoResiduoRecoleccionDto>>(tiposResiduo, options) ?? new List<ClienteWeb.Models.TipoResiduoRecoleccionDto>();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al procesar los tipos de residuo: {ex.Message}";
                    await CargarDatosAsync();
                    return Page();
                }
            }

            // Validar tipos de residuo
            if (tiposResiduoList == null || tiposResiduoList.Count == 0)
            {
                ErrorMessage = "Debe seleccionar al menos un tipo de residuo con cantidad válida";
                await CargarDatosAsync();
                return Page();
            }

            // Validar que todas las cantidades sean mayores a 0
            foreach (var residuo in tiposResiduoList)
            {
                if (residuo.CantidadEstimada <= 0)
                {
                    ErrorMessage = $"La cantidad para {residuo.WasteTypeName} debe ser mayor a 0";
                    await CargarDatosAsync();
                    return Page();
                }
            }

            // Convertir VehiculoAsignadoInput a VehiculoAsignadoDto
            var vehiculosDto = vehiculosList.Select(v => new ClienteWeb.Models.VehiculoAsignadoDto
            {
                Vehiculo = v.Vehiculo,
                Chofer = v.Chofer,
                Tecnicos = v.Tecnicos ?? new List<string>()
            }).ToList();

            var recoleccion = new RecoleccionCreateDto
            {
                IdContrato = idContrato,
                Cliente = cliente,
                Fecha = fecha,
                Direccion = direccion,
                Estado = estado,
                TiposResiduo = tiposResiduoList,
                Vehiculos = vehiculosDto,
                Observaciones = observaciones
            };

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

    // ============ DTOs auxiliares SOLO para input del formulario ============
    public class VehiculoAsignadoInput
    {
        public string Vehiculo { get; set; } = "";
        public string Chofer { get; set; } = "";
        public List<string> Tecnicos { get; set; } = new();
    }
}