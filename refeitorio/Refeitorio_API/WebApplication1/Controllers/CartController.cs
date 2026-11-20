using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult> GetCart(int userId)
        {
            var (cartItems, hasSufficientBalance) = await _cartService.GetCartItemsAsync(userId);
            return Ok(new { 
                Items = cartItems,
                HasSufficientBalance = hasSufficientBalance
            });
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart(AddCartItemDTO cartItemDto)
        {
            var (item, hasSufficientBalance) = await _cartService.AddToCartAsync(cartItemDto);
            if (item == null)
                return BadRequest("Não foi possível adicionar o produto ao carrinho.");

            return Ok(new {
                Item = item,
                HasSufficientBalance = hasSufficientBalance
            });
        }

        [HttpPut("{id}/quantity/{quantity}/user/{userId}")]
        public async Task<ActionResult> UpdateQuantity(int id, int quantity, int userId)
        {
            var (success, hasSufficientBalance) = await _cartService.UpdateQuantityAsync(id, quantity, userId);
            if (!success)
                return NotFound();

            return Ok(new {
                HasSufficientBalance = hasSufficientBalance
            });
        }

        [HttpDelete("{id}/user/{userId}")]
        public async Task<ActionResult> RemoveFromCart(int id, int userId)
        {
            var (success, hasSufficientBalance) = await _cartService.RemoveFromCartAsync(id, userId);
            if (!success)
                return NotFound();

            return Ok(new {
                HasSufficientBalance = hasSufficientBalance
            });
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}
