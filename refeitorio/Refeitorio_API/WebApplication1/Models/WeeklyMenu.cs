namespace WebApplication1.Models
{
    public class WeeklyMenu
    {
        public int Id { get; set; }
        public DateTime MenuDate { get; set; }
        public bool IsVegetarian { get; set; }
        public int MainDishId { get; set; }
        public int? SoupId { get; set; }
        public int? DessertId { get; set; }
        public decimal Price { get; set; }
        public int MaxReservations { get; set; }
        
        // Relações com Product
        public Product MainDish { get; set; }
        public Product Soup { get; set; }
        public Product Dessert { get; set; }
    }
}
