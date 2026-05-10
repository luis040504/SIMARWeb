using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ClienteWeb.Services;
using ClienteWeb.Models;
using System.Threading.Tasks;

namespace ClienteWeb.Pages.Billing
{
    public class IndexModel : PageModel
    {
        private readonly IBillingService _billingService;
        private readonly IInvoiceGeneratorService _pdfService;
        private readonly HttpClient _clientesApi;

        public IndexModel(IBillingService billingService, IInvoiceGeneratorService pdfService, IHttpClientFactory factory)
        {
            _billingService = billingService;
            _pdfService = pdfService;
            _clientesApi = factory.CreateClient("ClientesApi");
        }

        public string Role { get; set; } = "Admin";

        [BindProperty(SupportsGet = true)]
        public string ActiveTab { get; set; }

        [BindProperty]
        public string SearchQuery { get; set; }

        [BindProperty]
        public DateTime? DateFilter { get; set; }

        [BindProperty]
        public string StatusFilter { get; set; }

        public List<BillingRecord> DisplayedRecords { get; set; }
        public bool IsSearchResult { get; set; }

        [BindProperty]
        public string SelectedRecordId { get; set; }
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

        public async Task OnGetAsync()
        {
            DetermineRole();
            SetDefaultTab();
            await LoadDataAsync();
            ActiveModal = "None";
        }

        public async Task OnPostSearchAsync()
        {
            DetermineRole();
            SetDefaultTab();
            await LoadDataAsync();
            ActiveModal = "None";
        }

        public async Task OnPostPrepareModalAsync(string modalType)
        {
            ActiveModal = modalType;
            DetermineRole();
            SetDefaultTab();
            await LoadDataAsync(); 
            ActiveModal = modalType; // Re-asegurar después de LoadData si fuera necesario
            if (!string.IsNullOrEmpty(SelectedRecordId))
            {
                SelectedRecord = DisplayedRecords.FirstOrDefault(r => r.Id == SelectedRecordId);
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



        public async Task<IActionResult> OnPostGenerateAsync()
        {
            try
            {
                var items = JsonSerializer.Deserialize<List<InvoiceItemDto>>(ItemsJson);
                var subtotal = items.Sum(i => i.Amount);
                var taxTotal = subtotal * 0.16m;
                var total = subtotal + taxTotal;

                var billingCreate = new BillingCreate
                {
                    UploadType = "DIGITAL",
                    RecordType = "Invoice",
                    Metadata = new InvoiceMetadata { CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, Source = "web_app" },
                    Issuer = new Issuer { TaxId = "SIM120101XYZ", Name = "SIMAR S.A. de C.V.", TaxRegime = "601" },
                    Receiver = new Receiver { TaxId = TaxId, Name = BillingName, PostalCode = PostalCode, FiscalRegime = FiscalRegime, TaxUsage = CfdiUsage },
                    FiscalData = new FiscalData { IssueDate = DateTime.Now },
                    Financials = new Financials { Subtotal = subtotal, TaxTotal = taxTotal, Total = total, PaymentForm = PaymentForm, PaymentMethod = PaymentMethod },
                    Items = items.Select(i => new BillingItem { 
                        Description = i.Concept, 
                        Quantity = i.Quantity, 
                        UnitPrice = i.Amount / (decimal)i.Quantity, 
                        Amount = i.Amount,
                        ProductCode = ProductCode,
                        UnitCode = UnitCode,
                        TaxObject = TaxObject,
                        Taxes = new List<TaxItem> { new TaxItem { Amount = i.Amount * 0.16m } }
                    }).ToList(),
                    Attachments = new Attachments(),
                    Status = "Pending"
                };

                await _billingService.CreateInvoiceAsync(billingCreate);
                StatusMessage = $"¡La prefactura para {BillingName} ha sido generada exitosamente!";
                return RedirectToPage(new { ActiveTab = this.ActiveTab });
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { ActiveTab = this.ActiveTab });
            }
        }

        public async Task<IActionResult> OnPostUploadPhysicalInvoiceAsync(IFormFile PhysicalInvoice, string selectedRecordIdUpload)
        {
            if (PhysicalInvoice != null && PhysicalInvoice.Length > 0 && PhysicalInvoice.ContentType == "application/pdf")
            {
                try
                {
                    await _billingService.UploadPhysicalInvoiceAsync(selectedRecordIdUpload, PhysicalInvoice.OpenReadStream(), PhysicalInvoice.FileName);
                    StatusMessage = $"¡La factura física '{PhysicalInvoice.FileName}' se ha subido correctamente!";
                }
                catch (BillingApiException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Error: El archivo debe ser un PDF válido.";
            }
            return RedirectToPage(new { ActiveTab = this.ActiveTab });
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedRecordId))
                {
                    TempData["ErrorMessage"] = "No se ha seleccionado ninguna factura para editar.";
                    return RedirectToPage(new { ActiveTab = this.ActiveTab });
                }

                var items = JsonSerializer.Deserialize<List<InvoiceItemDto>>(ItemsJson);
                var subtotal = items.Sum(i => i.Amount);
                var taxTotal = subtotal * 0.16m;
                var total = subtotal + taxTotal;

                var billingUpdate = new BillingCreate
                {
                    UploadType = "DIGITAL",
                    RecordType = "Invoice",
                    Metadata = new InvoiceMetadata { UpdatedAt = DateTime.Now, Source = "web_app" },
                    Issuer = new Issuer { TaxId = "SIM120101XYZ", Name = "SIMAR S.A. de C.V.", TaxRegime = "601" },
                    Receiver = new Receiver { TaxId = TaxId, Name = BillingName, PostalCode = PostalCode, FiscalRegime = FiscalRegime, TaxUsage = CfdiUsage },
                    FiscalData = new FiscalData { IssueDate = DateTime.Now },
                    Financials = new Financials { Subtotal = subtotal, TaxTotal = taxTotal, Total = total, PaymentForm = PaymentForm, PaymentMethod = PaymentMethod },
                    Items = items.Select(i => new BillingItem { 
                        Description = i.Concept, 
                        Quantity = (double)i.Quantity, 
                        UnitPrice = i.Amount / (decimal)i.Quantity, 
                        Amount = i.Amount,
                        ProductCode = ProductCode,
                        UnitCode = UnitCode,
                        TaxObject = TaxObject,
                        Taxes = new List<TaxItem> { new TaxItem { Amount = i.Amount * 0.16m } }
                    }).ToList(),
                    Status = "Pending", 
                    Reason = "" 
                };

                await _billingService.UpdateInvoiceAsync(SelectedRecordId, billingUpdate);
                StatusMessage = $"¡La prefactura de {BillingName} ha sido actualizada y reenviada exitosamente!";
                return RedirectToPage(new { ActiveTab = "RejectedInvoices" });
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { ActiveTab = "RejectedInvoices" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado al actualizar: {ex.Message}";
                return RedirectToPage(new { ActiveTab = "RejectedInvoices" });
            }
        }

