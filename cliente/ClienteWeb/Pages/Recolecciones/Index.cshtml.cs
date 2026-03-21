using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteWeb.Pages.Recolecciones
{
    public class Recoleccion
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Vehiculo { get; set; } = string.Empty;
        public string Tecnico { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string? TipoResiduo { get; set; }
        public decimal? CantidadEstimada { get; set; }
        public string? Estado { get; set; } = "Programada";
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
        public List<ClienteItem> Clientes { get; set; } = new();
        public List<TecnicoItem> Tecnicos { get; set; } = new();
        public List<VehiculoItem> Vehiculos { get; set; } = new();

        public void OnGet()
        {
            // Datos de ejemplo para comboboxes
            CargarDatosCombobox();

            // Datos de ejemplo para la tabla
            Recolecciones = new List<Recoleccion>
            {
                new Recoleccion
                {
                    Id = 1,
                    Cliente = "Industrias ABC",
                    Fecha = DateTime.Now.AddDays(2),
                    Direccion = "Av. Industrial 123, Zona Norte",
                    Vehiculo = "Kenworth T680 - VH-001",
                    Tecnico = "Juan Pérez",
                    Observaciones = "Residuos peligrosos - Manejo especial",
                    TipoResiduo = "Peligrosos",
                    CantidadEstimada = 5.5m,
                    Estado = "Programada"
                },
                new Recoleccion
                {
                    Id = 2,
                    Cliente = "Hospital Regional",
                    Fecha = DateTime.Now.AddDays(1),
                    Direccion = "Blvd. de la Salud 456, Centro",
                    Vehiculo = "International 4300 - VH-002",
                    Tecnico = "María García",
                    Observaciones = "Residuos biológicos - Contenedores especiales",
                    TipoResiduo = "Biológicos",
                    CantidadEstimada = 3.2m,
                    Estado = "Programada"
                },
                new Recoleccion
                {
                    Id = 3,
                    Cliente = "Centro Comercial Plaza",
                    Fecha = DateTime.Now.AddDays(3),
                    Direccion = "Av. Principal 789, Colonia Centro",
                    Vehiculo = "Freightliner M2 - VH-003",
                    Tecnico = "Carlos López",
                    Observaciones = "Recolección de papel y cartón",
                    TipoResiduo = "Reciclables",
                    CantidadEstimada = 2.8m,
                    Estado = "Programada"
                },
                new Recoleccion
                {
                    Id = 4,
                    Cliente = "Laboratorios Médicos",
                    Fecha = DateTime.Now.AddDays(-1),
                    Direccion = "Calle de la Ciencia 321, Zona Industrial",
                    Vehiculo = "RAM 5500 - VH-004",
                    Tecnico = "Ana Martínez",
                    Observaciones = "Residuos biológicos - Urgente",
                    TipoResiduo = "Biológicos",
                    CantidadEstimada = 1.5m,
                    Estado = "Completada"
                },
                new Recoleccion
                {
                    Id = 5,
                    Cliente = "Constructora del Norte",
                    Fecha = DateTime.Now.AddDays(5),
                    Direccion = "Periférico Norte 555, Colonia Industrial",
                    Vehiculo = "Kenworth T680 - VH-001",
                    Tecnico = "Roberto Sánchez",
                    Observaciones = "Residuos de construcción",
                    TipoResiduo = "Construcción",
                    CantidadEstimada = 12.0m,
                    Estado = "Programada"
                },
                new Recoleccion
                {
                    Id = 6,
                    Cliente = "Restaurante El Sazón",
                    Fecha = DateTime.Now.AddDays(-2),
                    Direccion = "Calle de los Sabores 222, Centro",
                    Vehiculo = "International 4300 - VH-002",
                    Tecnico = "Laura Rodríguez",
                    Observaciones = "Residuos orgánicos",
                    TipoResiduo = "Orgánicos",
                    CantidadEstimada = 0.8m,
                    Estado = "Cancelada"
                }
            };

            // Aplicar filtros (simulado)
            if (!string.IsNullOrEmpty(FiltroCliente))
            {
                Recolecciones = Recolecciones.Where(r => 
                    r.Cliente.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (FiltroFechaInicio.HasValue)
            {
                Recolecciones = Recolecciones.Where(r => r.Fecha.Date >= FiltroFechaInicio.Value.Date).ToList();
            }

            if (FiltroFechaFin.HasValue)
            {
                Recolecciones = Recolecciones.Where(r => r.Fecha.Date <= FiltroFechaFin.Value.Date).ToList();
            }

            if (!string.IsNullOrEmpty(FiltroVehiculo))
            {
                Recolecciones = Recolecciones.Where(r => 
                    r.Vehiculo.Contains(FiltroVehiculo, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(FiltroTecnico))
            {
                Recolecciones = Recolecciones.Where(r => 
                    r.Tecnico.Contains(FiltroTecnico, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(FiltroEstado))
            {
                Recolecciones = Recolecciones.Where(r => 
                    r.Estado == FiltroEstado).ToList();
            }
        }

        private void CargarDatosCombobox()
        {
            // Clientes de ejemplo
            Clientes = new List<ClienteItem>
            {
                new ClienteItem { Id = 1, Nombre = "Industrias ABC", Direccion = "Av. Industrial 123" },
                new ClienteItem { Id = 2, Nombre = "Hospital Regional", Direccion = "Blvd. de la Salud 456" },
                new ClienteItem { Id = 3, Nombre = "Centro Comercial Plaza", Direccion = "Av. Principal 789" },
                new ClienteItem { Id = 4, Nombre = "Laboratorios Médicos", Direccion = "Calle de la Ciencia 321" },
                new ClienteItem { Id = 5, Nombre = "Constructora del Norte", Direccion = "Periférico Norte 555" },
                new ClienteItem { Id = 6, Nombre = "Restaurante El Sazón", Direccion = "Calle de los Sabores 222" },
                new ClienteItem { Id = 7, Nombre = "Universidad Tecnológica", Direccion = "Campus Universitario 100" },
                new ClienteItem { Id = 8, Nombre = "Farmacéutica del Valle", Direccion = "Av. de la Salud 789" }
            };

            // Técnicos de ejemplo
            Tecnicos = new List<TecnicoItem>
            {
                new TecnicoItem { Id = 1, Nombre = "Juan Pérez", Especialidad = "Residuos peligrosos" },
                new TecnicoItem { Id = 2, Nombre = "María García", Especialidad = "Residuos biológicos" },
                new TecnicoItem { Id = 3, Nombre = "Carlos López", Especialidad = "Residuos reciclables" },
                new TecnicoItem { Id = 4, Nombre = "Ana Martínez", Especialidad = "Residuos biológicos" },
                new TecnicoItem { Id = 5, Nombre = "Roberto Sánchez", Especialidad = "Residuos construcción" },
                new TecnicoItem { Id = 6, Nombre = "Laura Rodríguez", Especialidad = "Residuos orgánicos" },
                new TecnicoItem { Id = 7, Nombre = "Miguel Ángel Torres", Especialidad = "Residuos peligrosos" },
                new TecnicoItem { Id = 8, Nombre = "Gabriela Flores", Especialidad = "Residuos varios" }
            };

            // Vehículos de ejemplo
            Vehiculos = new List<VehiculoItem>
            {
                new VehiculoItem { Id = 1, Nombre = "Kenworth T680", Placas = "VH-001" },
                new VehiculoItem { Id = 2, Nombre = "International 4300", Placas = "VH-002" },
                new VehiculoItem { Id = 3, Nombre = "Freightliner M2", Placas = "VH-003" },
                new VehiculoItem { Id = 4, Nombre = "RAM 5500", Placas = "VH-004" }
            };
        }

        // Métodos para obtener datos para combobox via AJAX
        public IActionResult OnGetObtenerClientes()
        {
            CargarDatosCombobox();
            return new JsonResult(Clientes);
        }

        public IActionResult OnGetObtenerTecnicos()
        {
            CargarDatosCombobox();
            return new JsonResult(Tecnicos);
        }

        public IActionResult OnGetObtenerVehiculos()
        {
            CargarDatosCombobox();
            return new JsonResult(Vehiculos);
        }

        public IActionResult OnGetObtenerEstados()
        {
            var estados = new List<string> { "Programada", "Completada", "Cancelada", "En ruta" };
            return new JsonResult(estados);
        }
    }
}