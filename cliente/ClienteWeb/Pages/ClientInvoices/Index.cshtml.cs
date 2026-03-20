using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClienteWeb.Pages.ClientInvoices
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } // "Pending" or "Accepted"

        [BindProperty(SupportsGet = true)]
        public int? ViewId { get; set; }

        public ClientInvoice SelectedInvoice { get; set; }

        public List<ClientInvoice> Invoices { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private static List<ClientInvoice> _simulatedDb = new List<ClientInvoice>
        {
            new ClientInvoice { Id = 1, InvoiceNumber = "F-1001", Description = "Servicio de Mantenimiento", Date = DateTime.Now.AddDays(-2), Amount = 1500.00m, Status = "Pending", TaxId = "ABC123456T1", BillingName = "Empresa A SA de CV", PostalCode = "01000", FiscalRegime = "601", CfdiUsage = "G03", PaymentForm = "03", PaymentMethod = "PUE", ProductCode = "80141600", UnitCode = "E48", TaxObject = "02" },
            new ClientInvoice { Id = 2, InvoiceNumber = "F-1002", Description = "Soporte Técnico", Date = DateTime.Now.AddDays(-5), Amount = 5000.00m, Status = "Accepted", TaxId = "DEF789012U2", BillingName = "Cliente B", PostalCode = "02000", FiscalRegime = "612", CfdiUsage = "I04", PaymentForm = "01", PaymentMethod = "PUE", ProductCode = "81111800", UnitCode = "E48", TaxObject = "02" },
            new ClientInvoice { Id = 3, InvoiceNumber = "F-1003", Description = "Instalación", Date = DateTime.Now.AddDays(-10), Amount = 3200.00m, Status = "Pending", TaxId = "GHI345678V3", BillingName = "Comercio C", PostalCode = "03000", FiscalRegime = "601", CfdiUsage = "G01", PaymentForm = "99", PaymentMethod = "PPD", ProductCode = "72151500", UnitCode = "E48", TaxObject = "02" },
            new ClientInvoice { Id = 4, InvoiceNumber = "F-1004", Description = "Asesoría Contable", Date = DateTime.Now.AddDays(-15), Amount = 4500.00m, Status = "Accepted", TaxId = "JKL901234W4", BillingName = "Consultoría D", PostalCode = "04000", FiscalRegime = "626", CfdiUsage = "P01", PaymentForm = "02", PaymentMethod = "PUE", ProductCode = "84111500", UnitCode = "E48", TaxObject = "02" },
            new ClientInvoice { Id = 5, InvoiceNumber = "F-1005", Description = "Licencia de Software", Date = DateTime.Now.AddDays(-1), Amount = 8000.00m, Status = "Pending", TaxId = "MNO567890X5", BillingName = "Tech E", PostalCode = "05000", FiscalRegime = "601", CfdiUsage = "G03", PaymentForm = "04", PaymentMethod = "PPD", ProductCode = "43231500", UnitCode = "E48", TaxObject = "02" }
        };

        public void OnGet()
        {
            if (string.IsNullOrEmpty(Filter))
            {
                Filter = "Pending";
            }
            LoadInvoices();

            if (ViewId.HasValue)
            {
                SelectedInvoice = _simulatedDb.FirstOrDefault(i => i.Id == ViewId.Value);
            }
        }

        public IActionResult OnPostAccept(int id)
        {
            var invoice = _simulatedDb.FirstOrDefault(i => i.Id == id);
            if (invoice != null && invoice.Status == "Pending")
            {
                invoice.Status = "Accepted";
                StatusMessage = $"¡La factura {invoice.InvoiceNumber} ha sido aceptada exitosamente!";
            }
            return RedirectToPage(new { Filter = "Pending" });
        }

        public IActionResult OnPostReject(int id)
        {
            var invoice = _simulatedDb.FirstOrDefault(i => i.Id == id);
            if (invoice != null && invoice.Status == "Pending")
            {
                invoice.Status = "Rejected";
                StatusMessage = $"La factura {invoice.InvoiceNumber} ha sido rechazada.";
            }
            return RedirectToPage(new { Filter = "Pending" });
        }

        public IActionResult OnPostDownload(int id)
        {
            var invoice = _simulatedDb.FirstOrDefault(i => i.Id == id);
            if (invoice != null && invoice.Status == "Accepted")
            {
                StatusMessage = $"Iniciando descarga de la factura {invoice.InvoiceNumber}... (Simulación)";
            }
            return RedirectToPage(new { Filter = "Accepted" });
        }

        private void LoadInvoices()
        {
            Invoices = _simulatedDb.Where(i => i.Status == Filter).OrderByDescending(i => i.Date).ToList();
        }
    }

    public class ClientInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // "Pending", "Accepted", "Rejected"

        // Creation Data
        public string TaxId { get; set; }
        public string BillingName { get; set; }
        public string PostalCode { get; set; }
        public string FiscalRegime { get; set; }
        public string CfdiUsage { get; set; }
        public string PaymentForm { get; set; }
        public string PaymentMethod { get; set; }
        public string ProductCode { get; set; }
        public string UnitCode { get; set; }
        public string TaxObject { get; set; }
    }
}
