using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ClienteWeb.Models
{
    public class InvoiceMetadata
    {
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class Issuer
    {
        [JsonPropertyName("tax_id")]
        public string TaxId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tax_regime")]
        public string TaxRegime { get; set; }
    }

    public class Receiver
    {
        [JsonPropertyName("tax_id")]
        public string TaxId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("tax_usage")]
        public string TaxUsage { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("fiscal_regime")]
        public string FiscalRegime { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
    }

    public class FiscalData
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("invoice_series")]
        public string InvoiceSeries { get; set; }

        [JsonPropertyName("invoice_folio")]
        public string InvoiceFolio { get; set; }

        [JsonPropertyName("issue_date")]
        public DateTime IssueDate { get; set; }

        [JsonPropertyName("certification_date")]
        public DateTime? CertificationDate { get; set; }

        [JsonPropertyName("cfdi_version")]
        public string CfdiVersion { get; set; }
    }

    public class Financials
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "MXN";

        [JsonPropertyName("exchange_rate")]
        public double ExchangeRate { get; set; } = 1.0;

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("discount")]
        public decimal Discount { get; set; } = 0.0m;

        [JsonPropertyName("tax_total")]
        public decimal TaxTotal { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonPropertyName("payment_form")]
        public string PaymentForm { get; set; }
    }

    public class TaxItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "IVA";

        [JsonPropertyName("rate")]
        public double Rate { get; set; } = 0.16;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class BillingItem
    {
        [JsonPropertyName("product_code")]
        public string ProductCode { get; set; }

        [JsonPropertyName("unit_code")]
        public string UnitCode { get; set; }

        [JsonPropertyName("tax_object")]
        public string TaxObject { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("taxes")]
        public List<TaxItem> Taxes { get; set; } = new List<TaxItem>();
    }

    public class Attachments
    {
        [JsonPropertyName("xml_url")]
        public string XmlUrl { get; set; }

        [JsonPropertyName("pdf_url")]
        public string PdfUrl { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }

    public class BillingBase
    {
        [JsonPropertyName("upload_type")]
        public string UploadType { get; set; }

        [JsonPropertyName("record_type")]
        public string RecordType { get; set; } = "Invoice";

        [JsonPropertyName("service_type")]
        public string ServiceType { get; set; }

        [JsonPropertyName("metadata")]
        public InvoiceMetadata Metadata { get; set; }

        [JsonPropertyName("issuer")]
        public Issuer Issuer { get; set; }

        [JsonPropertyName("receiver")]
        public Receiver Receiver { get; set; }

        [JsonPropertyName("fiscal_data")]
        public FiscalData FiscalData { get; set; }

        [JsonPropertyName("financials")]
        public Financials Financials { get; set; }

        [JsonPropertyName("items")]
        public List<BillingItem> Items { get; set; }

        [JsonPropertyName("attachments")]
        public Attachments Attachments { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("activo")]
        public bool Activo { get; set; } = true;
    }

    public class BillingResponse : BillingBase
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
    }

    public class BillingCreate : BillingBase { }

    // Aggregator Models
    public class ClientSummary
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("razon_social")]
        public string RazonSocial { get; set; }

        [JsonPropertyName("rfc")]
        public string Rfc { get; set; }

        [JsonPropertyName("direccion_fiscal")]
        public string DireccionFiscal { get; set; }
    }

    public class ContractSummary
    {
        [JsonPropertyName("folio")]
        public string Folio { get; set; }

        [JsonPropertyName("precio_unitario")]
        public double PrecioUnitario { get; set; }

        [JsonPropertyName("metodo_pago")]
        public string MetodoPago { get; set; }

        [JsonPropertyName("condiciones")]
        public string Condiciones { get; set; }
    }

    public class ResidueDetail
    {
        [JsonPropertyName("residuo")]
        public string Residuo { get; set; }

        [JsonPropertyName("cantidad")]
        public double Cantidad { get; set; }

        [JsonPropertyName("unidad")]
        public string Unidad { get; set; }
    }

    public class ReadyToBill
    {
        [JsonPropertyName("manifest_id")]
        public int ManifestId { get; set; }

        [JsonPropertyName("numero_manifiesto")]
        public string NumeroManifiesto { get; set; }

        [JsonPropertyName("fecha_servicio")]
        public DateTime FechaServicio { get; set; }

        [JsonPropertyName("tipo_residuo")]
        public string TipoResiduo { get; set; }

        [JsonPropertyName("cliente")]
        public ClientSummary Cliente { get; set; }

        [JsonPropertyName("contrato")]
        public ContractSummary Contrato { get; set; }

        [JsonPropertyName("detalles_servicio")]
        public List<ResidueDetail> DetallesServicio { get; set; }

        [JsonPropertyName("total_estimado")]
        public decimal TotalEstimado { get; set; }
    }
}
