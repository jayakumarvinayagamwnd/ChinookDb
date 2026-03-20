using Chinook.API.Infrastructure.Persistence.Entities.Catalog;

namespace Chinook.API.Infrastructure.Persistence.Entities.Billing;

public class InvoiceLine
{
    public int InvoiceLineId { get; set; }
    public int InvoiceId { get; set; }
    public int TrackId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public Invoice? Invoice { get; set; }
    public Track? Track { get; set; }
}
