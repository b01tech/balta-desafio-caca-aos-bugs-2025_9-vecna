using BugStore.Application.Reports.DTOs;
using BugStore.Application.Reports.Handlers;
using BugStore.Application.Reports.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using Moq;

namespace BugStore.Application.Tests.Reports.Handlers;

public class GetBestCustomersHandlerTests
{
    private readonly Mock<IOrderReadOnlyRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetBestCustomersHandler _handler;

    public GetBestCustomersHandlerTests()
    {
        _repositoryMock = new Mock<IOrderReadOnlyRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetBestCustomersHandler(_repositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnBestCustomers()
    {
        // Arrange
        var topCustomers = 5;
        var request = new RequestBestCustomerDTO(topCustomers);
        var query = new GetBestCustomersQuery(request);

        var expectedCustomers = new List<(
            Guid CustomerId,
            string CustomerName,
            long TotalOrders,
            decimal TotalSpent
        )>
        {
            (Guid.CreateVersion7(), "João Silva", 15L, 2500.00m),
            (Guid.CreateVersion7(), "Maria Santos", 12L, 2200.50m),
            (Guid.CreateVersion7(), "Pedro Costa", 10L, 1800.75m)
        };

        _repositoryMock
            .Setup(x => x.GetBestCustomersAsync(topCustomers))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Customers);
        Assert.Equal(3, result.Customers.Count);

        var firstCustomer = result.Customers.First();
        Assert.Equal(expectedCustomers[0].CustomerId, firstCustomer.CustomerId);
        Assert.Equal(expectedCustomers[0].CustomerName, firstCustomer.CustomerName);
        Assert.Equal(expectedCustomers[0].TotalOrders, firstCustomer.TotalOrders);
        Assert.Equal(expectedCustomers[0].TotalSpent, firstCustomer.TotalSpent);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var topCustomers = 10;
        var request = new RequestBestCustomerDTO(topCustomers);
        var query = new GetBestCustomersQuery(request);

        var expectedCustomers =
            new List<(
                Guid CustomerId,
                string CustomerName,
                long TotalOrders,
                decimal TotalSpent
            )>();

        _repositoryMock
            .Setup(x => x.GetBestCustomersAsync(topCustomers))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Customers);
        Assert.Empty(result.Customers);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var topCustomers = 3;
        var request = new RequestBestCustomerDTO(topCustomers);
        var query = new GetBestCustomersQuery(request);

        _repositoryMock
            .Setup(x => x.GetBestCustomersAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<(Guid, string, long, decimal)>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetBestCustomersAsync(topCustomers), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDefaultTopCustomers_ShouldUseCorrectValue()
    {
        // Arrange
        var request = new RequestBestCustomerDTO(); // Usa valor padrão de 5
        var query = new GetBestCustomersQuery(request);

        _repositoryMock
            .Setup(x => x.GetBestCustomersAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<(Guid, string, long, decimal)>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetBestCustomersAsync(5), Times.Once);
    }
}
