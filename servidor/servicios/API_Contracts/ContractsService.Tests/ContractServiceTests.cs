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

        [Fact]
        public async Task CreateContractAsync_InvalidDates_ThrowsArgumentException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context);
            var invalidContract = new Contract 
            { 
                ClientId = 101, 
                Anexo3Steps = new List<Anexo3Schedule> { 
                    new Anexo3Schedule { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(-5) }
                }
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateContractAsync(invalidContract));
            Assert.Contains("La fecha de fin no puede ser anterior a la de inicio", exception.Message);
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

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetContractPdfAsync(999));     
            Assert.Equal("Contract not found", exception.Message); 
        }

        [Fact]
        public async Task GetContractPdfAsync_NegativeId_ThrowsArgumentException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetContractPdfAsync(-1));
            Assert.Equal("ID inválido", exception.Message);
        }

        [Fact]
        public async Task GetContractByIdAsync_ValidId_ReturnsContract()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var result = await service.GetContractByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("CON-202604-AAAA", result.Folio);
        }

        [Fact]
        public async Task GetContractByIdAsync_NonExistentId_ThrowsKeyNotFoundException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetContractByIdAsync(999));
        }

        [Fact]
        public async Task UpdateContractAsync_ValidUpdate_UpdatesAndLogs()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);
            var updateRequest = new Contract { Status = "Firmado", TotalBasePrice = 20000m };

            var result = await service.UpdateContractAsync(1, updateRequest);

            Assert.Equal("Contrato actualizado exitosamente.", result.Message);
            var updated = await context.Contracts.FindAsync(1);
            Assert.Equal("Firmado", updated!.Status);
            Assert.Equal(20000m, updated.TotalBasePrice);

            var log = await context.AuditLogs.OrderByDescending(l => l.Timestamp).FirstAsync();
            Assert.Equal("Update Contract Full", log.Action);
            Assert.Contains("(incluyendo anexos)", log.Details);
        }

        [Fact]
        public async Task UpdateContractAsync_NonExistentId_ThrowsKeyNotFoundException()
        {
            var context = GetInMemoryDbContext();
            var service = new ContractService(context);
            var updateRequest = new Contract { Status = "Firmado" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateContractAsync(999, updateRequest));
        }

        [Fact]
        public async Task UpdateContractAsync_UpdateAnnexItems_SyncsCorrectly()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var existing = await context.Contracts
                .Include(c => c.Anexo1Items)
                .FirstAsync(c => c.Id == 3);
            
            var updateRequest = new Contract
            {
                Status = "Firmado",
                Anexo1Items = new List<Anexo1Scope>
                {
                    new Anexo1Scope { Id = 0, NombreResiduo = "Nuevo Residuo" }
                }
            };

            await service.UpdateContractAsync(3, updateRequest);

            var updated = await context.Contracts
                .Include(c => c.Anexo1Items)
                .FirstAsync(c => c.Id == 3);

            Assert.Single(updated.Anexo1Items);
            Assert.Equal("Nuevo Residuo", updated.Anexo1Items.First().NombreResiduo);
        }

        [Fact]
        public async Task UpdateContractAsync_InvalidAnexo3Dates_ThrowsArgumentException()
        {
            var context = GetInMemoryDbContext();
            await SeedDataAsync(context);
            var service = new ContractService(context);

            var updateRequest = new Contract
            {
                Anexo3Steps = new List<Anexo3Schedule>
                {
                    new Anexo3Schedule { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(-1) }
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateContractAsync(3, updateRequest));
        }
    }
}