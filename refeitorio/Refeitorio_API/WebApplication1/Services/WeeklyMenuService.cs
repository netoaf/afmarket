using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class WeeklyMenuService
    {
        private readonly WeeklyMenuRepository _menuRepository;
        private readonly ProductRepository _productRepository;
        private readonly TicketRepository _ticketRepository;

        public WeeklyMenuService(
            WeeklyMenuRepository menuRepository,
            ProductRepository productRepository,
            TicketRepository ticketRepository)
        {
            _menuRepository = menuRepository;
            _productRepository = productRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task<IEnumerable<WeeklyMenuDTO>> GetMenuAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isVegetarian = null)
        {
            var menus = await _menuRepository.GetAllAsync(startDate, endDate, isVegetarian);
            var menuDTOs = new List<WeeklyMenuDTO>();

            foreach (var menu in menus)
            {
                var menuDTO = await MapToDTOAsync(menu);
                menuDTOs.Add(menuDTO);
            }

            return menuDTOs;
        }

        public async Task<WeeklyMenuDTO> GetByIdAsync(int id)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
                return null;

            return await MapToDTOAsync(menu);
        }

        public async Task<WeeklyMenuDTO> CreateAsync(CreateWeeklyMenuDTO menuDto)
        {
            // Verificar se os produtos existem
            var mainDish = await _productRepository.GetByIdAsync(menuDto.MainDishId);
            if (mainDish == null)
                return null;

            // Validar SoupId se fornecido
            if (menuDto.SoupId.HasValue)
            {
                var soup = await _productRepository.GetByIdAsync(menuDto.SoupId.Value);
                if (soup == null)
                    return null;
            }

            // Validar DessertId se fornecido
            if (menuDto.DessertId.HasValue)
            {
                var dessert = await _productRepository.GetByIdAsync(menuDto.DessertId.Value);
                if (dessert == null)
                    return null;
            }

            var weeklyMenu = new WeeklyMenu
            {
                MenuDate = menuDto.MenuDate,
                IsVegetarian = menuDto.IsVegetarian,
                MainDishId = menuDto.MainDishId,
                SoupId = menuDto.SoupId,
                DessertId = menuDto.DessertId,
                Price = menuDto.Price,
                MaxReservations = menuDto.MaxReservations
            };

            await _menuRepository.AddAsync(weeklyMenu);
            return await GetByIdAsync(weeklyMenu.Id);
        }

        public async Task<bool> UpdateAsync(int id, CreateWeeklyMenuDTO menuDto)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
                return false;

            // Verificar se os produtos existem
            var mainDish = await _productRepository.GetByIdAsync(menuDto.MainDishId);
            if (mainDish == null)
                return false;

            // Validar SoupId se fornecido
            if (menuDto.SoupId.HasValue)
            {
                var soup = await _productRepository.GetByIdAsync(menuDto.SoupId.Value);
                if (soup == null)
                    return false;
            }

            // Validar DessertId se fornecido
            if (menuDto.DessertId.HasValue)
            {
                var dessert = await _productRepository.GetByIdAsync(menuDto.DessertId.Value);
                if (dessert == null)
                    return false;
            }

            menu.MenuDate = menuDto.MenuDate;
            menu.IsVegetarian = menuDto.IsVegetarian;
            menu.MainDishId = menuDto.MainDishId;
            menu.SoupId = menuDto.SoupId;
            menu.DessertId = menuDto.DessertId;
            menu.Price = menuDto.Price;
            menu.MaxReservations = menuDto.MaxReservations;

            await _menuRepository.UpdateAsync(menu);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
                return false;

            await _menuRepository.DeleteAsync(id);
            return true;
        }

        private async Task<WeeklyMenuDTO> MapToDTOAsync(WeeklyMenu menu)
        {
            var currentReservations = await _menuRepository.GetCurrentReservationCountAsync(menu.Id);

            return new WeeklyMenuDTO
            {
                Id = menu.Id,
                MenuDate = menu.MenuDate,
                IsVegetarian = menu.IsVegetarian,
                MainDish = MapProductToDTO(menu.MainDish),
                Soup = menu.Soup != null ? MapProductToDTO(menu.Soup) : null,
                Dessert = menu.Dessert != null ? MapProductToDTO(menu.Dessert) : null,
                Price = menu.Price,
                MaxReservations = menu.MaxReservations,
                CurrentReservations = currentReservations
            };
        }

        private ProductDTO MapProductToDTO(Product product)
        {
            if (product == null)
                return null;

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
