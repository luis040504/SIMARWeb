using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteWeb.Services;
using ClienteWeb.Pages.Manifest.Generate;

namespace ClienteWeb.Pages.Recolecciones
{
    public class Recoleccion
    {
        public string Id { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Vehiculo { get; set; } = string.Empty;
        public string Tecnico { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string? TipoResiduo { get; set; }
        public decimal? CantidadEstimada { get; set; }
        public string? Estado { get; set; } = "Programada";
        public string ManifestNumber { get; set; } = string.Empty;
    }

    public class ClienteItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; }
    }

    public class TecnicoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
    }

    public class VehiculoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;
    }

    public class IndexModel : PageModel
    {
        private readonly ManifestApiService _manifestService;
        private readonly ClientesApiService _clientesService;
        private readonly VehiculosApiService _vehiculosService;
        private readonly EmpleadosApiService _empleadosService;
        private readonly ContratosApiService _contratosService;

        public IndexModel(
            ManifestApiService manifestService,
            ClientesApiService clientesService,
            VehiculosApiService vehiculosService,
            EmpleadosApiService empleadosService,
            ContratosApiService contratosService)
        {
            _manifestService = manifestService;
            _clientesService = clientesService;
            _vehiculosService = vehiculosService;
            _empleadosService = empleadosService;
            _contratosService = contratosService;
        }

        [BindProperty(SupportsGet = true)]
        public string? FiltroCliente { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FiltroFechaInicio { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FiltroFechaFin { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroVehiculo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroTecnico { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroEstado { get; set; }

        public List<Recoleccion> Recolecciones { get; set; } = new();
        public List<ManifestSummary> ManifiestosPendientes { get; set; } = new();
        public List<ContratoDto> Contratos { get; set; } = new();
        public List<ClienteItem> Clientes { get; set; } = new();
        public List<TecnicoItem> Tecnicos { get; set; } = new();
        public List<VehiculoItem> Vehiculos { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarDatosComboboxAsync();

            var manifests = await _manifestService.GetAllAsync(
                razonSocial: FiltroCliente,
                estado: FiltroEstado);
            
            // FILTRO SOLICITADO: Solo mostrar contratos con una recolección programada
            // En este contexto, mostramos los manifiestos que están en borrador o en tránsito vinculados a contratos
            Recolecciones = manifests
                .Where(m => m.ContratoId.HasValue && m.Status != "cancelado") 
                .Select(m => new Recoleccion
            {
                Id = m.Id,
                ManifestNumber = m.ManifestNumber,
                Cliente = m.SocialReason,
                Fecha = m.ManifestDate.ToDateTime(TimeOnly.MinValue),
                Direccion = m.Municipality,
                Vehiculo = m.TransporterName,
                Tecnico = m.TransporterResponsibleName ?? "Pendiente", 
                Estado = m.Status switch
                {
                    "borrador" => "Programada",
                    "en_transito" => "En ruta",
                    "completado" => "Completada",
                    "cancelado" => "Cancelada",
                    _ => m.Status
                },
                TipoResiduo = m.ResidueSummary
            }).ToList();

            // Manifiestos que pueden ser programados (Borradores)
            ManifiestosPendientes = manifests.Where(m => m.Status == "borrador").ToList();

            // Contratos activos para generar nuevas recolecciones
            Contratos = await _contratosService.GetAllAsync();

            // Aplicar filtros de fecha
            if (FiltroFechaInicio.HasValue)
            {
                Recolecciones = Recolecciones.Where(r => r.Fecha.Date >= FiltroFechaInicio.Value.Date).ToList();
            }
            if (FiltroFechaFin.HasValue)
            {
                Recolecciones = Recolecciones.Where(r => r.Fecha.Date <= FiltroFechaFin.Value.Date).ToList();
            }
        }

        public async Task<IActionResult> OnGetObtenerContrato(int id)
        {
            var detail = await _contratosService.GetDetailAsync(id);
            return new JsonResult(detail);
        }

        private async Task CargarDatosComboboxAsync()
        {
            var clientes = await _clientesService.GetAllAsync();
            Clientes = clientes.Select(c => new ClienteItem { Id = c.Id, Nombre = c.Name, Direccion = c.Address }).ToList();

            var vehiculos = await _vehiculosService.GetAllAsync();
            Vehiculos = vehiculos.Select(v => new VehiculoItem { Id = v.Id, Nombre = v.Marca + " " + v.Modelo, Placas = v.Placas }).ToList();

            var empleados = await _empleadosService.GetAllAsync();
            Tecnicos = empleados.Select(e => new TecnicoItem { Id = e.Id, Nombre = e.Nombre, Especialidad = e.Puesto }).ToList();
        }

        public async Task<IActionResult> OnPostAsync(
            int? contractId, 
            string? manifestId, 
            string vehiculo, 
            string tecnico, 
            string estado, 
            DateTime fecha, 
            string direccion, 
            string tipoResiduo,
            decimal? cantidadEstimada,
            string? observaciones)
        {
            if (contractId.HasValue && string.IsNullOrEmpty(manifestId))
            {
                // Generar NUEVO manifiesto a partir del contrato
                var contrato = await _contratosService.GetDetailAsync(contractId.Value);
                if (contrato == null) return Page();

                var cliente = contrato.ClientId > 0 ? await _clientesService.GetByIdAsync(contrato.ClientId) : null;
                var wasteService = contrato.Services.FirstOrDefault(s => s.WasteType == tipoResiduo) ?? contrato.Services.FirstOrDefault();
                
                var choferes = await _empleadosService.GetChoferesAsync();
                var chofer = choferes.FirstOrDefault(c => c.FullName == tecnico);
                
                var vehiculosApi = await _vehiculosService.GetAllAsync();
                var vApi = vehiculosApi.FirstOrDefault(v => v.DisplayLabel == vehiculo || v.Placas == (vehiculo.Contains(" - ") ? vehiculo.Split(" - ")[1] : ""));

                var newManifest = new SpecialWasteModel(_manifestService, _clientesService, _vehiculosService, _contratosService, _empleadosService)
                {
                    IdCliente = cliente?.Id ?? 0,
                    ContratoId = contrato.Id,
                    SocialReason = contrato.ClientName,
                    Address = direccion ?? wasteService?.ServiceAddress ?? contrato.ClientAddress,
                    Municipality = wasteService?.ServiceAddress ?? cliente?.Address ?? contrato.ClientAddress, 
                    EnvironmentalRegistrationNumber = cliente?.SemarnatNum ?? "PENDIENTE",
                    PhoneNumber = cliente?.Phone ?? "PENDIENTE",
                    Email = cliente?.ContactEmail,
                    ManifestDate = DateOnly.FromDateTime(fecha),
                    ManifestTime = TimeOnly.FromDateTime(fecha),
                    GeneratorObservations = observaciones,
                    
                    // Datos del Transportista (Desde Recolecciones/Selección)
                    TransporterSocialReason = vApi?.Marca ?? "SIMAR", 
                    VehiclePlate = vApi?.Placas ?? (vehiculo.Contains(" - ") ? vehiculo.Split(" - ")[1] : vehiculo),
                    TransporterResponsibleName = chofer?.FullName ?? tecnico,
                    DriverName = chofer?.FullName ?? tecnico,
                    DriverLicense = chofer?.LicenseNumber,
                    VehicleType = vApi != null ? $"{vApi.Marca} {vApi.Modelo}" : "CAMION",
                    
                    // Datos del Generador (Extras)
                    GeneratorResponsibleName = cliente?.Name,

                    Residues = new List<ResidueItem>
                    {
                        new ResidueItem 
                        { 
                            ResidueName = tipoResiduo,
                            Unit = wasteService?.WasteUnit ?? "kg",
                            Weight = cantidadEstimada ?? 0,
                            ContainerType = "Bolsa/Contenedor"
                        }
                    }
                };

                var newId = await _manifestService.CreateSpecialAsync(newManifest);
                
                // Actualizar estado a lo solicitado (Programada o En ruta)
                string apiEstado = estado switch
                {
                    "Programada" => "borrador",
                    "En ruta" => "en_transito",
                    "Completada" => "completado",
                    "Cancelada" => "cancelado",
                    _ => "borrador"
                };
                
                await _manifestService.UpdateStatusAsync(newId, apiEstado);
                TempData["MensajeExito"] = "Recolección y Manifiesto generados exitosamente.";
            }
            else if (!string.IsNullOrEmpty(manifestId))
            {
                // Actualizar manifiesto EXISTENTE
                string apiEstado = estado switch
                {
                    "Programada" => "borrador",
                    "En ruta" => "en_transito",
                    "Completada" => "completado",
                    "Cancelada" => "cancelado",
                    _ => "en_transito"
                };

                await _manifestService.UpdateTransportAsync(manifestId, vehiculo, tecnico, apiEstado, observaciones, cantidadEstimada);
                TempData["MensajeExito"] = "Recolección actualizada exitosamente.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string id, string nuevoEstado)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(nuevoEstado))
                return BadRequest();

            // Mapear estado legible a estado de API
            string apiEstado = nuevoEstado switch
            {
                "Programada" => "borrador",
                "En ruta" => "en_transito",
                "Completada" => "completado",
                "Cancelada" => "cancelado",
                _ => nuevoEstado
            };

            await _manifestService.UpdateStatusAsync(id, apiEstado);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetObtenerManifiesto(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var detail = await _manifestService.GetByIdAsync(id);
            return new JsonResult(detail);
        }

        public async Task<IActionResult> OnGetObtenerClientes()
        {
            await CargarDatosComboboxAsync();
            return new JsonResult(Clientes);
        }

        public async Task<IActionResult> OnGetObtenerTecnicos()
        {
            await CargarDatosComboboxAsync();
            return new JsonResult(Tecnicos);
        }

        public async Task<IActionResult> OnGetObtenerVehiculos()
        {
            await CargarDatosComboboxAsync();
            return new JsonResult(Vehiculos);
        }

        public IActionResult OnGetObtenerEstados()
        {
            var estados = new List<string> { "Programada", "Completada", "Cancelada", "En ruta" };
            return new JsonResult(estados);
        }
    }
}