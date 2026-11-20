using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            var categoryDtos = categories.Select(c => new CategoryDTO { Id = c.Id, Name = c.Name });
            return Ok(categoryDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            var categoryDto = new CategoryDTO { Id = category.Id, Name = category.Name };
            return Ok(categoryDto);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CreateCategoryDTO createDto)
        {
            var category = await _categoryService.CreateAsync(createDto.Name);
            var categoryDto = new CategoryDTO { Id = category.Id, Name = category.Name };
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDTO updateDto)
        {
            var result = await _categoryService.UpdateAsync(id, updateDto.Name);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                Console.WriteLine($"Controller: Received delete request for category ID {id}");
                
                // First check if category exists and get details
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    Console.WriteLine($"Controller: Category {id} not found");
                    return NotFound(new { Message = $"Category with ID {id} not found" });
                }

                Console.WriteLine($"Controller: Found category '{category.Name}', proceeding with deletion");

                // Delete the category (service will handle associated products)
                var result = await _categoryService.DeleteAsync(id);
                
                if (!result.Success)
                {
                    Console.WriteLine($"Controller: Service reported deletion failed for category {id}");
                    return BadRequest(new { Message = "Failed to delete category" });
                }
                
                var response = new 
                { 
                    Message = "Category deleted successfully",
                    CategoryId = id,
                    CategoryName = category.Name,
                    DeletedProductsCount = result.DeletedProductsCount,
                    Warning = result.HasAssociatedProducts ? $"WARNING: {result.DeletedProductsCount} associated products were also deleted" : null
                };

                Console.WriteLine($"Controller: Successfully deleted category '{category.Name}' with {result.DeletedProductsCount} products");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Controller: Exception during category deletion: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Message = "Internal server error during category deletion", Error = ex.Message });
            }
        }
    }
}
