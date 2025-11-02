using BugStore.Application.Extensions;
using BugStore.Application.Order.DTOs;
using BugStore.Application.Order.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using Mediator;

namespace BugStore.Application.Order.Handlers
{
    public class GetOrderListHandler : IRequestHandler<GetOrderListQuery, ResponseListOrderDTO>
    {
        private readonly IOrderReadOnlyRepository _orderReadOnlyRepository;
        private readonly ICacheService _cacheService;
        private readonly string _cacheKeyPrefix = "orders_list_";

        public GetOrderListHandler(
            IOrderReadOnlyRepository orderReadOnlyRepository,
            ICacheService cacheService
        )
        {
            _orderReadOnlyRepository = orderReadOnlyRepository;
            _cacheService = cacheService;
        }

        public async ValueTask<ResponseListOrderDTO> Handle(
            GetOrderListQuery request,
            CancellationToken cancellationToken
        )
        {
            var cacheKey = $"{_cacheKeyPrefix}page_{request.Page}_size_{request.PageSize}";

            var cachedResult = await _cacheService.GetAsync<ResponseListOrderDTO>(
                cacheKey,
                cancellationToken
            );
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var orders = await _orderReadOnlyRepository.GetAllAsync(request.Page, request.PageSize);
            var totalItems = await _orderReadOnlyRepository.GetTotalItemAsync();

            var totalPages = totalItems.CalculateTotalPages(request.PageSize);

            var orderSummaries = orders
                .Select(o => new ResponseOrderSummaryDTO(o.Id, o.CustomerId, o.CreatedAt, o.Total))
                .ToList();

            var result = new ResponseListOrderDTO(
                totalItems,
                request.Page,
                totalPages,
                orderSummaries
            );
            await _cacheService.SetAsync(cacheKey, result, cancellationToken);
            return result;
        }
    }
}
