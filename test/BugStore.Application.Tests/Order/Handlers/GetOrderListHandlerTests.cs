using BugStore.Application.Order.Handlers;
using BugStore.Application.Order.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using BugStore.TestUtilities.Builders;
using Moq;

namespace BugStore.Application.Tests.Order.Handlers;

public class GetOrderListHandlerTests
{
    private readonly Mock<IOrderReadOnlyRepository> _readRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetOrderListHandler _handler;

    public GetOrderListHandlerTests()
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
        _handler = new GetOrderListHandler(_readRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnPaginatedOrders()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var totalItems = 25L;
        var orders = new List<Domain.Entities.Order>
        {
            OrderBuilder.Build(),
            OrderBuilder.Build(),
            OrderBuilder.Build()
        };
        var query = new GetOrderListQuery(page, pageSize);

        _readRepositoryMock.Setup(x => x.GetAllAsync(page, pageSize)).ReturnsAsync(orders);

        _readRepositoryMock.Setup(x => x.GetTotalItemAsync()).ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalItems, result.TotalItems);
        Assert.Equal(page, result.Page);
        Assert.Equal(3, result.TotalPages); // 25 items / 10 per page = 3 pages
        Assert.Equal(orders.Count, result.Orders.Count);

        for (int i = 0; i < orders.Count; i++)
        {
            Assert.Equal(orders[i].Id, result.Orders[i].Id);
            Assert.Equal(orders[i].CustomerId, result.Orders[i].CustomerId);
            Assert.Equal(orders[i].CreatedAt, result.Orders[i].CreatedAt);
            Assert.Equal(orders[i].Total, result.Orders[i].Total);
        }

        _readRepositoryMock.Verify(x => x.GetAllAsync(page, pageSize), Times.Once);
        _readRepositoryMock.Verify(x => x.GetTotalItemAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var totalItems = 0L;
        var orders = new List<Domain.Entities.Order>();
        var query = new GetOrderListQuery(page, pageSize);

        _readRepositoryMock.Setup(x => x.GetAllAsync(page, pageSize)).ReturnsAsync(orders);

        _readRepositoryMock.Setup(x => x.GetTotalItemAsync()).ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(page, result.Page);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Orders);

        _readRepositoryMock.Verify(x => x.GetAllAsync(page, pageSize), Times.Once);
        _readRepositoryMock.Verify(x => x.GetTotalItemAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentPageSize_ShouldCalculateCorrectTotalPages()
    {
        // Arrange
        var page = 2;
        var pageSize = 5;
        var totalItems = 12L;
        var orders = new List<Domain.Entities.Order> { OrderBuilder.Build(), OrderBuilder.Build() };
        var query = new GetOrderListQuery(page, pageSize);

        _readRepositoryMock.Setup(x => x.GetAllAsync(page, pageSize)).ReturnsAsync(orders);

        _readRepositoryMock.Setup(x => x.GetTotalItemAsync()).ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(totalItems, result.TotalItems);
        Assert.Equal(page, result.Page);
        Assert.Equal(3, result.TotalPages); // 12 items / 5 per page = 3 pages (rounded up)
        Assert.Equal(orders.Count, result.Orders.Count);

        _readRepositoryMock.Verify(x => x.GetAllAsync(page, pageSize), Times.Once);
        _readRepositoryMock.Verify(x => x.GetTotalItemAsync(), Times.Once);
    }
}
