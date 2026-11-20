using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync(int? categoryId = null, bool? isVegetarian = null)
        {
            var products = await _productRepository.GetAllAsync(categoryId, isVegetarian);
            return products.Select(MapToDTO);
        }

        public async Task<ProductDTO> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            return MapToDTO(product);
        }

        public async Task<ProductDTO> CreateAsync(CreateProductDTO productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                CategoryId = productDto.CategoryId,
                IsVegetarian = productDto.IsVegetarian,
                Stock = productDto.Stock,
                Calories = productDto.Calories,
                Protein = productDto.Protein,
                Carbs = productDto.Carbs,
                Fat = productDto.Fat,
                Allergens = productDto.Allergens,
                IsAvailable = true
            };

            await _productRepository.AddAsync(product);
            return await GetByIdAsync(product.Id);
        }

        public async Task<bool> UpdateAsync(int id, CreateProductDTO productDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.CategoryId = productDto.CategoryId;
            product.IsVegetarian = productDto.IsVegetarian;
            product.Stock = productDto.Stock;
            product.Calories = productDto.Calories;
            product.Protein = productDto.Protein;
            product.Carbs = productDto.Carbs;
            product.Fat = productDto.Fat;
            product.Allergens = productDto.Allergens;

            await _productRepository.UpdateAsync(product);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            await _productRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> UpdateStockAsync(int id, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            await _productRepository.UpdateStockAsync(id, quantity);
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(string query, decimal? minPrice = null, decimal? maxPrice = null, string allergens = null)
        {
            var products = await _productRepository.SearchAsync(query, minPrice, maxPrice, allergens);
            return products.Select(MapToDTO);
        }

        private ProductDTO MapToDTO(Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                IsAvailable = product.IsAvailable,
                IsVegetarian = product.IsVegetarian,
                Stock = product.Stock,
                Calories = product.Calories,
                Protein = product.Protein,
                Carbs = product.Carbs,
                Fat = product.Fat,
                Allergens = product.Allergens
            };
        }
    }
}
