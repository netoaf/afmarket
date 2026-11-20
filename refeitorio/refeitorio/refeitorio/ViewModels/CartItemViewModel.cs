namespace refeitorio.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public ProductViewModel Product { get; set; }
        
        // Display helper properties for backward compatibility
        public string ProductName => Product?.Name ?? string.Empty;
        public decimal Price => Product?.Price ?? 0;
        public decimal UnitPrice => Product?.Price ?? 0;
        public string ImageUrl => Product?.ImageUrl ?? string.Empty;
        public decimal TotalPrice => Price * Quantity;
    }
}
