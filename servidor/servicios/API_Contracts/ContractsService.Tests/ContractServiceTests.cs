using ContractsService.Data;
using ContractsService.Services;
using Microsoft.EntityFrameworkCore;
using ContractsService.Models;
using Xunit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContractsService.Tests
{
    public class MockClientesApiService : IClientesApiService
    {
        public ClienteApiDto? MockCliente { get; set; }

        public Task<ClienteApiDto?> BuscarPorRfcAsync(string rfc) => Task.FromResult(MockCliente);
        public Task<ClienteApiDto?> ObtenerPorIdAsync(int id) => Task.FromResult(MockCliente);
        public Task<ClienteApiDto?> CrearClienteAsync(ClienteApiDto cliente, string token) => Task.FromResult(MockCliente);
    }

    public class MockCatalogApiService : ICatalogApiService
    {
        public List<WasteCatalogDto> MockWastes { get; set; } = new();

        public Task<List<WasteCatalogDto>> GetActiveWastesAsync() => Task.FromResult(MockWastes);
    }

    public class ContractServiceTests
    {
        private ContractsDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ContractsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            return new ContractsDbContext(options);
        }

        private async Task SeedDataAsync(ContractsDbContext context)
        {
            context.Contracts.AddRange(
                new Contract 
                { 
                    Id = 1, 
                    Folio = "CON-202604-AAAA", 
                    ClientId = 10, 
                    Status = "Activo", 
                    CreatedAt = DateTime.UtcNow, 
                    TotalBasePrice = 15000m 
                },
                new Contract 
                { 
                    Id = 2, 
                    Folio = "CON-202604-BBBB", 
                    ClientId = 20, 
                    Status = "Vencido", 
                    CreatedAt = DateTime.UtcNow.AddDays(-1), 
                    TotalBasePrice = 25000m 
                },
                new Contract 
                { 
                    Id = 3, 
                    Folio = "CON-202604-CCCC", 
                    ClientId = 10, 
                    Status = "Pendiente de firma", 
                    CreatedAt = DateTime.UtcNow, 
                    TotalBasePrice = 30000m,
                    FirstServiceDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 12, 31)
                }
            );
            await context.SaveChangesAsync();
        }

        // ==========================================
        // CREATE CONTRACT TESTS
        // ==========================================

        [Fact]
        public async Task CreateContractAsync_HappyPath_ReturnsSuccessAndGeneratesFolio()
        {
            var context = GetInMemoryDbContext();
            var clientesMock = new MockClientesApiService();
            var catalogMock = new MockCatalogApiService();
            
            var service = new ContractService(context, clientesMock, catalogMock);
            var newContract = new Contract { ClientId = 101, TotalBasePrice = 15000m };

            var result = await service.CreateContractAsync(newContract, "token123");

            Assert.NotNull(result);
            Assert.StartsWith("CON-", result.Folio);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateContractAsync_WithQuotation_UpdatesQuotationStatus()
        {
            var context = GetInMemoryDbContext();
            var quote = new Quotation { Id = 5, Folio = "Q-001", Status = "approved", ClientRfc = "RFC123" };
            context.Quotations.Add(quote);
            await context.SaveChangesAsync();

            var clientesMock = new MockClientesApiService
            {
                MockCliente = new ClienteApiDto { Id = 101, Rfc = "RFC123", Name = "Test Client" }
            };
            var catalogMock = new MockCatalogApiService();
            
            var service = new ContractService(context, clientesMock, catalogMock);
            var newContract = new Contract { QuotationId = 5, TotalBasePrice = 15000m };

            var result = await service.CreateContractAsync(newContract, "token123");

            Assert.NotNull(result);
            Assert.Equal(101, newContract.ClientId);
            
            var updatedQuote = await context.Quotations.FindAsync(5);
            Assert.Equal("contracted", updatedQuote!.Status);
        }

        [Fact]
        public async Task CreateContractAsync_NullRequest_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            await Assert.ThrowsAsync<NullReferenceException>(() => service.CreateContractAsync(null!, "token"));
        }

        // ==========================================
        // GET CONTRACTS TESTS
        // ==========================================

        [Fact]
        public async Task GetContractsAsync_EmptySearch_ReturnsAllContracts()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var result = await service.GetContractsAsync(null, null, null);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetContractsAsync_SearchByFolio_ReturnsMatch()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var result = await service.GetContractsAsync("AAAA", null, null);

            Assert.Single(result);
            Assert.Equal("CON-202604-AAAA", result.First().Folio);
        }

        [Fact]
        public async Task GetContractsAsync_FilterByStatus_ReturnsMatch()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var result = await service.GetContractsAsync(null, "Vencido", null);

            Assert.Single(result);
            Assert.Equal("Vencido", result.First().Status);
        }

        [Fact]
        public async Task GetContractsAsync_FilterByDate_ReturnsActiveContractsInThatPeriod()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var contract = await context.Contracts.FindAsync(3);
            contract!.Payments.Add(new ContractPaymentItem { PaymentDate = DateTime.UtcNow.AddDays(10), Description = "Pago 1", Amount = 1000m });
            await context.SaveChangesAsync();

            var searchDate = DateTime.UtcNow.AddDays(5);
            var result = await service.GetContractsAsync(null, null, searchDate);

            Assert.Single(result);
            Assert.Equal("CON-202604-CCCC", result.First().Folio);
        }

        // ==========================================
        // GET CONTRACT BY ID TESTS
        // ==========================================

        [Fact]
        public async Task GetContractByIdAsync_ValidId_ReturnsContract()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            
            var clientMock = new MockClientesApiService
            {
                MockCliente = new ClienteApiDto { Id = 10, BusinessName = "Test Business" }
            };
            
            var service = new ContractService(context, clientMock, new MockCatalogApiService());

            var result = await service.GetContractByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("CON-202604-AAAA", result.Folio);
            Assert.Equal("Test Business", result.ClientName);
        }

        [Fact]
        public async Task GetContractByIdAsync_FallbackToQuotation_ReturnsQuotationAsContract()
        {
            var context = GetInMemoryDbContext();
            var quote = new Quotation 
            { 
                Id = 15, 
                Folio = "Q-FALLBACK", 
                ClientName = "Quote Client", 
                ClientRfc = "RFC999",
                ServicesRawJson = "[{\"wastes\":[{\"name\":\"Papel\",\"unit\":\"kg\",\"type\":\"especial\"}]}]"
            };
            context.Quotations.Add(quote);
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var result = await service.GetContractByIdAsync(15);

            Assert.NotNull(result);
            Assert.Equal("Q-FALLBACK", result.Folio);
            Assert.Equal("Quote Client", result.ClientName);
        }

        [Fact]
        public async Task GetContractByIdAsync_NonExistentId_ThrowsKeyNotFoundException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetContractByIdAsync(999));
        }

        // ==========================================
        // GET CONTRACT FULL DETAIL TESTS
        // ==========================================

        [Fact]
        public async Task GetContractFullDetailAsync_ValidId_ReturnsFullDetail()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);

            var clientMock = new MockClientesApiService
            {
                MockCliente = new ClienteApiDto { Id = 10, BusinessName = "Full Client", Address = "Calle 123" }
            };
            var service = new ContractService(context, clientMock, new MockCatalogApiService());

            var result = await service.GetContractFullDetailAsync(1);

            Assert.NotNull(result);
            Assert.Equal("CON-202604-AAAA", result.Folio);
            Assert.Equal("Full Client", result.ClientName);
            Assert.Equal("Calle 123", result.ClientAddress);
        }

        [Fact]
        public async Task GetContractFullDetailAsync_FallbackToQuotation_ReturnsQuotationFullDetail()
        {
            var context = GetInMemoryDbContext();
            var quote = new Quotation 
            { 
                Id = 25, 
                Folio = "Q-FULL", 
                ClientName = "Quote Client", 
                ClientRfc = "RFC999",
                ServicesRawJson = "[{\"location\":{\"street\":\"Calle Principal\",\"municipality\":\"Xalapa\"},\"wastes\":[{\"name\":\"RPBI\",\"unit\":\"kg\",\"type\":\"peligroso\"}]}]"
            };
            context.Quotations.Add(quote);
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var result = await service.GetContractFullDetailAsync(25);

            Assert.NotNull(result);
            Assert.Equal("Q-FULL", result.Folio);
            Assert.Single(result.Services);
            Assert.Equal("RPBI", result.Services.First().WasteType);
            Assert.Equal("Calle Principal, Xalapa", result.Services.First().ServiceAddress);
        }

        // ==========================================
        // UPDATE CONTRACT TESTS
        // ==========================================

        [Fact]
        public async Task UpdateContractAsync_ValidUpdate_UpdatesSecondaryLists()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var updateRequest = new Contract 
            { 
                Status = "Activo", 
                TotalBasePrice = 20000m,
                Services = new List<ContractServiceItem>
                {
                    new ContractServiceItem { Id = 0, WasteType = "RPBI", WasteUnit = "kg", Subtotal = 1000m }
                },
                Payments = new List<ContractPaymentItem>
                {
                    new ContractPaymentItem { Id = 0, Description = "Primer Pago", Amount = 5000m, PaymentDate = DateTime.UtcNow }
                },
                Extras = new List<ContractExtra>
                {
                    new ContractExtra { Id = 0, Description = "Maniobra", UnitCost = 200m, Quantity = 2 }
                }
            };

            var result = await service.UpdateContractAsync(3, updateRequest);

            Assert.Equal("Contrato actualizado exitosamente.", result.Message);

            var updated = await context.Contracts
                .Include(c => c.Services)
                .Include(c => c.Payments)
                .Include(c => c.Extras)
                .FirstAsync(c => c.Id == 3);

            Assert.Equal("Activo", updated.Status);
            Assert.Equal(20000m, updated.TotalBasePrice);
            Assert.Single(updated.Services);
            Assert.Equal("RPBI", updated.Services.First().WasteType);
            Assert.Single(updated.Payments);
            Assert.Equal("Primer Pago", updated.Payments.First().Description);
            Assert.Single(updated.Extras);
            Assert.Equal("Maniobra", updated.Extras.First().Description);
        }

        [Fact]
        public async Task UpdateContractAsync_WithSignedPath_SetsStatusToActivo()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var updateRequest = new Contract
            {
                SignedContractPath = "/uploads/signed/1.pdf",
                TotalBasePrice = 15000m
            };

            await service.UpdateContractAsync(3, updateRequest);

            var updated = await context.Contracts.FindAsync(3);
            Assert.Equal("Activo", updated!.Status);
            Assert.Equal("/uploads/signed/1.pdf", updated.SignedContractPath);
        }

        [Fact]
        public async Task UpdateContractAsync_NonExistentId_ThrowsKeyNotFoundException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var updateRequest = new Contract { Status = "Firmado" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateContractAsync(999, updateRequest));
        }

        // ==========================================
        // GET CONTRACT PDF TESTS
        // ==========================================

        [Fact]
        public async Task GetContractPdfAsync_WhenSignedFileExists_ReturnsFileContent()
        {
            var context = GetInMemoryDbContext();
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Dummy PDF Content");

            var contract = new Contract { Folio = "CON-PDF", SignedContractPath = tempFile };
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            try 
            {
                var result = await service.GetContractPdfAsync(contract.Id);
                Assert.Equal("application/pdf", result.ContentType);
                Assert.Contains("Contrato_Firmado", result.FileName);
                Assert.NotEmpty(result.Content);
            } 
            finally 
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task GetContractPdfAsync_InvalidId_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetContractPdfAsync(999));     
            Assert.Equal("Contract not found", exception.Message); 
        }

        [Fact]
        public async Task GetContractPdfAsync_NegativeId_ThrowsArgumentException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetContractPdfAsync(-1));
            Assert.Equal("ID inválido", exception.Message);
        }

        // ==========================================
        // ADDITIONAL TESTS
        // ==========================================

        [Fact]
        public async Task GetContractsAsync_NullStatus_ExcludesCanceledContracts()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            
            // Add a canceled contract
            context.Contracts.Add(new Contract { Id = 4, Folio = "CON-CANCELED", ClientId = 10, Status = "Cancelado" });
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var result = await service.GetContractsAsync(null, null, null);

            // SeedDataAsync has 3 contracts, and the new one is Canceled, so it should exclude it and return 3.
            Assert.Equal(3, result.Count);
            Assert.All(result, c => Assert.NotEqual("Cancelado", c.Status));
        }

        [Fact]
        public async Task GetContractsAsync_CanceledStatusExplicitly_IncludesCanceledContractsOnly()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            
            // Add a canceled contract
            context.Contracts.Add(new Contract { Id = 4, Folio = "CON-CANCELED", ClientId = 10, Status = "Cancelado" });
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var result = await service.GetContractsAsync(null, "Cancelado", null);

            Assert.Single(result);
            Assert.Equal("CON-CANCELED", result.First().Folio);
            Assert.Equal("Cancelado", result.First().Status);
        }

        [Fact]
        public async Task GetContractFullDetailAsync_NonExistentId_ThrowsKeyNotFoundException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetContractFullDetailAsync(999));
        }

        [Fact]
        public async Task UpdateContractAsync_CancellationRequest_UpdatesStatusAndCancellationReason()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var request = new Contract
            {
                Status = "Cancelado",
                CancellationReason = "Falta de presupuesto"
            };

            var result = await service.UpdateContractAsync(1, request);

            Assert.Equal("Contrato cancelado exitosamente.", result.Message);

            var updated = await context.Contracts.FindAsync(1);
            Assert.Equal("Cancelado", updated!.Status);
            Assert.Equal("Falta de presupuesto", updated.CancellationReason);
        }

        [Fact]
        public async Task UpdateContractAsync_WithStatusCanceladoAndSignedPath_RetainsCanceladoStatus()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());

            var request = new Contract
            {
                Status = "Cancelado",
                CancellationReason = "Mal servicio",
                SignedContractPath = "/uploads/signed/contract.pdf"
            };

            await service.UpdateContractAsync(1, request);

            var updated = await context.Contracts.FindAsync(1);
            Assert.Equal("Cancelado", updated!.Status);
            Assert.Equal("Mal servicio", updated.CancellationReason);
            Assert.Equal("/uploads/signed/contract.pdf", updated.SignedContractPath);
        }

        [Fact]
        public async Task CreateContractAsync_HappyPath_PersistsContractInDb()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var newContract = new Contract { ClientId = 99, TotalBasePrice = 12000m, Representative = "John Doe" };

            await service.CreateContractAsync(newContract, "token");

            var persisted = await context.Contracts.FirstOrDefaultAsync(c => c.ClientId == 99);
            Assert.NotNull(persisted);
            Assert.Equal(12000m, persisted.TotalBasePrice);
            Assert.Equal("John Doe", persisted.Representative);
        }

        [Fact]
        public async Task UpdateContractAsync_WhenContractCanceled_ThrowsInvalidOperationException()
        {
            var context = GetInMemoryDbContext();
            context.Contracts.Add(new Contract { Id = 5, Status = "Cancelado", Folio = "CON-C" });
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var request = new Contract { Status = "Activo" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateContractAsync(5, request));
        }

        [Fact]
        public async Task UpdateContractAsync_WhenActiveAndModified_ThrowsInvalidOperationException()
        {
            var context = GetInMemoryDbContext();
            context.Contracts.Add(new Contract { Id = 6, Status = "Activo", Folio = "CON-A", TotalBasePrice = 1000m });
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var request = new Contract { Status = "Activo", TotalBasePrice = 2000m };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateContractAsync(6, request));
        }

        [Fact]
        public async Task UpdateContractAsync_InvalidDates_ThrowsArgumentException()
        {
            var context = GetInMemoryDbContext();
            context.Contracts.Add(new Contract { Id = 7, Status = "Pendiente de firma", Folio = "CON-D", TotalBasePrice = 1000m });
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var request = new Contract 
            { 
                Status = "Pendiente de firma", 
                TotalBasePrice = 1000m,
                FirstServiceDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 5, 1)
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateContractAsync(7, request));
        }

        [Fact]
        public async Task UpdateContractAsync_InvalidBasePrice_ThrowsArgumentException()
        {
            var context = GetInMemoryDbContext();
            context.Contracts.Add(new Contract { Id = 8, Status = "Pendiente de firma", Folio = "CON-P", TotalBasePrice = 1000m });
            await context.SaveChangesAsync();

            var service = new ContractService(context, new MockClientesApiService(), new MockCatalogApiService());
            var request = new Contract 
            { 
                Status = "Pendiente de firma", 
                TotalBasePrice = -500m
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateContractAsync(8, request));
        }
    }
}