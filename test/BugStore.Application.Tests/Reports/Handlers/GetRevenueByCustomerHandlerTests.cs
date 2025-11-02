using BugStore.Application.Reports.DTOs;
using BugStore.Application.Reports.Handlers;
using BugStore.Application.Reports.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using BugStore.Exception.ProjectException;
using BugStore.TestUtilities.Builders;
using Moq;

namespace BugStore.Application.Tests.Reports.Handlers;

public class GetRevenueByCustomerHandlerTests
{
    private readonly Mock<IOrderReadOnlyRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerReadOnlyRepository> _customerRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetRevenueByCustomerHandler _handler;

    public GetRevenueByCustomerHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderReadOnlyRepository>();
        _customerRepositoryMock = new Mock<ICustomerReadOnlyRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetRevenueByCustomerHandler(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _cacheServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ShouldReturnRevenueData()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var customer = CustomerBuilder.Build();
        var expectedTotalOrders = 8L;
        var expectedTotalSpent = 1200.75m;

        var request = new RequestRevenueByCustomerDTO(customerId);
        var query = new GetRevenueByCustomerQuery(request);

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId)).ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.GetTotalByCustomerIdAsync(customerId))
            .ReturnsAsync((expectedTotalOrders, expectedTotalSpent));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(customer.Name, result.CustomerName);
        Assert.Equal(expectedTotalOrders, result.TotalOrders);
        Assert.Equal(expectedTotalSpent, result.TotalSpent);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldThrowArgumentException()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var request = new RequestRevenueByCustomerDTO(customerId);
        var query = new GetRevenueByCustomerQuery(request);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Domain.Entities.Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None).AsTask()
        );
    }

    [Fact]
    public async Task Handle_WithCustomerWithoutOrders_ShouldReturnZeroValues()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var customer = CustomerBuilder.Build();
        var expectedTotalOrders = 0L;
        var expectedTotalSpent = 0m;

        var request = new RequestRevenueByCustomerDTO(customerId);
        var query = new GetRevenueByCustomerQuery(request);

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId)).ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.GetTotalByCustomerIdAsync(customerId))
            .ReturnsAsync((expectedTotalOrders, expectedTotalSpent));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(customer.Name, result.CustomerName);
        Assert.Equal(0L, result.TotalOrders);
        Assert.Equal(0m, result.TotalSpent);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoriesWithCorrectParameters()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var customer = CustomerBuilder.Build();
        var request = new RequestRevenueByCustomerDTO(customerId);
        var query = new GetRevenueByCustomerQuery(request);

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId)).ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(x => x.GetTotalByCustomerIdAsync(customerId))
            .ReturnsAsync((5L, 500m));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _customerRepositoryMock.Verify(x => x.GetByIdAsync(customerId), Times.Once);
        _orderRepositoryMock.Verify(x => x.GetTotalByCustomerIdAsync(customerId), Times.Once);
    }
}
