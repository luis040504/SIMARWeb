using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ClienteWeb.Services;

namespace ClienteWeb.Pages.Billing
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Role { get; set; } = "Admin";

        [BindProperty(SupportsGet = true)]
        public string ActiveTab { get; set; }

        [BindProperty]
        public string SearchQuery { get; set; }

        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        [BindProperty]
        public string StatusFilter { get; set; }

        public List<BillingRecord> DisplayedRecords { get; set; }
        public bool IsSearchResult { get; set; }

        [BindProperty]
        public int? SelectedRecordId { get; set; }
        public BillingRecord SelectedRecord { get; set; }

        [BindProperty]
        public string ActiveModal { get; set; } = "None"; // "Generate", "Edit", "Details"

        // Form Fields for Generate / Edit
        [BindProperty] public string TaxId { get; set; }
        [BindProperty] public string BillingName { get; set; }
        [BindProperty] public string PostalCode { get; set; }
        [BindProperty] public string FiscalRegime { get; set; }
        [BindProperty] public string CfdiUsage { get; set; }
        [BindProperty] public string PaymentForm { get; set; }
        [BindProperty] public string PaymentMethod { get; set; }
        [BindProperty] public string ProductCode { get; set; }
        [BindProperty] public string UnitCode { get; set; }
        [BindProperty] public string TaxObject { get; set; }
        [BindProperty] public string ItemsJson { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private static List<BillingRecord> _simulatedDb = new List<BillingRecord>
        {
            new BillingRecord { Id = 1, RecordType = "Service", ClientName = "Juan Pérez", TaxId = "PEPJ800101XYZ", ServiceType = "Recolección de RPBI", Date = DateTime.Now.AddDays(-2), Amount = 1500.00m },
            new BillingRecord { Id = 2, RecordType = "Service", ClientName = "Empresa SA de CV", TaxId = "EMP120304QWE", ServiceType = "Manejo Especial de Químicos", Date = DateTime.Now.AddDays(-5), Amount = 5000.00m },
            new BillingRecord { Id = 3, RecordType = "Invoice", InvoiceNumber = "F-1001", Status = "Rejected", Reason = "Código Postal Inválido", ClientName = "Juan Pérez", TaxId = "PEPJ800101XYZ", Description = "Tratamiento de Residuos Peligrosos", Date = DateTime.Now.AddDays(-2), Amount = 1500.00m, PostalCode = "01000", FiscalRegime = "601", CfdiUsage = "G03", PaymentForm = "03", PaymentMethod = "PUE", ProductCode = "80141600", UnitCode = "E48", TaxObject = "02" },
            new BillingRecord { Id = 4, RecordType = "Invoice", InvoiceNumber = "F-1003", Status = "Rejected", Reason = "Régimen Fiscal Incorrecto", ClientName = "María López", TaxId = "LOMM901212ABC", Description = "Transporte de Manejo Especial", Date = DateTime.Now.AddDays(-1), Amount = 3200.00m, PostalCode = "03000", FiscalRegime = "601", CfdiUsage = "G01", PaymentForm = "99", PaymentMethod = "PPD", ProductCode = "72151500", UnitCode = "E48", TaxObject = "02" },
            new BillingRecord { Id = 5, RecordType = "Invoice", InvoiceNumber = "F-1004", Status = "Pending", ClientName = "Comercio C", TaxId = "GHI345678V3", Description = "Recolección de Residuos Peligrosos", Date = DateTime.Now.AddDays(-10), Amount = 3200.00m, PostalCode = "03000", FiscalRegime = "601", CfdiUsage = "G01", PaymentForm = "99", PaymentMethod = "PPD", ProductCode = "72151500", UnitCode = "E48", TaxObject = "02" },
            new BillingRecord { Id = 6, RecordType = "Invoice", InvoiceNumber = "F-1005", Status = "Accepted", ClientName = "Consultoría D", TaxId = "JKL901234W4", Description = "Incineración de Desechos", Date = DateTime.Now.AddDays(-15), Amount = 4500.00m, PostalCode = "04000", FiscalRegime = "626", CfdiUsage = "P01", PaymentForm = "02", PaymentMethod = "PUE", ProductCode = "84111500", UnitCode = "E48", TaxObject = "02" }
        };

        public void OnGet()
        {
            SetDefaultTab();
            LoadData();
            ActiveModal = "None";
        }

        public void OnPostSearch()
        {
            SetDefaultTab();
            var filtered = GetBaseQuery();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(r => 
                    (r.TaxId != null && r.TaxId.ToLower().Contains(query)) || 
                    (r.ClientName != null && r.ClientName.ToLower().Contains(query)) || 
                    (r.InvoiceNumber != null && r.InvoiceNumber.ToLower().Contains(query))
                );
            }

            if (StartDate.HasValue)
            {
                filtered = filtered.Where(r => r.Date.Date >= StartDate.Value.Date);
            }

            if (EndDate.HasValue)
            {
                filtered = filtered.Where(r => r.Date.Date <= EndDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                filtered = filtered.Where(r => r.Status == StatusFilter);
            }

            DisplayedRecords = filtered.OrderByDescending(r => r.Date).ToList();
            IsSearchResult = !string.IsNullOrEmpty(SearchQuery) || StartDate.HasValue || EndDate.HasValue || !string.IsNullOrEmpty(StatusFilter);
            
            if (!IsSearchResult)
            {
                LoadData();
            }
            ActiveModal = "None";
        }

        public void OnPostPrepareModal(string modalType)
        {
            ActiveModal = modalType;
            SetDefaultTab();
            LoadData(); 

            if (SelectedRecordId.HasValue)
            {
                SelectedRecord = _simulatedDb.FirstOrDefault(r => r.Id == SelectedRecordId.Value);
                if (SelectedRecord != null)
                {
                    TaxId = SelectedRecord.TaxId;
                    BillingName = SelectedRecord.ClientName;
                    PostalCode = SelectedRecord.PostalCode ?? "12345";
                    FiscalRegime = SelectedRecord.FiscalRegime ?? "601";
                    CfdiUsage = SelectedRecord.CfdiUsage ?? "G03";
                    PaymentMethod = SelectedRecord.PaymentMethod ?? "PUE";
                    PaymentForm = SelectedRecord.PaymentForm ?? "03";
                    ProductCode = SelectedRecord.ProductCode ?? "80141600";
                    UnitCode = SelectedRecord.UnitCode ?? "E48";
                    TaxObject = SelectedRecord.TaxObject ?? "02";
                    
                    var initialItems = new List<InvoiceItemDto>
                    {
                        new InvoiceItemDto 
                        { 
                            Concept = SelectedRecord.ServiceType ?? SelectedRecord.Description ?? "Servicio", 
                            Quantity = 1,
                            Amount = SelectedRecord.Amount 
                        }
                    };
                    ItemsJson = JsonSerializer.Serialize(initialItems);
                }
            }
        }

        public IActionResult OnPostChangeRole(string newRole)
        {
            return RedirectToPage(new { Role = newRole });
        }

        public IActionResult OnPostGenerate()
        {
            try
            {
                // Aquí iría la llamada real al HttpClient de API_Billing que usará el interceptor:
                // await _httpClient.PostAsJsonAsync("/billing", dto);
                
                // Simulación de error de red para propósitos de prueba
                if (TaxId == "ERROR1234567") 
                {
                    throw new BillingApiException("Error de validación de datos: El RFC proporcionado no se encuentra en los registros del SAT.", 422);
                }

                StatusMessage = $"¡La factura para el RFC {TaxId} ha sido generada exitosamente!";
                return RedirectToPage(new { Role = this.Role, ActiveTab = this.ActiveTab });
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { Role = this.Role, ActiveTab = this.ActiveTab });
            }
        }

        public IActionResult OnPostUploadPhysicalInvoice(IFormFile PhysicalInvoice, int selectedRecordIdUpload)
        {
            if (PhysicalInvoice != null && PhysicalInvoice.Length > 0 && PhysicalInvoice.ContentType == "application/pdf")
            {
                StatusMessage = $"¡La factura física '{PhysicalInvoice.FileName}' se ha subido correctamente!";
            }
            else
            {
                StatusMessage = "Error: El archivo debe ser un PDF válido.";
            }
            return RedirectToPage(new { Role = this.Role, ActiveTab = this.ActiveTab });
        }

        public IActionResult OnPostEdit()
        {
            try
            {
                // Aquí iría la llamada real al HttpClient de API_Billing
                
                StatusMessage = $"¡La factura de {TaxId} ha sido actualizada y reenviada exitosamente!";
                var invoice = _simulatedDb.FirstOrDefault(i => i.Id == SelectedRecordId);
                if(invoice != null) invoice.Status = "Pending";
                return RedirectToPage(new { Role = this.Role, ActiveTab = "RejectedInvoices" });
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { Role = this.Role, ActiveTab = "RejectedInvoices" });
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
            return RedirectToPage(new { Role = this.Role, ActiveTab = this.ActiveTab });
        }

        public IActionResult OnPostReject(int id, List<string> rejectReasons)
        {
            var invoice = _simulatedDb.FirstOrDefault(i => i.Id == id);
            if (invoice != null && invoice.Status == "Pending")
            {
                invoice.Status = "Rejected";
                invoice.Reason = string.Join(" / ", rejectReasons);
                StatusMessage = $"La factura {invoice.InvoiceNumber} ha sido rechazada.";
            }
            return RedirectToPage(new { Role = this.Role, ActiveTab = this.ActiveTab });
        }

        public IActionResult OnPostDownload(int id)
        {
            var invoice = _simulatedDb.FirstOrDefault(i => i.Id == id);
            if (invoice != null)
            {
                StatusMessage = $"Iniciando descarga de la factura {invoice.InvoiceNumber}... (Simulación)";
            }
            return RedirectToPage(new { Role = this.Role, ActiveTab = this.ActiveTab });
        }

        public string GetDisplayName_FiscalRegime(string code) => code switch
        {
            "601" => "601 - General Ley Personas Morales",
            "612" => "612 - Personas Físicas con Actividades",
            "626" => "626 - RESICO",
            _ => code ?? "N/A"
        };

        public string GetDisplayName_CfdiUsage(string code) => code switch
        {
            "G01" => "G01 - Adquisición mercancías",
            "G03" => "G03 - Gastos en general",
            "P01" => "P01 - Por definir",
            _ => code ?? "N/A"
        };

        public string GetDisplayName_PaymentForm(string code) => code switch
        {
            "01" => "01 - Efectivo",
            "02" => "02 - Cheque nominativo",
            "03" => "03 - Transferencia",
            "04" => "04 - Tarjeta Crédito",
            "99" => "99 - Por definir",
            _ => code ?? "N/A"
        };

        public string GetDisplayName_PaymentMethod(string code) => code switch
        {
            "PUE" => "PUE - Pago una exhibición",
            "PPD" => "PPD - Pago diferido",
            _ => code ?? "N/A"
        };

        private void SetDefaultTab()
        {
            if (Role == "Admin")
            {
                if (ActiveTab != "RecentServices" && ActiveTab != "RejectedInvoices" && ActiveTab != "GeneratedInvoices") ActiveTab = "RecentServices";
            }
            else
            {
                if (ActiveTab != "PendingInvoices" && ActiveTab != "AcceptedInvoices") ActiveTab = "PendingInvoices";
            }
        }

        private IEnumerable<BillingRecord> GetBaseQuery()
        {
            if (Role == "Admin")
            {
                if (ActiveTab == "RecentServices") return _simulatedDb.Where(r => r.RecordType == "Service");
                if (ActiveTab == "RejectedInvoices") return _simulatedDb.Where(r => r.RecordType == "Invoice" && r.Status == "Rejected");
                if (ActiveTab == "GeneratedInvoices") return _simulatedDb.Where(r => r.RecordType == "Invoice" && (r.Status == "Pending" || r.Status == "Accepted"));
            }
            else
            {
                if (ActiveTab == "PendingInvoices") return _simulatedDb.Where(r => r.RecordType == "Invoice" && r.Status == "Pending");
                if (ActiveTab == "AcceptedInvoices") return _simulatedDb.Where(r => r.RecordType == "Invoice" && r.Status == "Accepted");
            }
            return new List<BillingRecord>();
        }

        private void LoadData()
        {
            DisplayedRecords = GetBaseQuery().OrderByDescending(r => r.Date).ToList();
            IsSearchResult = false;
        }
    }

    public class BillingRecord
    {
        public int Id { get; set; }
        public string RecordType { get; set; } 
        public string ClientName { get; set; }
        public string TaxId { get; set; }
        public string ServiceType { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string InvoiceNumber { get; set; }
        public string Status { get; set; } 
        public string Reason { get; set; }
        public string Description { get; set; }
        public string PostalCode { get; set; }
        public string FiscalRegime { get; set; }
        public string CfdiUsage { get; set; }
        public string PaymentForm { get; set; }
        public string PaymentMethod { get; set; }
        public string ProductCode { get; set; }
        public string UnitCode { get; set; }
        public string TaxObject { get; set; }
    }

    public class InvoiceItemDto
    {
        public string Concept { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
