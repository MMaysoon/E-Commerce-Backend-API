
using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Baskets;
using E_commerce.Application.IServices;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Baskets;
using E_commerce.Domain.Entities.Products;
using System.Security.Cryptography;
using System.Text;

namespace E_commerce.Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BasketService(IBasketRepository basketRepository,IUnitOfWork unitOfWork,IMapper mapper)
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // 1. Create new basket
        public async Task<Result<BasketCreationDto>> CreateBasketAsync(CancellationToken ct = default)
        {
            var basketToken = GenerateBasketToken();

            var basket = new CustomerBasket
            {
                Id = Guid.NewGuid().ToString(),
                BasketTokenHash = HashBasketToken(basketToken),
                Items = []
            };

            var result = await _basketRepository.CreateOrUpdateAsync(basket, ct: ct);

            if (result == null)
            {
                return Result<BasketCreationDto>.Fail(
                    Error.Failure(
                        "Basket.Create.Failed",
                        "Failed to create basket."));
            }

            return Result<BasketCreationDto>.Ok(
                new BasketCreationDto
                {
                    Basket = _mapper.Map<BasketDto>(result),
                    BasketToken = basketToken
                });
        }

        // 2. Get basket
        public async Task<Result<BasketDto>> GetBasketAsync(string basketId,string basketToken, CancellationToken ct = default)
        {
            var basket = await GetOwnedBasketAsync(basketId, basketToken, ct);

            if (basket == null)
                return BasketAccessFailure();

            return Result<BasketDto>.Ok(
                _mapper.Map<BasketDto>(basket));
        }

        // 3. Add item
        public async Task<Result<BasketDto>> AddItemAsync(string basketId,string basketToken,AddBasketItemDto itemDto,CancellationToken ct = default)
        {
            var basket = await GetOwnedBasketAsync(basketId, basketToken, ct);

            if (basket == null)
                return BasketAccessFailure();

            if (itemDto.Quantity < 1 || itemDto.Quantity > 99)
            {
                return Result<BasketDto>.Fail(
                    Error.Failure(
                        "Basket.InvalidQuantity",
                        "Quantity must be between 1 and 99."));
            }

            // Get product from SQL Server
            var productRepository =_unitOfWork.GetRepository<Product, int>();

            var product = await productRepository.GetByIdAsync(itemDto.ProductId, ct);

            if (product == null)
            {
                return Result<BasketDto>.Fail(
                    Error.Failure(
                        "Basket.ProductNotFound",
                        "Product not found."));
            }

            var existingItem = basket.Items.FirstOrDefault( x => x.Id == itemDto.ProductId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + itemDto.Quantity > 99)
                {
                    return Result<BasketDto>.Fail(
                        Error.Failure(
                            "Basket.QuantityLimit",
                            "Maximum quantity per product is 99."));
                }

                existingItem.Quantity += itemDto.Quantity;

                // Refresh product data from database
                existingItem.ProductName = product.Name;
                existingItem.PictureUrl = product.PictureUrl;
                existingItem.Price = product.Price;
            }
            else
            {
                basket.Items.Add(new BasketItem
                {
                    Id = product.Id,
                    ProductName = product.Name,
                    PictureUrl = product.PictureUrl,
                    Price = product.Price,
                    Quantity = itemDto.Quantity
                });
            }

            return await SaveBasketAsync(basket, ct);
        }

        // 4. Update item quantity
        public async Task<Result<BasketDto>> UpdateItemAsync(string basketId,string basketToken,int productId,UpdateBasketItemDto itemDto,CancellationToken ct = default)
        {
            var basket = await GetOwnedBasketAsync( basketId, basketToken, ct);

            if (basket == null)
                return BasketAccessFailure();

            if (itemDto.Quantity < 1 || itemDto.Quantity > 99)
            {
                return Result<BasketDto>.Fail(
                    Error.Failure(
                        "Basket.InvalidQuantity",
                        "Quantity must be between 1 and 99."));
            }

            var item = basket.Items.FirstOrDefault(x => x.Id == productId);

            if (item == null)
            {
                return Result<BasketDto>.Fail(
                    Error.Failure(
                        "Basket.ItemNotFound",
                        "Item not found in basket."));
            }

            item.Quantity = itemDto.Quantity;

            return await SaveBasketAsync(basket, ct);
        }

        // 5. Remove item
        public async Task<Result<BasketDto>> RemoveItemAsync(string basketId,string basketToken,int productId,CancellationToken ct = default)
        {
            var basket = await GetOwnedBasketAsync( basketId, basketToken, ct);

            if (basket == null)
                return BasketAccessFailure();

            var item = basket.Items.FirstOrDefault(x => x.Id == productId);

            if (item == null)
            {
                return Result<BasketDto>.Fail(
                    Error.Failure(
                        "Basket.ItemNotFound",
                        "Item not found in basket."));
            }

            basket.Items.Remove(item);

            return await SaveBasketAsync(basket, ct);
        }

        // 6. Delete basket
        public async Task<Result<bool>> DeleteBasketAsync(string basketId,string basketToken,CancellationToken ct = default)
        {
            var basket = await GetOwnedBasketAsync(basketId, basketToken, ct);

            if (basket == null)
                return Result<bool>.Fail(
                    Error.Failure(
                        "Basket.AccessDenied",
                        "Basket not found or access denied."));

            var deleted = await _basketRepository.DeleteBasketAsync(basketId, ct);

            return deleted
                ? Result<bool>.Ok(true)
                : Result<bool>.Fail(
                    Error.Failure(
                        "Basket.Delete.Failed",
                        "Failed to delete basket."));
        }

        // 7. Get basket for checkout
        public async Task<Result<CustomerBasket>> GetBasketForCheckoutAsync(string basketId,string basketToken,CancellationToken ct = default)
        {
            var basket = await GetOwnedBasketAsync(basketId,basketToken,ct);

            if (basket == null)
            {
                return Result<CustomerBasket>.Fail(
                    Error.Failure(
                        "Basket.AccessDenied",
                        "Basket not found or access denied."));
            }

            if (basket.Items.Count == 0)
            {
                return Result<CustomerBasket>.Fail(
                    Error.Validation(
                        "Basket.Empty",
                        "Cannot checkout with an empty basket."));
            }

            return Result<CustomerBasket>.Ok(basket);
        }

        // Helper: Get basket only if token is valid
        private async Task<CustomerBasket?> GetOwnedBasketAsync(string basketId, string basketToken,CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(basketToken))
                return null;

            var basket = await _basketRepository.GetBasketAsync(
                basketId, ct);

            if (basket == null ||
                string.IsNullOrWhiteSpace(basket.BasketTokenHash))
                return null;

            var providedHash = HashBasketToken(basketToken);

            return VerifyBasketToken(
                providedHash, basket.BasketTokenHash)
                ? basket
                : null;
        }

        // Helper: Save basket and return DTO
        private async Task<Result<BasketDto>> SaveBasketAsync(CustomerBasket basket,CancellationToken ct)
        {
            var result = await _basketRepository.CreateOrUpdateAsync(
                basket, ct: ct);

            if (result == null)
            {
                return Result<BasketDto>.Fail(
                    Error.Failure(
                        "Basket.Save.Failed",
                        "Failed to save basket."));
            }

            return Result<BasketDto>.Ok( _mapper.Map<BasketDto>(result));
        }

        // Generate secure random token
        private static string GenerateBasketToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        // Hash token before storing it
        private static string HashBasketToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes);
        }

        // Compare hashes safely
        private static bool VerifyBasketToken(string providedHash,string storedHash)
        {
            try
            {
                var providedBytes =
                    Convert.FromHexString(providedHash);

                var storedBytes =
                    Convert.FromHexString(storedHash);

                return CryptographicOperations.FixedTimeEquals(
                    providedBytes, storedBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static Result<BasketDto> BasketAccessFailure()
        {
            return Result<BasketDto>.Fail(
                Error.Failure(
                    "Basket.AccessDenied",
                    "Basket not found or access denied."));
        }
    }
}
