namespace WebApplication1.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public int? DeliveredByStaffId { get; set; }
        
        // Relações
        public User User { get; set; }
        public User DeliveredByStaff { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
