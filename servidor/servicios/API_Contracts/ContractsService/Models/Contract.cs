public class Contract
{
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    public decimal TotalBasePrice { get; set; } 
    
    public List<Anexo1Scope> Anexo1Items { get; set; } = new();
    public List<Anexo2Payment> Anexo2Payments { get; set; } = new();
    public List<Anexo3Schedule> Anexo3Steps { get; set; } = new();
    public List<Anexo4Extra> Anexo4Extras { get; set; } = new();
}

public class Anexo1Scope
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int ExternalResiduoId { get; set; }
    public string NombreResiduo { get; set; } = "";
    public string EstadoFisico { get; set; } = "";
    public string FormaAlmacenado { get; set; } = "";
}

public class Anexo2Payment
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string Concept { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public bool IsBilled { get; set; } = false;
}

public class Anexo3Schedule
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string Phase { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Deliverable { get; set; } = "";
}

public class Anexo4Extra
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int? ExternalResiduoId { get; set; } 
    public string Description { get; set; } = "";
    public decimal UnitCost { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => UnitCost * Quantity;
}