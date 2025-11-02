using BugStore.Application.Reports.DTOs;
using BugStore.Application.Reports.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using Mediator;

namespace BugStore.Application.Reports.Handlers
{
    public class GetBestCustomersHandler
        : IRequestHandler<GetBestCustomersQuery, ResponseBestCustomersListDTO>
    {
        private readonly IOrderReadOnlyRepository _orderRepository;
        private readonly ICacheService _cacheService;
        private readonly string _cacheKeyPrefix = "best_customers_";

        public GetBestCustomersHandler(
            IOrderReadOnlyRepository orderRepository,
            ICacheService cacheService
        )
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
        }

        public async ValueTask<ResponseBestCustomersListDTO> Handle(
            GetBestCustomersQuery request,
            CancellationToken cancellationToken
        )
        {
            var cacheKey = $"{_cacheKeyPrefix}{request.Request.TopCustomers}";

            var cachedResult = await _cacheService.GetAsync<ResponseBestCustomersListDTO>(
                cacheKey,
                cancellationToken
            );
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var bestCustomers = await _orderRepository.GetBestCustomersAsync(
                request.Request.TopCustomers
            );

            var customerDTOs = bestCustomers
                .Select(bc => new ResponseBestCustomerDTO(
                    bc.CustomerId,
                    bc.CustomerName,
                    bc.TotalOrders,
                    bc.TotalSpent
                ))
                .ToList();

            var result = new ResponseBestCustomersListDTO(customerDTOs);

            await _cacheService.SetAsync(cacheKey, result, cancellationToken);

            return result;
        }
    }
}
