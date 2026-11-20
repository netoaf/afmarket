namespace refeitorio.ViewModels
{
    public class WeeklyMenuItemViewModel
    {
        public int Id { get; set; }
        public DateTime MenuDate { get; set; }
        public bool IsVegetarian { get; set; }
        public ProductViewModel MainDish { get; set; }
        public ProductViewModel Soup { get; set; }
        public ProductViewModel Dessert { get; set; }
        public decimal Price { get; set; }
        public int MaxReservations { get; set; } = 50;
        public int CurrentReservations { get; set; } = 0;
        
        // Compatibility aliases for presentation
        public DayOfWeek DayOfWeek 
        { 
            get => MenuDate.DayOfWeek;
            set { } // Setter vazio para compatibilidade
        }
        
        public List<int> ProductIds 
        { 
            get 
            {
                var ids = new List<int>();
                if (MainDish != null) ids.Add(MainDish.Id);
                if (Soup != null) ids.Add(Soup.Id);
                if (Dessert != null) ids.Add(Dessert.Id);
                return ids;
            }
            set { } // Setter vazio para compatibilidade
        }
        
        public int MaxTickets 
        { 
            get => MaxReservations;
            set => MaxReservations = value;
        }
        
        public int CurrentTickets 
        { 
            get => CurrentReservations;
            set => CurrentReservations = value;
        }
    }
}
