public class Contract
{
    public int Id { get; set; }

    public string Folio { get; set; } = "";

    public string Client { get; set; } = "";
    public string RFC { get; set; } = "";
    public string Address { get; set; } = "";
    public string Representative { get; set; } = "";

    public string ServiceDetails { get; set; } = "";
    public decimal Price { get; set; }
    public string PaymentMethod { get; set; } = "";

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string Status { get; set; } = "";

    public string ServiceConditions { get; set; } = "";
    public string AdminObservations { get; set; } = "";

    public string? PdfUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}