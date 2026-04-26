using ContractsService.Data;
using ContractsService.Services;
using Microsoft.EntityFrameworkCore;
using ContractsService.Models;
using Xunit;

namespace ContractsService.Tests
{
    public class ContractServiceTests
    {
        private ContractsDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ContractsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            return new ContractsDbContext(options);
        }

        [Fact]
        public async Task CreateContractAsync_HappyPath_ReturnsSuccessAndGeneratesFolio()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context);
            var newContract = new Contract { ClientId = 101, TotalBasePrice = 15000m };

            var result = await service.CreateContractAsync(newContract);

            Assert.NotNull(result);
            Assert.StartsWith("CON-", result.Folio);
            Assert.True(result.Id > 0);

            var auditLog = await context.AuditLogs.FirstOrDefaultAsync();
            Assert.NotNull(auditLog);
            Assert.Contains(result.Folio, auditLog.Details);
        }

        [Fact]
        public async Task CreateContractAsync_NullRequest_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context);

            await Assert.ThrowsAsync<NullReferenceException>(() => service.CreateContractAsync(null!));
        }

        private async Task SeedDataAsync(ContractsDbContext context)
        {
            context.Contracts.AddRange(
                new Contract { Id = 1, Folio = "CON-202604-AAAA", ClientId = 10, Status = "Activo", CreatedAt = DateTime.Now },
                new Contract { Id = 2, Folio = "CON-202604-BBBB", ClientId = 20, Status = "Vencido", CreatedAt = DateTime.Now.AddDays(-1) },
                new Contract { 
                    Id = 3, 
                    Folio = "CON-202604-CCCC", 
                    ClientId = 10, 
                    Status = "Pendiente de firma",
                    Anexo3Steps = new List<Anexo3Schedule> { 
                        new Anexo3Schedule { StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) } 
                    }
                }
            );
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetContractsAsync_EmptySearch_ReturnsAllContracts()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var result = await service.GetContractsAsync(null, null, null);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetContractsAsync_SearchByFolio_ReturnsMatch()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var result = await service.GetContractsAsync("AAAA", null, null);

            Assert.Single(result);
            Assert.Equal("CON-202604-AAAA", result.First().Folio);
        }

        [Fact]
        public async Task GetContractsAsync_FilterByStatus_WhenSearchActive_ReturnsMatch()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var result = await service.GetContractsAsync("10", "Activo", null);

            Assert.Single(result);
            Assert.Equal("Activo", result.First().Status);
            Assert.Equal(10, result.First().ClientId);
        }

        [Fact]
        public async Task GetContractsAsync_FilterByDate_ReturnsActiveContractsInThatPeriod()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var searchDate = new DateTime(2026, 6, 15);
            var result = await service.GetContractsAsync("10", null, searchDate);

            Assert.Single(result);
            Assert.Equal("CON-202604-CCCC", result.First().Folio);
        }

        [Fact]
        public async Task GetContractsAsync_NoMatches_ReturnsEmptyList()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var result = await service.GetContractsAsync("CLIENTE_GHOST", null, null);

            Assert.Empty(result);
        }
        
        [Fact]
        public async Task GetContractPdfAsync_ValidId_ReturnsPdfBuffer()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var result = await service.GetContractPdfAsync(1);

            Assert.NotNull(result.Content);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Contains(".pdf", result.FileName);
        }

        [Fact]
        public async Task GetContractPdfAsync_InvalidId_ThrowsException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context);

            var exception = await Assert.ThrowsAsync<Exception>(() => service.GetContractPdfAsync(999));
            Assert.Equal("Contract not found", exception.Message);
        }
    }
}