using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ContractsService.Models;

public class Contract
{
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    
    [Range(1, int.MaxValue, ErrorMessage = "El ID del cliente es requerido.")]
    public int ClientId { get; set; }

    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal TotalBasePrice { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pendiente de firma";

    public string ClientObjetoSocial { get; set; } = "";
    public string ClientDeclaraciones { get; set; } = "";
    public string ContractDuration { get; set; } = "";
    public DateTime? FirstServiceDate { get; set; }

    public List<ContractServiceItem> Services { get; set; } = new();
    public List<ContractPaymentItem> Payments { get; set; } = new();
    public List<ContractExtra> Extras { get; set; } = new();
}

public class ContractServiceItem { public int Id { get; set; } public int ContractId { get; set; } public string WasteType { get; set; } = ""; public string WasteUnit { get; set; } = ""; public string Frequency { get; set; } = ""; public int Vehicles { get; set; } public int Technicians { get; set; } public string ServiceAddress { get; set; } = ""; public string WarehouseAddress { get; set; } = ""; }
public class ContractPaymentItem { public int Id { get; set; } public int ContractId { get; set; } public string Description { get; set; } = ""; public decimal Amount { get; set; } public DateTime PaymentDate { get; set; } }
public class ContractExtra { public int Id { get; set; } public int ContractId { get; set; } public string Description { get; set; } = ""; public decimal UnitCost { get; set; } public int Quantity { get; set; } }

public class Quotation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    public string Status { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string ClientRfc { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public int ValidityDays { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } 
    
    public string ServicesRawJson { get; set; } = "[]"; 
}