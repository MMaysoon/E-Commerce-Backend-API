
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Baskets;

namespace E_commerce.Application.IServices
{
    public interface IBasketService
    {
        Task<Result<BasketCreationDto>> CreateBasketAsync(CancellationToken ct = default);

        Task<Result<BasketDto>> GetBasketAsync(string basketId,string basketToken,CancellationToken ct = default);

        Task<Result<BasketDto>> AddItemAsync(string basketId,string basketToken,AddBasketItemDto itemDto, CancellationToken ct = default);

        Task<Result<BasketDto>> UpdateItemAsync(string basketId,string basketToken,int productId, UpdateBasketItemDto itemDto,CancellationToken ct = default);

        Task<Result<BasketDto>> RemoveItemAsync(string basketId,string basketToken, int productId,CancellationToken ct = default);

        Task<Result<bool>> DeleteBasketAsync( string basketId,string basketToken,CancellationToken ct = default);
    }
}
