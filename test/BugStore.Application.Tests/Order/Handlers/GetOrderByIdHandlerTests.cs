using BugStore.Application.Order.Handlers;
using BugStore.Application.Order.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using BugStore.Exception.ProjectException;
using BugStore.TestUtilities.Builders;
using Moq;

namespace BugStore.Application.Tests.Order.Handlers;

public class GetOrderByIdHandlerTests
{
    private readonly Mock<IOrderReadOnlyRepository> _readRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _readRepositoryMock = new Mock<IOrderReadOnlyRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _cacheServiceMock
            .Setup(x =>
                x.GetAsync<BugStore.Application.Order.DTOs.ResponseOrderDetailedDTO>(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((BugStore.Application.Order.DTOs.ResponseOrderDetailedDTO?)null);
        _handler = new GetOrderByIdHandler(_readRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldThrowNotFoundException()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var query = new GetOrderByIdQuery(orderId);

        _readRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Domain.Entities.Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None).AsTask()
        );

        _readRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
    }
}
