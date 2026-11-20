namespace WebApplication1.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MenuId { get; set; }
        public DateTime ReservationDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public int? ValidatedByStaffId { get; set; }
        
        // Relações
        public User User { get; set; }
        public WeeklyMenu Menu { get; set; }
        public User ValidatedByStaff { get; set; }
    }
}
