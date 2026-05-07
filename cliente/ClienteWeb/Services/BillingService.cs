using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ClienteWeb.Models;
using Microsoft.Extensions.Configuration;

namespace ClienteWeb.Services
{
    public class BillingService : IBillingService
    {
        private readonly HttpClient _httpClient;

        public BillingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // The BaseAddress is usually set in Program.cs when configuring the HttpClient
        }

        public async Task<List<BillingResponse>> GetInvoicesAsync(string status = null, string receiverTaxId = null, string searchQuery = null)
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
            if (!string.IsNullOrEmpty(receiverTaxId)) query.Add($"receiver_tax_id={receiverTaxId}");
            if (!string.IsNullOrEmpty(searchQuery)) query.Add($"search_query={searchQuery}");

            string url = "billing/";
            if (query.Count > 0) url += "?" + string.Join("&", query);

            return await _httpClient.GetFromJsonAsync<List<BillingResponse>>(url);
        }

        public async Task<List<ReadyToBill>> GetReadyToBillAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ReadyToBill>>("billing/ready-to-bill");
        }

        public async Task<BillingResponse> GetInvoiceByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<BillingResponse>($"billing/{id}");
        }

        public async Task<BillingResponse> CreateInvoiceAsync(BillingCreate data)
        {
            var response = await _httpClient.PostAsJsonAsync("billing/", data);
            return await response.Content.ReadFromJsonAsync<BillingResponse>();
        }

        public async Task<BillingResponse> UpdateStatusAsync(string id, string status, string reason = null)
        {
            var url = $"billing/{id}/status?new_status={status}";
            if (!string.IsNullOrEmpty(reason)) url += $"&reason={Uri.EscapeDataString(reason)}";

            var response = await _httpClient.PatchAsync(url, null);
            return await response.Content.ReadFromJsonAsync<BillingResponse>();
        }

        public async Task<BillingResponse> UploadPhysicalInvoiceAsync(string id, Stream fileStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync($"billing/{id}/upload", content);
            return await response.Content.ReadFromJsonAsync<BillingResponse>();
        }
    }
}
