using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteWeb.Pages.GenerateInvoice
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string c_Search { get; set; }

        public List<SimulatedService> DisplayedServices { get; set; }
        public bool IsSearchResult { get; set; }
        
        [BindProperty]
        public int? SelectedServiceId { get; set; }

        public SimulatedService SelectedService { get; set; }

        [BindProperty]
        public bool ShowInvoiceModal { get; set; }

        // Receiver Information
        [BindProperty]
        public string TaxId { get; set; } // RFC
        [BindProperty]
        public string BillingName { get; set; } // Name
        [BindProperty]
        public string PostalCode { get; set; } // CP
        [BindProperty]
        public string FiscalRegime { get; set; } // Régimen
        [BindProperty]
        public string CfdiUsage { get; set; }

        // Payment Attributes
        [BindProperty]
        public string PaymentForm { get; set; }
        [BindProperty]
        public string PaymentMethod { get; set; }

        // Concept Details
        [BindProperty]
        public string ProductCode { get; set; }
        [BindProperty]
        public string UnitCode { get; set; }
        [BindProperty]
        public string TaxObject { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        // Simulated Database
        private static readonly List<SimulatedService> _simulatedDb = new List<SimulatedService>
        {
            new SimulatedService { Id = 1, Client = "Juan Pérez", TaxId = "PEPJ800101XYZ", Type = "Mantenimiento", Date = DateTime.Now.AddDays(-2), Amount = 1500.00m },
            new SimulatedService { Id = 2, Client = "Empresa SA de CV", TaxId = "EMP120304QWE", Type = "Soporte Técnico", Date = DateTime.Now.AddDays(-5), Amount = 5000.00m },
            new SimulatedService { Id = 3, Client = "María López", TaxId = "LOMM901212ABC", Type = "Instalación", Date = DateTime.Now.AddDays(-1), Amount = 3200.00m },
            new SimulatedService { Id = 4, Client = "Servicios Integrales", TaxId = "SIN080808DEF", Type = "Mantenimiento", Date = DateTime.Now.AddDays(-8), Amount = 4500.00m }
        };

        public void OnGet()
        {
            LoadRecentServices();
            IsSearchResult = false;
        }

        public void OnPostSearch()
        {
            ShowInvoiceModal = false;

            if (!string.IsNullOrEmpty(c_Search))
            {
                var searchLower = c_Search.ToLower();
                // Simulate DB retrieval filtering by search
                DisplayedServices = _simulatedDb.Where(s => 
                    s.TaxId.ToLower().Contains(searchLower) || 
                    s.Client.ToLower().Contains(searchLower) || 
                    s.Type.ToLower().Contains(searchLower)).ToList();

                IsSearchResult = true;
            }
            else
            {
                LoadRecentServices();
                IsSearchResult = false;
            }
        }

        public void OnPostPrepareInvoice()
        {
            // Persist search view state if needed, or default it to recent
            if (!string.IsNullOrEmpty(c_Search))
            {
                OnPostSearch(); // Reload search results
            }
            else
            {
                LoadRecentServices();
                IsSearchResult = false;
            }
            
            if (SelectedServiceId.HasValue)
            {
                SelectedService = _simulatedDb.FirstOrDefault(s => s.Id == SelectedServiceId.Value);
                if (SelectedService != null)
                {
                    ShowInvoiceModal = true;
                    TaxId = SelectedService.TaxId;
                    BillingName = SelectedService.Client;
                    
                    PostalCode = "12345"; 
                    FiscalRegime = "601"; 
                    CfdiUsage = "G03"; 
                    PaymentMethod = "PUE"; 
                    PaymentForm = "03"; 
                    
                    ProductCode = "80141600"; 
                    UnitCode = "E48"; 
                    TaxObject = "02"; 
                }
            }
        }

        public IActionResult OnPostGenerate()
        {
            SuccessMessage = $"¡La factura para el RFC {TaxId} ({BillingName}) ha sido generada exitosamente! Se ha enviado una notificación automática al CLIENTE.";
            return RedirectToPage();
        }

        public IActionResult OnPostUploadPhysicalInvoice(IFormFile PhysicalInvoice)
        {
            if (PhysicalInvoice != null && PhysicalInvoice.Length > 0)
            {
                if (PhysicalInvoice.ContentType != "application/pdf")
                {
                    SuccessMessage = "Error: El archivo debe ser un PDF.";
                }
                else
                {
                    SuccessMessage = $"¡La factura física '{PhysicalInvoice.FileName}' se ha subido correctamente!";
                }
            }
            else
            {
                SuccessMessage = "Error: No se seleccionó ningún archivo o el archivo está vacío.";
            }

            if (!string.IsNullOrEmpty(c_Search))
            {
                OnPostSearch();
            }
            else
            {
                LoadRecentServices();
                IsSearchResult = false;
            }

            return RedirectToPage();
        }

        private void LoadRecentServices()
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            DisplayedServices = _simulatedDb.Where(s => s.Date >= sevenDaysAgo).ToList();
        }
    }

    public class SimulatedService
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public string TaxId { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
