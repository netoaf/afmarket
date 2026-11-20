using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class CartService
    {
        private readonly CartRepository _cartRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;

        public CartService(
            CartRepository cartRepository,
            ProductRepository productRepository,
            UserRepository userRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<(IEnumerable<CartItemDTO> Items, bool HasSufficientBalance)> GetCartItemsAsync(int userId)
        {
            var cartItems = await _cartRepository.GetUserCartAsync(userId);
            var itemDtos = cartItems.Select(MapToDTO).ToList();
            
            // Verificar se o usuário tem saldo suficiente
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (itemDtos, false);
            
            decimal totalCartValue = itemDtos.Sum(item => item.TotalPrice);
            bool hasSufficientBalance = user.Balance >= totalCartValue;
            
            return (itemDtos, hasSufficientBalance);
        }

        public async Task<(CartItemDTO Item, bool HasSufficientBalance)> AddToCartAsync(AddCartItemDTO cartItemDto)
        {
            // Verificar se o usuário existe
            var user = await _userRepository.GetByIdAsync(cartItemDto.UserId);
            if (user == null)
                return (null, false);

            // Verificar se o produto existe
            var product = await _productRepository.GetByIdAsync(cartItemDto.ProductId);
            if (product == null || !product.IsAvailable || product.Stock <= 0)
                return (null, false);

            // Verificar se o produto já está no carrinho
            var existingCartItem = await _cartRepository.GetByUserAndProductAsync(cartItemDto.UserId, cartItemDto.ProductId);
            
            CartItemDTO resultItemDto;
            if (existingCartItem != null)
            {
                // Atualizar a quantidade
                existingCartItem.Quantity += cartItemDto.Quantity;
                await _cartRepository.UpdateAsync(existingCartItem);
                resultItemDto = MapToDTO(existingCartItem);
            }
            else
            {
                // Adicionar novo item ao carrinho
                var cartItem = new Cart
                {
                    UserId = cartItemDto.UserId,
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity,
                    CreatedAt = DateTime.Now
                };

                await _cartRepository.AddAsync(cartItem);
                
                // Buscar o item do carrinho com o produto
                cartItem = await _cartRepository.GetByIdAsync(cartItem.Id);
                resultItemDto = MapToDTO(cartItem);
            }
            
            // Verificar se o usuário tem saldo suficiente para todos os itens do carrinho
            var (_, hasSufficientBalance) = await GetCartItemsAsync(cartItemDto.UserId);
            
            return (resultItemDto, hasSufficientBalance);
        }

        public async Task<(bool Success, bool HasSufficientBalance)> UpdateQuantityAsync(int id, int quantity, int userId)
        {
            if (quantity <= 0)
            {
                var removeResult = await RemoveFromCartAsync(id, userId);
                return (removeResult.Success, removeResult.HasSufficientBalance);
            }

            var cartItem = await _cartRepository.GetByIdAsync(id);
            if (cartItem == null)
                return (false, false);

            cartItem.Quantity = quantity;
            await _cartRepository.UpdateAsync(cartItem);
            
            // Verificar o saldo após atualização
            var (_, hasSufficientBalance) = await GetCartItemsAsync(userId);
            return (true, hasSufficientBalance);
        }

        public async Task<(bool Success, bool HasSufficientBalance)> RemoveFromCartAsync(int id, int userId)
        {
            var cartItem = await _cartRepository.GetByIdAsync(id);
            if (cartItem == null)
                return (false, false);

            await _cartRepository.DeleteAsync(id);
            
            // Verificar o saldo após remoção
            var (_, hasSufficientBalance) = await GetCartItemsAsync(userId);
            return (true, hasSufficientBalance);
        }

        public async Task ClearCartAsync(int userId)
        {
            await _cartRepository.ClearCartAsync(userId);
        }

        private CartItemDTO MapToDTO(Cart cartItem)
        {
            return new CartItemDTO
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product?.Name,
                ImageUrl = cartItem.Product?.ImageUrl,
                UnitPrice = cartItem.Product?.Price ?? 0,
                Quantity = cartItem.Quantity,
                TotalPrice = (cartItem.Product?.Price ?? 0) * cartItem.Quantity
            };
        }
    }
}
