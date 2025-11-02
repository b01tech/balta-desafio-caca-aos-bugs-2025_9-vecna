using BugStore.Application.Reports.DTOs;
using BugStore.Application.Reports.Queries;
using BugStore.Application.Services.Cache;
using BugStore.Domain.Interfaces;
using BugStore.Exception.ExceptionMessages;
using BugStore.Exception.ProjectException;
using Mediator;

namespace BugStore.Application.Reports.Handlers
{
    public class GetRevenueByCustomerHandler
        : IRequestHandler<GetRevenueByCustomerQuery, ResponseRevenueByCustomerDTO>
    {
        private readonly IOrderReadOnlyRepository _orderRepository;
        private readonly ICustomerReadOnlyRepository _customerRepository;
        private readonly ICacheService _cacheService;
        private readonly string _cacheKeyPrefix = "revenue_customer_";

        public GetRevenueByCustomerHandler(
            IOrderReadOnlyRepository orderRepository,
            ICustomerReadOnlyRepository customerRepository,
            ICacheService cacheService
        )
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _cacheService = cacheService;
        }

        public async ValueTask<ResponseRevenueByCustomerDTO> Handle(
            GetRevenueByCustomerQuery request,
            CancellationToken cancellationToken
        )
        {
            var cacheKey = $"{_cacheKeyPrefix}{request.Request.CustomerId}";

            var cachedResult = await _cacheService.GetAsync<ResponseRevenueByCustomerDTO>(
                cacheKey,
                cancellationToken
            );
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var customer = await _customerRepository.GetByIdAsync(request.Request.CustomerId);
            if (customer == null)
                throw new NotFoundException(ResourceExceptionMessage.CUSTOMER_NOT_FOUND);

            var (totalOrders, totalSpent) = await _orderRepository.GetTotalByCustomerIdAsync(
                request.Request.CustomerId
            );

            var result = new ResponseRevenueByCustomerDTO(
                customer.Id,
                customer.Name,
                totalOrders,
                totalSpent
            );

            await _cacheService.SetAsync(cacheKey, result, cancellationToken);

            return result;
        }
    }
}
