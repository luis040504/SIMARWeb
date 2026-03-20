using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteWeb.Pages.EditInvoice
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string c_Search { get; set; }

        public List<SimulatedInvoice> DisplayedInvoices { get; set; }
        public bool IsSearchResult { get; set; }
        
        [BindProperty]
        public int? SelectedInvoiceId { get; set; }

        public SimulatedInvoice SelectedInvoice { get; set; }

        [BindProperty]
        public bool ShowEditModal { get; set; }

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

        // Simulated Database for Invoices
        private static readonly List<SimulatedInvoice> _simulatedDb = new List<SimulatedInvoice>
        {
            new SimulatedInvoice { Id = 101, InvoiceNumber = "F-1001", Client = "Juan Pérez", TaxId = "PEPJ800101XYZ", Date = DateTime.Now.AddDays(-2), Amount = 1500.00m, Status = "Rejected", Reason = "Código Postal Inválido" },
            new SimulatedInvoice { Id = 102, InvoiceNumber = "F-1002", Client = "Empresa SA de CV", TaxId = "EMP120304QWE", Date = DateTime.Now.AddDays(-5), Amount = 5000.00m, Status = "Paid", Reason = "" },
            new SimulatedInvoice { Id = 103, InvoiceNumber = "F-1003", Client = "María López", TaxId = "LOMM901212ABC", Date = DateTime.Now.AddDays(-1), Amount = 3200.00m, Status = "Rejected", Reason = "Régimen Fiscal Incorrecto" },
            new SimulatedInvoice { Id = 104, InvoiceNumber = "F-1004", Client = "Servicios Integrales", TaxId = "SIN080808DEF", Date = DateTime.Now.AddDays(-8), Amount = 4500.00m, Status = "Pending", Reason = "" }
        };

        public void OnGet()
        {
            LoadRejectedInvoices();
            IsSearchResult = false;
        }

        public void OnPostSearch()
        {
            ShowEditModal = false;

            if (!string.IsNullOrEmpty(c_Search))
            {
                var searchLower = c_Search.ToLower();
                // Simulate DB retrieval filtering by search AND status = Rejected
                DisplayedInvoices = _simulatedDb.Where(i => 
                    i.Status == "Rejected" &&
                    (i.TaxId.ToLower().Contains(searchLower) || 
                     i.Client.ToLower().Contains(searchLower) || 
                     i.InvoiceNumber.ToLower().Contains(searchLower))).ToList();

                IsSearchResult = true;
            }
            else
            {
                LoadRejectedInvoices();
                IsSearchResult = false;
            }
        }

        public void OnPostPrepareEdit()
        {
            // Persist search view state if needed, or default it to recent
            if (!string.IsNullOrEmpty(c_Search))
            {
                OnPostSearch(); // Reload search results
            }
            else
            {
                LoadRejectedInvoices();
                IsSearchResult = false;
            }
            
            if (SelectedInvoiceId.HasValue)
            {
                SelectedInvoice = _simulatedDb.FirstOrDefault(i => i.Id == SelectedInvoiceId.Value);
                if (SelectedInvoice != null)
                {
                    ShowEditModal = true;
                    TaxId = SelectedInvoice.TaxId;
                    BillingName = SelectedInvoice.Client;
                    
                    // Pre-fill with simulated existing data
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

        public IActionResult OnPostEdit()
        {
            SuccessMessage = $"¡La factura de {TaxId} ({BillingName}) ha sido actualizada y reenviada exitosamente!";
            
            // In a real scenario, you would update the database record and change status
            var invoiceToUpdate = _simulatedDb.FirstOrDefault(i => i.Id == SelectedInvoiceId);
            if(invoiceToUpdate != null)
            {
                invoiceToUpdate.Status = "Pending"; // Simulated state change
            }

            return RedirectToPage();
        }

        private void LoadRejectedInvoices()
        {
            DisplayedInvoices = _simulatedDb.Where(i => i.Status == "Rejected").ToList();
        }
    }

    public class SimulatedInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string Client { get; set; }
        public string TaxId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }
}
