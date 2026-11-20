namespace refeitorio.ViewModels
{
    public class WeeklyMenuViewModel
    {
        public Dictionary<DayOfWeek, WeeklyMenuItemViewModel> RegularMenu { get; set; } = new();
        public Dictionary<DayOfWeek, WeeklyMenuItemViewModel> VegetarianMenu { get; set; } = new();
        public bool ShowVegetarian { get; set; } = false;
        public Dictionary<DayOfWeek, bool> HasTicket { get; set; } = new();
        public DateTime CurrentWeekStart { get; set; } // Início da semana atual
        public DateTime WeekStart { get; set; } // Início da semana sendo visualizada
    }
}
