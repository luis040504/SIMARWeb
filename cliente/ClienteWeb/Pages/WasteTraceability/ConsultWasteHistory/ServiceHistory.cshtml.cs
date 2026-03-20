using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace ClienteWeb.Pages.WasteTraceability.ConsultWasteHistory
{
    public class ServicioHistorial
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public string Observaciones { get; set; }
        public string Manifiesto { get; set; }
        public string TecnicoAsignado { get; set; }
        public string OperadorAsignado { get; set; }
        public string Vehiculo { get; set; }
        public string TipoResiduo { get; set; }
        public double CantidadEstimada { get; set; }
        public string Estado { get; set; }
        public DateTime FechaServicio { get; set; }
        public string Direccion { get; set; }
        public string Contrato { get; set; }
        public string Conductor { get; set; }
    }

    public class ServiceHistoryModel : PageModel
    {
        public List<ServicioHistorial> ServiciosHistorial { get; set; }
        public string ClienteSeleccionado { get; set; }

        public void OnGet(int? id, string cliente)
        {
            ClienteSeleccionado = cliente ?? "Cliente no especificado";
            
            ServiciosHistorial = new List<ServicioHistorial>();
            
            if (cliente == "Industrias ABC")
            {
                ServiciosHistorial = new List<ServicioHistorial>
                {
                    new ServicioHistorial
                    {
                        Id = 1001,
                        Cliente = "Industrias ABC",
                        Observaciones = "Recolección mensual de residuos plásticos - PENDIENTE",
                        Manifiesto = "MAN-2024-001",
                        TecnicoAsignado = "Carlos López",
                        OperadorAsignado = "Pedro Ramírez",
                        Vehiculo = "Camión ABC-123",
                        TipoResiduo = "Plásticos",
                        CantidadEstimada = 500.5,
                        Estado = "Asignado",
                        FechaServicio = DateTime.Today,
                        Direccion = "Av. Industrial 123, Parque Industrial",
                        Contrato = "CON-2024-001",
                        Conductor = "Juan Pérez"
                    },
                    new ServicioHistorial
                    {
                        Id = 1006,
                        Cliente = "Industrias ABC",
                        Observaciones = "Recolección de residuos plásticos - Lote 2",
                        Manifiesto = "MAN-2024-006",
                        TecnicoAsignado = "Carlos López",
                        OperadorAsignado = "Pedro Ramírez",
                        Vehiculo = "Camión ABC-123",
                        TipoResiduo = "Plásticos",
                        CantidadEstimada = 450.0,
                        Estado = "Recolectado",
                        FechaServicio = DateTime.Today.AddDays(-7),
                        Direccion = "Av. Industrial 123, Parque Industrial",
                        Contrato = "CON-2024-001",
                        Conductor = "Juan Pérez"
                    },
                    new ServicioHistorial
                    {
                        Id = 1009,
                        Cliente = "Industrias ABC",
                        Observaciones = "Recolección de residuos plásticos - Lote 3",
                        Manifiesto = "MAN-2024-009",
                        TecnicoAsignado = "Carlos López",
                        OperadorAsignado = "Pedro Ramírez",
                        Vehiculo = "Camión ABC-123",
                        TipoResiduo = "Plásticos",
                        CantidadEstimada = 520.0,
                        Estado = "En curso",
                        FechaServicio = DateTime.Today.AddDays(-14),
                        Direccion = "Av. Industrial 123, Parque Industrial",
                        Contrato = "CON-2024-001",
                        Conductor = "Juan Pérez"
                    }
                };
            }
            else if (cliente == "Comercial XYZ")
            {
                ServiciosHistorial = new List<ServicioHistorial>
                {
                    new ServicioHistorial
                    {
                        Id = 1002,
                        Cliente = "Comercial XYZ",
                        Observaciones = "Recolección semanal de residuos orgánicos - PENDIENTE",
                        Manifiesto = "MAN-2024-045",
                        TecnicoAsignado = "Roberto Sánchez",
                        OperadorAsignado = "Ana Torres",
                        Vehiculo = "Camión DEF-456",
                        TipoResiduo = "Orgánicos",
                        CantidadEstimada = 300.0,
                        Estado = "Asignado",
                        FechaServicio = DateTime.Today,
                        Direccion = "Calle Comercio 456, Centro",
                        Contrato = "CON-2024-045",
                        Conductor = "María García"
                    },
                    new ServicioHistorial
                    {
                        Id = 1007,
                        Cliente = "Comercial XYZ",
                        Observaciones = "Recolección de residuos orgánicos - Semana 2",
                        Manifiesto = "MAN-2024-047",
                        TecnicoAsignado = "Roberto Sánchez",
                        OperadorAsignado = "Ana Torres",
                        Vehiculo = "Camión DEF-456",
                        TipoResiduo = "Orgánicos",
                        CantidadEstimada = 280.0,
                        Estado = "Concluido",
                        FechaServicio = DateTime.Today.AddDays(-7),
                        Direccion = "Calle Comercio 456, Centro",
                        Contrato = "CON-2024-045",
                        Conductor = "María García"
                    }
                };
            }
            else
            {
                // Datos de ejemplo para otros clientes
                ServiciosHistorial = new List<ServicioHistorial>
                {
                    new ServicioHistorial
                    {
                        Id = 1,
                        Cliente = cliente ?? "Cliente",
                        Observaciones = "Servicio de recolección de residuos - PENDIENTE",
                        Manifiesto = "MAN-2024-XXX",
                        TecnicoAsignado = "Carlos López",
                        OperadorAsignado = "Pedro Ramírez",
                        Vehiculo = "Camión ABC-123",
                        TipoResiduo = "Residuos generales",
                        CantidadEstimada = 100.0,
                        Estado = "Asignado",
                        FechaServicio = DateTime.Today,
                        Direccion = "Dirección del servicio",
                        Contrato = "CON-2024-XXX"
                    },
                    new ServicioHistorial
                    {
                        Id = 2,
                        Cliente = cliente ?? "Cliente",
                        Observaciones = "Servicio de recolección de residuos",
                        Manifiesto = "MAN-2024-XX1",
                        TecnicoAsignado = "Carlos López",
                        OperadorAsignado = "Agusto Ramirez",
                        Vehiculo = "Camión ABC-122",
                        TipoResiduo = "Residuos generales",
                        CantidadEstimada = 100.0,
                        Estado = "En curso",
                        FechaServicio = DateTime.Today,
                        Direccion = "Dirección del servicio",
                        Contrato = "CON-2024-XXX"
                    }
                };
            }
        }
    }
}