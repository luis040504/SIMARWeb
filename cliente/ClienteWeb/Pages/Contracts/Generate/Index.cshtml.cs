using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Contracts.Generate
{
    public class GenerateModel : PageModel
    {
        [BindProperty]
        public int QuotationId { get; set; }

        public List<Quotation> Quotations { get; set; } = new();

        [BindProperty] public string BusinessName { get; set; } = "";
        [BindProperty] public string RFC { get; set; } = "";
        [BindProperty] public string Address { get; set; } = "";
        [BindProperty] public string Representative { get; set; } = "";
        [BindProperty] public string ServiceDetails { get; set; } = "";
        [BindProperty] public decimal Price { get; set; }
        [BindProperty] public string PaymentMethod { get; set; } = "";
        [BindProperty] public string Validity { get; set; } = "";

        public bool ShowPreview { get; set; }

        public void OnGet()
        {
            LoadQuotations();
        }

        public void OnPost(string action)
        {
            LoadQuotations();

            if (action == "preview")
            {
                var quotation = GetMockQuotation(QuotationId);
                if (quotation != null)
                {
                    BusinessName = quotation.BusinessName;
                    RFC = quotation.RFC;
                    Address = quotation.Address;
                    Representative = quotation.Representative;
                    ServiceDetails = quotation.ServiceDetails;
                    Price = quotation.Price;
                    PaymentMethod = quotation.PaymentMethod;
                    Validity = quotation.Validity;
                    ShowPreview = true;
                }
            }
            else if (action == "cancel")
            {
                ShowPreview = false;
            }
        }

        private void LoadQuotations()
        {
            Quotations = new List<Quotation>
            {
                new Quotation { Id = 1, Name = "Cotización Empresa X" },
                new Quotation { Id = 2, Name = "Cotización Comercial Y" }
            };
        }

        private QuotationDetail? GetMockQuotation(int id)
        {
            var data = new List<QuotationDetail>
            {
                new QuotationDetail {
                    Id = 1, BusinessName = "Empresa X", RFC = "XAXX010101000", 
                    Address = "Av. Principal 123, Xalapa, Ver.", 
                    Representative = "JUAN PÉREZ LÓPEZ",
                    ServiceDetails = "Recolección de residuos peligrosos biológico-infecciosos", 
                    Price = 12500, PaymentMethod = "Transferencia bancaria", Validity = "12 meses"
                },
                new QuotationDetail {
                    Id = 2, BusinessName = "Comercial Y", RFC = "YAYY020202000", 
                    Address = "Calle Secundaria 456, Veracruz, Ver.", 
                    Representative = "MARÍA GARCÍA SOLÍS",
                    ServiceDetails = "Manejo de residuos industriales no peligrosos", 
                    Price = 8400, PaymentMethod = "Efectivo / Cheque", Validity = "6 meses"
                }
            };
            return data.FirstOrDefault(q => q.Id == id);
        }
    }

    public class Quotation { public int Id { get; set; } public string Name { get; set; } = ""; }
    public class QuotationDetail {
        public int Id { get; set; }
        public string BusinessName { get; set; } = "";
        public string RFC { get; set; } = "";
        public string Address { get; set; } = "";
        public string Representative { get; set; } = "";
        public string ServiceDetails { get; set; } = "";
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Validity { get; set; } = "";
    }
}