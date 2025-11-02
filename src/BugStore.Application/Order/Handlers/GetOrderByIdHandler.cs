using BugStore.Application.Order.DTOs;
using BugStore.Application.Order.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using BugStore.Exception.ExceptionMessages;
using BugStore.Exception.ProjectException;
using Mediator;

namespace BugStore.Application.Order.Handlers
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, ResponseOrderDetailedDTO>
    {
        private readonly IOrderReadOnlyRepository _orderReadOnlyRepository;
        private readonly ICacheService _cacheService;
        private readonly string _cacheKeyPrefix = "order_detail_";

        public GetOrderByIdHandler(IOrderReadOnlyRepository orderReadOnlyRepository, ICacheService cacheService)
        {
            _orderReadOnlyRepository = orderReadOnlyRepository;
            _cacheService = cacheService;
        }

        public async ValueTask<ResponseOrderDetailedDTO> Handle(
            GetOrderByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            var cacheKey = $"{_cacheKeyPrefix}{request.Id}";

            var cachedResult = await _cacheService.GetAsync<ResponseOrderDetailedDTO>(
                cacheKey,
                cancellationToken
            );
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var order = await _orderReadOnlyRepository.GetByIdAsync(request.Id);

            if (order is null)
                throw new NotFoundException(ResourceExceptionMessage.ORDER_NOT_FOUND);

            var result = new ResponseOrderDetailedDTO(
                order!.Id,
                order.CustomerId,
                order.CreatedAt,
                order.UpdatedAt,
                order
                    .Lines.Select(l => new OrderLineDTO(
                        l.ProductId,
                        l.Quantity,
                        l.Product.Price,
                        l.Total
                    ))
                    .ToList(),
                order.Total
            );
            await _cacheService.SetAsync(cacheKey, result, cancellationToken);
            return result;
        }
    }
}
