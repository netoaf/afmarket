namespace refeitorio.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVegetarian { get; set; }
        public int Stock { get; set; }
        public int Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }
        public string Allergens { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}
