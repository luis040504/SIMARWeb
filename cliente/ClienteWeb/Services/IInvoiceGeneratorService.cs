using System.Threading.Tasks;
using ClienteWeb.Models;

namespace ClienteWeb.Services
{
    public interface IInvoiceGeneratorService
    {
        byte[] GenerateInvoicePdf(BillingResponse invoice);
    }
}
