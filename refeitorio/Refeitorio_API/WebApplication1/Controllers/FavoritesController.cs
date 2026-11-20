using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly FavoriteService _favoriteService;

        public FavoritesController(FavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<FavoriteDTO>>> GetUserFavorites(int userId)
        {
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }

        [HttpPost]
        public async Task<ActionResult<FavoriteDTO>> AddFavorite(AddFavoriteDTO favoriteDto)
        {
            var favorite = await _favoriteService.AddFavoriteAsync(favoriteDto);
            if (favorite == null)
                return BadRequest("Produto já está nos favoritos");

            return Ok(favorite);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var result = await _favoriteService.RemoveFavoriteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("user/{userId}/product/{productId}")]
        public async Task<IActionResult> RemoveFavoriteByUserAndProduct(int userId, int productId)
        {
            var result = await _favoriteService.RemoveFavoriteByUserAndProductAsync(userId, productId);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