        public async Task<IActionResult> OnPostAcceptAsync(string id)
        {
            try
            {
                await _billingService.UpdateStatusAsync(id, "Accepted");
                StatusMessage = $"¡La factura ha sido aceptada exitosamente!";
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage(new { ActiveTab = this.ActiveTab });
        }

        public async Task<IActionResult> OnPostRejectAsync(string id, List<string> rejectReasons)
        {
            try
            {
                var reason = string.Join(" / ", rejectReasons);
                await _billingService.UpdateStatusAsync(id, "Rejected", reason);
                StatusMessage = $"La factura ha sido rechazada.";
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage(new { ActiveTab = this.ActiveTab });
        }

        public async Task<IActionResult> OnPostDownloadAsync(string id)
        {
            try
            {
                var invoice = await _billingService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    TempData["ErrorMessage"] = "No se pudo encontrar la factura para descargar.";
                    return RedirectToPage(new { ActiveTab = this.ActiveTab });
                }

                var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
                var fileName = $"Factura_{invoice.FiscalData?.InvoiceFolio ?? invoice.Id}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = $"Error al obtener datos: {ex.Message}";
                return RedirectToPage(new { ActiveTab = this.ActiveTab });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado al generar PDF: {ex.Message}";
                return RedirectToPage(new { ActiveTab = this.ActiveTab });
            }
        }

        private void DetermineRole()
        {
            var sessionRole = HttpContext.Session.GetString("Rol")?.ToLower();
            
            // Roles con permisos administrativos en facturación
            var adminRoles = new[] { "administrador", "admin", "empleado", "contador", "dueño" };

            if (adminRoles.Contains(sessionRole))
            {
                Role = "Admin";
            }
            else
            {
                // Por defecto para clientes o cualquier otro rol restringido
                Role = "Client";
            }
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

        private async Task LoadDataAsync()
        {
            try
            {
                string rfcFilter = null;
                if (Role != "Admin")
                {
                    var userId = HttpContext.Session.GetString("UserId");
                    if (!string.IsNullOrEmpty(userId))
                    {
                        try
                        {
                            var clientInfo = await _clientesApi.GetFromJsonAsync<ClienteOutput>($"client/user/{userId}");
                            if (clientInfo != null)
                            {
                                rfcFilter = clientInfo.RFC;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error fetching client RFC: {ex.Message}");
                        }
                    }
                }

                var allInvoices = await _billingService.GetInvoicesAsync(
                    status: StatusFilter,
                    receiverTaxId: rfcFilter,
                    searchQuery: SearchQuery
                );

                var mappedInvoices = allInvoices.Select(i => new BillingRecord
                {
                    Id = i.Id,
                    RecordType = "Invoice",
                    ClientName = i.Receiver.Name,
                    TaxId = i.Receiver.TaxId,
                    Description = i.Items.FirstOrDefault()?.Description ?? "Factura",
                    Date = i.FiscalData.IssueDate,
                    Amount = i.Financials.Total,
                    InvoiceNumber = i.FiscalData.InvoiceFolio ?? "PENDIENTE",
                    Status = i.Status,
                    Reason = i.Reason,
                    PostalCode = i.Receiver.PostalCode,
                    FiscalRegime = i.Receiver.FiscalRegime,
                    CfdiUsage = i.Receiver.TaxUsage,
                    PaymentForm = i.Financials.PaymentForm,
                    PaymentMethod = i.Financials.PaymentMethod,
                    ProductCode = i.Items.FirstOrDefault()?.ProductCode,
                    UnitCode = i.Items.FirstOrDefault()?.UnitCode,
                    TaxObject = i.Items.FirstOrDefault()?.TaxObject
                });

                var displayed = new List<BillingRecord>();

                if (Role == "Admin")
                {
                    if (ActiveTab == "RecentServices")
                    {
                        var ready = await _billingService.GetReadyToBillAsync();
                        displayed.AddRange(ready.Select(r => new BillingRecord
                        {
                            Id = r.Source == "contract" ? r.NumeroManifiesto : r.ManifestId.ToString(),
                            RecordType = "Service",
                            ClientName = r.Cliente.RazonSocial,
                            TaxId = r.Cliente.Rfc,
                            ServiceType = r.TipoResiduo,
                            Date = r.FechaServicio,
                            Amount = r.TotalEstimado,
                            PostalCode = r.Cliente.PostalCode, // Usando el campo directo
                            Description = r.Source == "contract" ? "Servicio por Contrato" : "Servicio por Manifiesto"
                        }));
                    }
                    else if (ActiveTab == "RejectedInvoices")
                    {
                        displayed.AddRange(mappedInvoices.Where(r => r.Status == "Rejected"));
                    }
                    else if (ActiveTab == "GeneratedInvoices")
                    {
                        displayed.AddRange(mappedInvoices.Where(r => r.Status == "Pending" || r.Status == "Accepted"));
                    }
                }
                else
                {
                    if (ActiveTab == "PendingInvoices")
                    {
                        displayed.AddRange(mappedInvoices.Where(r => r.Status == "Pending"));
                    }
                    else if (ActiveTab == "AcceptedInvoices")
                    {
                        displayed.AddRange(mappedInvoices.Where(r => r.Status == "Accepted"));
                    }
                }

                if (DateFilter.HasValue)
                {
                    displayed = displayed.Where(r => r.Date.Date == DateFilter.Value.Date).ToList();
                }

                DisplayedRecords = displayed.OrderByDescending(r => r.Date).ToList();
                IsSearchResult = !string.IsNullOrEmpty(SearchQuery) || DateFilter.HasValue || !string.IsNullOrEmpty(StatusFilter);
            }
            catch (BillingApiException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                DisplayedRecords = new List<BillingRecord>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar datos de facturación: {ex.Message}";
                DisplayedRecords = new List<BillingRecord>();
            }
        }
    }

    public class BillingRecord
    {
        public string Id { get; set; }
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
        public string Source { get; set; }
    }

    public class InvoiceItemDto
    {
        public string Concept { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
