using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClienteWeb.Models;

namespace ClienteWeb.Services
{
    public interface IBillingService
    {
        Task<List<BillingResponse>> GetInvoicesAsync(string status = null, string receiverTaxId = null, string searchQuery = null);
        Task<List<ReadyToBill>> GetReadyToBillAsync();
        Task<BillingResponse> GetInvoiceByIdAsync(string id);
        Task<BillingResponse> CreateInvoiceAsync(BillingCreate data);
        Task<BillingResponse> UpdateInvoiceAsync(string id, BillingCreate data);
        Task<BillingResponse> UpdateStatusAsync(string id, string status, string reason = null);
        Task<BillingResponse> UploadPhysicalInvoiceAsync(string id, Stream fileStream, string fileName);
    }
}
