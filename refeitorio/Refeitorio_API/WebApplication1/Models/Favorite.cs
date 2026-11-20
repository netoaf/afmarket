namespace WebApplication1.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; }
        
        // Relações
        public User User { get; set; }
        public Product Product { get; set; }
    }
}
