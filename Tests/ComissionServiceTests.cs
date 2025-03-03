using Moq;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Application.Services;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Infrastructure.Repository;
using Xunit;

namespace PaymentsMS.Tests
{
    public class ComissionServiceTests
    {
        private readonly Mock<ICreateRepository<Comissions>> _mockComissionsCreateRepository;
        private readonly IComissionService _comissionService;
        private const double PERCENT_COMISSION = 0.10;

        public ComissionServiceTests()
        {
            _mockComissionsCreateRepository = new Mock<ICreateRepository<Comissions>>();
            _comissionService = new ComissionService(_mockComissionsCreateRepository.Object);
        }

        [Fact]
        public async Task TakeComission_ShouldCalculateCorrectComission()
        {
            // arrange
            int transactionId = 1;
            decimal totalTransaction = 1000;
            decimal expectedComission = totalTransaction * (decimal)PERCENT_COMISSION;

            // act
            await _comissionService.TakeComission(transactionId, totalTransaction);

            // assert
            _mockComissionsCreateRepository.Verify(repo => repo.CreateAsync(It.Is<Comissions>(
                c => c.IdTransaction == transactionId &&
                     c.Total == expectedComission &&
                     c.Percent == (decimal)PERCENT_COMISSION * 100
            )), Times.Once);
        }

        [Fact]
        public async Task TakeComission_ShouldCallRepositoryOnce()
        {
            // arrange
            int transactionId = 2;
            decimal totalTransaction = 500;

            // act
            await _comissionService.TakeComission(transactionId, totalTransaction);

            // assert
            _mockComissionsCreateRepository.Verify(repo => repo.CreateAsync(It.IsAny<Comissions>()), Times.Once);
        }

        [Fact]
        public async Task TakeComission_ShouldThrowException_WhenRepositoryFails()
        {
            // arrange
            int transactionId = 3;
            decimal totalTransaction = 1500;

            _mockComissionsCreateRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Comissions>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _comissionService.TakeComission(transactionId, totalTransaction));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
