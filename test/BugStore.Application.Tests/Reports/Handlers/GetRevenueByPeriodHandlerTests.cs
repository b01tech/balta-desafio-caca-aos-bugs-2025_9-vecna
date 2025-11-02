using BugStore.Application.Reports.DTOs;
using BugStore.Application.Reports.Handlers;
using BugStore.Application.Reports.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using Moq;

namespace BugStore.Application.Tests.Reports.Handlers;

public class GetRevenueByPeriodHandlerTests
{
    private readonly Mock<IOrderReadOnlyRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetRevenueByPeriodHandler _handler;

    public GetRevenueByPeriodHandlerTests()
    {
        _repositoryMock = new Mock<IOrderReadOnlyRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetRevenueByPeriodHandler(_repositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPeriod_ShouldReturnRevenueData()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var expectedTotalOrders = 10L;
        var expectedTotalRevenue = 1500.50m;

        var request = new RequestRevenueByPeriodDTO(startDate, endDate);
        var query = new GetRevenueByPediodQuery(request);

        _repositoryMock
            .Setup(x => x.GetTotalByPeriod(startDate, endDate))
            .ReturnsAsync((expectedTotalOrders, expectedTotalRevenue));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal(expectedTotalOrders, result.TotalOrders);
        Assert.Equal(expectedTotalRevenue, result.TotalRevenue);
    }

    [Fact]
    public async Task Handle_WithEmptyPeriod_ShouldReturnZeroValues()
    {
        // Arrange
        var startDate = new DateTime(2024, 6, 1);
        var endDate = new DateTime(2024, 6, 30);
        var expectedTotalOrders = 0L;
        var expectedTotalRevenue = 0m;

        var request = new RequestRevenueByPeriodDTO(startDate, endDate);
        var query = new GetRevenueByPediodQuery(request);

        _repositoryMock
            .Setup(x => x.GetTotalByPeriod(startDate, endDate))
            .ReturnsAsync((expectedTotalOrders, expectedTotalRevenue));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal(0L, result.TotalOrders);
        Assert.Equal(0m, result.TotalRevenue);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var startDate = new DateTime(2024, 3, 1);
        var endDate = new DateTime(2024, 3, 31);
        var request = new RequestRevenueByPeriodDTO(startDate, endDate);
        var query = new GetRevenueByPediodQuery(request);

        _repositoryMock
            .Setup(x => x.GetTotalByPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((5L, 750.25m));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetTotalByPeriod(startDate, endDate), Times.Once);
    }
}
