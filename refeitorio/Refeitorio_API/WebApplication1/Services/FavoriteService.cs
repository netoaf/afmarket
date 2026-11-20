using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class FavoriteService
    {
        private readonly FavoriteRepository _favoriteRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserRepository _userRepository;

        public FavoriteService(
            FavoriteRepository favoriteRepository,
            ProductRepository productRepository,
            UserRepository userRepository)
        {
            _favoriteRepository = favoriteRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<FavoriteDTO>> GetUserFavoritesAsync(int userId)
        {
            var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
            return favorites.Select(MapToDTO);
        }

        public async Task<FavoriteDTO> AddFavoriteAsync(AddFavoriteDTO favoriteDto)
        {
            // Verificar se o usuário existe
            var user = await _userRepository.GetByIdAsync(favoriteDto.UserId);
            if (user == null)
                return null;

            // Verificar se o produto existe
            var product = await _productRepository.GetByIdAsync(favoriteDto.ProductId);
            if (product == null)
                return null;

            // Verificar se o produto já está nos favoritos
            var existingFavorite = await _favoriteRepository.GetByUserAndProductAsync(favoriteDto.UserId, favoriteDto.ProductId);
            if (existingFavorite != null)
                return null;

            // Adicionar aos favoritos
            var favorite = new Favorite
            {
                UserId = favoriteDto.UserId,
                ProductId = favoriteDto.ProductId,
                AddedAt = DateTime.Now
            };

            await _favoriteRepository.AddAsync(favorite);
            
            // Buscar o favorito com o produto
            favorite = await _favoriteRepository.GetByIdAsync(favorite.Id);
            return MapToDTO(favorite);
        }

        public async Task<bool> RemoveFavoriteAsync(int id)
        {
            var favorite = await _favoriteRepository.GetByIdAsync(id);
            if (favorite == null)
                return false;

            await _favoriteRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> RemoveFavoriteByUserAndProductAsync(int userId, int productId)
        {
            var favorite = await _favoriteRepository.GetByUserAndProductAsync(userId, productId);
            if (favorite == null)
                return false;

            await _favoriteRepository.DeleteAsync(favorite.Id);
            return true;
        }

        private FavoriteDTO MapToDTO(Favorite favorite)
        {
            return new FavoriteDTO
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                ProductId = favorite.ProductId,
                ProductName = favorite.Product?.Name,
                ProductImage = favorite.Product?.ImageUrl,
                Price = favorite.Product?.Price ?? 0,
                AddedAt = favorite.AddedAt
            };
        }
    }
}
