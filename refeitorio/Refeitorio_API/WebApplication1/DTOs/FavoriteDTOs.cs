namespace WebApplication1.DTOs
{
    public class FavoriteDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class AddFavoriteDTO
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
}
