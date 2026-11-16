using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Baskets;
using E_commerce.Application.IServices;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Baskets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketService(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        public async Task<Result<BasketDto>> CreateOrUpdateBasketAsync(BasketDto basket, CancellationToken cancellationToken = default)
        {
            var customerBasket=_mapper.Map<CustomerBasket>(basket);
            //ct from basketRepository becuse i not send pervious paramter (TTL)
            var basketResult = await _basketRepository.CreateOrUpdateAsync(customerBasket, ct:cancellationToken);
            return basketResult !=null ?
                Result<BasketDto>.Ok(_mapper.Map<BasketDto>(basketResult))
                : Result<BasketDto>.Fail(Error.Failure("BasketCreateOrUpdate.Failure","Failed to create or update basket"));
        }

        public async Task<Result<bool>> DeleteBasketAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await _basketRepository.DeleteBasketAsync(id, ct:cancellationToken);
            return result ?
                Result<bool>.Ok(true)
                : Result<bool>.Fail(Error.Failure("BasketDelete.Failure","Failed to delete basket"));
        }

        public async Task<Result<BasketDto>> GetBasketAsync(string id, CancellationToken ct = default)
        {
            var basket = await _basketRepository.GetBasketAsync(id, ct:ct);
            return basket != null ?
                Result<BasketDto>.Ok(_mapper.Map<BasketDto>(basket))
                : Result<BasketDto>.Fail(Error.Failure("BasketGet.Failure","Basket not found"));
        }
    }
}
