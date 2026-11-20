namespace WebApplication1.DTOs
{
    public class TicketDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int MenuId { get; set; }
        public DateTime MenuDate { get; set; }
        public decimal Price { get; set; }
        public bool IsVegetarian { get; set; }
        public DateTime ReservationDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public int? ValidatedByStaffId { get; set; }
    }

    public class CreateTicketDTO
    {
        public int UserId { get; set; }
        public int MenuId { get; set; }
    }
}
