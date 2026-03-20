using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteWeb.Pages.Contracts.Update
{
    public class UpdateModel : PageModel
    {
        [BindProperty]
        public string Id { get; set; } = "";

        [BindProperty]
        public string Client { get; set; } = "";

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        [BindProperty]
        public string ServiceConditions { get; set; } = "";

        [BindProperty]
        public string AdminObservations { get; set; } = "";

        [BindProperty]
        public IFormFile? PdfFile { get; set; }

        public string Status { get; set; } = "";

        public bool ShowSuccessMessage { get; set; }
        public string ErrorMessage { get; set; } = "";

        public List<string> AuditTrail { get; set; } = new();

        public IActionResult OnGet(string id)
        {
            var contract = GetMockContract(id);
            if (contract == null)
            {
                return RedirectToPage("/Contracts/Consult/Index");
            }

            if (contract.Status == "Vencido" || contract.Status == "Cancelado")
            {
                // Solo permitido para activos o pendientes de firma
                return RedirectToPage("/Contracts/Consult/Index");
            }

            Id = contract.Id;
            Client = contract.Client;
            StartDate = contract.StartDate;
            EndDate = contract.EndDate;
            Status = contract.Status;

            // Valores simulados que vendrían de una base de datos
            ServiceConditions = "Condiciones originales del servicio establecidas en la cláusula segunda...";
            AdminObservations = "Ninguna observación inicial registrada.";

            return Page();
        }

        public IActionResult OnPost()
        {
            // Volver a cargar el contrato para asegurar que nadie modificó el ID o Cliente
            var contract = GetMockContract(Id);
            if (contract == null || contract.Status == "Vencido" || contract.Status == "Cancelado")
            {
                return RedirectToPage("/Contracts/Consult/Index");
            }

            Status = contract.Status; // Mantener estado real

            // Validación principal: Fecha término posterior a inicio
            if (EndDate <= StartDate)
            {
                ErrorMessage = "La nueva fecha de término debe ser posterior a la fecha de inicio.";
                return Page();
            }

            // Simular guardado
            ShowSuccessMessage = true;

            // Generar registro de auditoría
            AuditTrail.Add($"Usuario: Administrador | Fecha de modificación: {DateTime.Now}");
            AuditTrail.Add($"Campos modificados: Fecha de término, Condiciones del servicio, Observaciones administrativas.");

            if (PdfFile != null)
            {
                // Validación simulada del PDF (límite y extensión)
                var extension = System.IO.Path.GetExtension(PdfFile.FileName).ToLower();
                if (extension == ".pdf")
                {
                    AuditTrail.Add($"Archivo adjunto validado: {PdfFile.FileName} (Tamaño: {PdfFile.Length / 1024} KB).");
                }
                else
                {
                    ErrorMessage = "El archivo adjunto debe ser un PDF válido.";
                    ShowSuccessMessage = false;
                    AuditTrail.Clear();
                    return Page();
                }
            }

            return Page();
        }

        private ContractMock? GetMockContract(string id)
        {
            // Simulación de la base de datos (igual que en ConsultModel)
            var data = new List<ContractMock>
            {
                new ContractMock { Id = "CON-001", Client = "Empresa X", StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(12), Status = "Activo" },
                new ContractMock { Id = "CON-002", Client = "Comercial Y", StartDate = DateTime.Today.AddMonths(-2), EndDate = DateTime.Today.AddMonths(10), Status = "Pendiente de firma" },
                new ContractMock { Id = "CON-003", Client = "Industrias Z", StartDate = DateTime.Today.AddYears(-1), EndDate = DateTime.Today.AddMonths(-1), Status = "Vencido" }
            };

            return data.FirstOrDefault(c => c.Id == id);
        }
    }

    public class ContractMock
    {
        public string Id { get; set; } = "";
        public string Client { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "";
    }
}
