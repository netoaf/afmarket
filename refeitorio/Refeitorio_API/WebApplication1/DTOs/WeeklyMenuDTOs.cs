namespace WebApplication1.DTOs
{
    public class WeeklyMenuDTO
    {
        public int Id { get; set; }
        public DateTime MenuDate { get; set; }
        public bool IsVegetarian { get; set; }
        public ProductDTO MainDish { get; set; }
        public ProductDTO Soup { get; set; }
        public ProductDTO Dessert { get; set; }
        public decimal Price { get; set; }
        public int MaxReservations { get; set; }
        public int CurrentReservations { get; set; }
    }

    public class CreateWeeklyMenuDTO
    {
        public DateTime MenuDate { get; set; }
        public bool IsVegetarian { get; set; }
        public int MainDishId { get; set; }
        public int? SoupId { get; set; }
        public int? DessertId { get; set; }
        public decimal Price { get; set; }
        public int MaxReservations { get; set; } = 150;
    }
}
