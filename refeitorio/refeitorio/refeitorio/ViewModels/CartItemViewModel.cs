namespace refeitorio.ViewModels
{
    public class CartItemViewModel
    {
        private string? _productName;
        private decimal? _unitPrice;
        private string? _imageUrl;

        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public ProductViewModel Product { get; set; }

        // The API returns these fields flat (productName, unitPrice, imageUrl), so they
        // must be settable. They fall back to the nested Product when it is present.
        public string ProductName
        {
            get => _productName ?? Product?.Name ?? string.Empty;
            set => _productName = value;
        }

        public decimal UnitPrice
        {
            get => _unitPrice ?? Product?.Price ?? 0;
            set => _unitPrice = value;
        }

        public string ImageUrl
        {
            get => _imageUrl ?? Product?.ImageUrl ?? string.Empty;
            set => _imageUrl = value;
        }

        // Display helper properties for backward compatibility
        public decimal Price => UnitPrice;
        public decimal TotalPrice => Price * Quantity;
    }
}
