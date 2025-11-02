using BugStore.Application.Reports.DTOs;
using BugStore.Application.Reports.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using Mediator;

namespace BugStore.Application.Reports.Handlers
{
    public class GetRevenueByPeriodHandler
        : IRequestHandler<GetRevenueByPediodQuery, ResponseRevenueByPeriodDTO>
    {
        private readonly IOrderReadOnlyRepository _orderRepository;
        private readonly ICacheService _cacheService;
        private readonly string _cacheKeyPrefix = "revenue_period_";

        public GetRevenueByPeriodHandler(
            IOrderReadOnlyRepository orderRepository,
            ICacheService cacheService
        )
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
        }

        public async ValueTask<ResponseRevenueByPeriodDTO> Handle(
            GetRevenueByPediodQuery request,
            CancellationToken cancellationToken
        )
        {
            var cacheKey =
                $"{_cacheKeyPrefix}{request.Request.StartDate:ddMMyyyy}_{request.Request.EndDate:ddMMyyyy}";

            var cachedResult = await _cacheService.GetAsync<ResponseRevenueByPeriodDTO>(
                cacheKey,
                cancellationToken
            );
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var (totalOrders, totalRevenue) = await _orderRepository.GetTotalByPeriod(
                request.Request.StartDate,
                request.Request.EndDate
            );

            var result = new ResponseRevenueByPeriodDTO(
                request.Request.StartDate,
                request.Request.EndDate,
                totalOrders,
                totalRevenue
            );

            await _cacheService.SetAsync(cacheKey, result, cancellationToken);

            return result;
        }
    }
}
