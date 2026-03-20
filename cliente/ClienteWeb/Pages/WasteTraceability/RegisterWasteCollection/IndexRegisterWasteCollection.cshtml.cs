using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteCollection
{
    public class ServicioRecoleccion
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public string Direccion { get; set; }
        public string Contrato { get; set; }
        public string Conductor { get; set; }
        public string Vehiculo { get; set; }
        public string Tecnico { get; set; }
        public DateTime FechaServicio { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public string TipoResiduo { get; set; }
        public double CantidadEstimada { get; set; }
        public string Manifiesto { get; set; }
        public string OperadorAsignado { get; set; }
    }

    public class IndexModel : PageModel
    {
        public List<ServicioRecoleccion> Servicios { get; set; }
        public string Rol { get; set; } = "empresa";

        public void OnGet(string rol = "empresa")
        {
            Rol = rol;
            ViewData["Rol"] = rol;
            
            Servicios = new List<ServicioRecoleccion>
            {
                new ServicioRecoleccion
                {
                    Id = 1001,
                    Cliente = "Industrias ABC",
                    Direccion = "Av. Industrial 123, Parque Industrial",
                    Contrato = "CON-2024-001",
                    Conductor = "Juan Pérez",
                    Vehiculo = "Camión ABC-123",
                    Tecnico = "Carlos López",
                    OperadorAsignado = "Pedro Ramírez",
                    FechaServicio = DateTime.Today,
                    Estado = "En curso",
                    Observaciones = "Residuos industriales no peligrosos",
                    TipoResiduo = "Plásticos",
                    CantidadEstimada = 500.5,
                    Manifiesto = "MAN-2024-001"
                },
                new ServicioRecoleccion
                {
                    Id = 1002,
                    Cliente = "Comercial XYZ",
                    Direccion = "Calle Comercio 456, Centro",
                    Contrato = "CON-2024-045",
                    Conductor = "María García",
                    Vehiculo = "Camión DEF-456",
                    Tecnico = "Roberto Sánchez",
                    OperadorAsignado = "Ana Torres",
                    FechaServicio = DateTime.Today,
                    Estado = "En curso",
                    Observaciones = "Recolección semanal de residuos orgánicos",
                    TipoResiduo = "Orgánicos",
                    CantidadEstimada = 300.0,
                    Manifiesto = "MAN-2024-045"
                },
                new ServicioRecoleccion
                {
                    Id = 1003,
                    Cliente = "Hospital del Sur",
                    Direccion = "Av. Salud 789, Colonia Médica",
                    Contrato = "CON-2024-089",
                    Conductor = "Ana Martínez",
                    Vehiculo = "Camión GHI-789",
                    Tecnico = "José Ramírez",
                    OperadorAsignado = "Laura Méndez",
                    FechaServicio = DateTime.Today.AddDays(1),
                    Estado = "En curso",
                    Observaciones = "Residuos biológicos - Manejo especial",
                    TipoResiduo = "Biológicos",
                    CantidadEstimada = 150.75,
                    Manifiesto = "MAN-2024-089"
                },
                new ServicioRecoleccion
                {
                    Id = 1004,
                    Cliente = "Restaurante El Sabor",
                    Direccion = "Calle Principal 321, Zona Centro",
                    Contrato = "CON-2024-112",
                    Conductor = "Pedro González",
                    Vehiculo = "Camión JKL-012",
                    Tecnico = "Miguel Ángel",
                    OperadorAsignado = "Carlos Ruiz",
                    FechaServicio = DateTime.Today,
                    Estado = "Concluido",
                    Observaciones = "Aceites y grasas",
                    TipoResiduo = "Aceites",
                    CantidadEstimada = 50.0,
                    Manifiesto = "MAN-2024-112"
                },
                new ServicioRecoleccion
                {
                    Id = 1005,
                    Cliente = "Constructora Moderna",
                    Direccion = "Blvd. Construcción 567, Zona Industrial",
                    Contrato = "CON-2024-078",
                    Conductor = "Luis Hernández",
                    Vehiculo = "Camión MNO-345",
                    Tecnico = "Fernando Díaz",
                    OperadorAsignado = "Roberto Méndez",
                    FechaServicio = DateTime.Today,
                    Estado = "Concluido",
                    Observaciones = "Escombros y materiales de construcción",
                    TipoResiduo = "Escombros",
                    CantidadEstimada = 1000.0,
                    Manifiesto = "MAN-2024-078"
                }
            };

            if (TempData["MensajeExito"] != null)
            {
                ViewData["MensajeExito"] = TempData["MensajeExito"];
            }

            if (TempData["MensajeError"] != null)
            {
                ViewData["MensajeError"] = TempData["MensajeError"];
                ViewData["TipoError"] = TempData["TipoError"];
            }
        }
    }
}