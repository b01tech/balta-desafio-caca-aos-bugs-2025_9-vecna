using BugStore.Application.Order.Handlers;
using BugStore.Application.Order.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using BugStore.TestUtilities.Builders;
using Moq;

namespace BugStore.Application.Tests.Order.Handlers;

public class GetOrderByCustomerIdHandlerTests
{
    private readonly Mock<IOrderReadOnlyRepository> _readRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetOrderByCustomerIdHandler _handler;

    public GetOrderByCustomerIdHandlerTests()
    {
        _readRepositoryMock = new Mock<IOrderReadOnlyRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _cacheServiceMock
            .Setup(x =>
                x.GetAsync<BugStore.Application.Order.DTOs.ResponseListOrderDTO>(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((BugStore.Application.Order.DTOs.ResponseListOrderDTO?)null);
        _handler = new GetOrderByCustomerIdHandler(
            _readRepositoryMock.Object,
            _cacheServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCustomerId_ShouldReturnCustomerOrders()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var page = 1;
        var pageSize = 10;
        var totalItems = 15L;
        var orders = new List<Domain.Entities.Order>
        {
            OrderBuilder.Build(customerId: customerId),
            OrderBuilder.Build(customerId: customerId),
            OrderBuilder.Build(customerId: customerId)
        };
        var query = new GetOrderByCustomerIdQuery(customerId, page, pageSize);

        _readRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(customerId, page, pageSize))
            .ReturnsAsync(orders);

        _readRepositoryMock.Setup(x => x.GetTotalItemAsync()).ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalItems, result.TotalItems);
        Assert.Equal(page, result.Page);
        Assert.Equal(2, result.TotalPages); // 15 items / 10 per page = 2 pages (rounded up)
        Assert.Equal(orders.Count, result.Orders.Count);

        for (int i = 0; i < orders.Count; i++)
        {
            Assert.Equal(orders[i].Id, result.Orders[i].Id);
            Assert.Equal(orders[i].CustomerId, result.Orders[i].CustomerId);
            Assert.Equal(orders[i].CreatedAt, result.Orders[i].CreatedAt);
            Assert.Equal(orders[i].Total, result.Orders[i].Total);
        }

        _readRepositoryMock.Verify(
            x => x.GetByCustomerIdAsync(customerId, page, pageSize),
            Times.Once
        );
        _readRepositoryMock.Verify(x => x.GetTotalItemAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomerWithoutOrders_ShouldReturnEmptyList()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var page = 1;
        var pageSize = 10;
        var totalItems = 0L;
        var orders = new List<Domain.Entities.Order>();
        var query = new GetOrderByCustomerIdQuery(customerId, page, pageSize);

        _readRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(customerId, page, pageSize))
            .ReturnsAsync(orders);

        _readRepositoryMock.Setup(x => x.GetTotalItemAsync()).ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(page, result.Page);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Orders);

        _readRepositoryMock.Verify(
            x => x.GetByCustomerIdAsync(customerId, page, pageSize),
            Times.Once
        );
        _readRepositoryMock.Verify(x => x.GetTotalItemAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentPageSize_ShouldCalculateCorrectTotalPages()
    {
        // Arrange
        var customerId = Guid.CreateVersion7();
        var page = 3;
        var pageSize = 3;
        var totalItems = 8L;
        var orders = new List<Domain.Entities.Order>
        {
            OrderBuilder.Build(customerId: customerId),
            OrderBuilder.Build(customerId: customerId)
        };
        var query = new GetOrderByCustomerIdQuery(customerId, page, pageSize);

        _readRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(customerId, page, pageSize))
            .ReturnsAsync(orders);

        _readRepositoryMock.Setup(x => x.GetTotalItemAsync()).ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalItems, result.TotalItems);
        Assert.Equal(page, result.Page);
        Assert.Equal(3, result.TotalPages); // 8 items / 3 per page = 3 pages (rounded up)
        Assert.Equal(orders.Count, result.Orders.Count);

        _readRepositoryMock.Verify(
            x => x.GetByCustomerIdAsync(customerId, page, pageSize),
            Times.Once
        );
        _readRepositoryMock.Verify(x => x.GetTotalItemAsync(), Times.Once);
    }
}
