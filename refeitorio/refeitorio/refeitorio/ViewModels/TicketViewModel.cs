namespace refeitorio.ViewModels
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MenuId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public WeeklyMenuItemViewModel Menu { get; set; }
        
        // Compatibility aliases for existing UI usage
        public int StudentId => UserId;
        public DateTime ReservationDate => CreatedAt;
        public DayOfWeek DayOfWeek => Menu?.MenuDate.DayOfWeek ?? DayOfWeek.Monday;
        public bool IsVegetarian => Menu?.IsVegetarian ?? false;
        public decimal Price => Menu?.Price ?? 0;
        
        // Computed status logic matching original behavior
        public string ComputedStatus
        {
            get
            {
                if (Menu == null) return "Desconhecido";
                var today = DateTime.Today;
                var mealDate = Menu.MenuDate.Date;

                if (mealDate < today) return "Expirado";
                return "Reservado";
            }
        }
    }
}
