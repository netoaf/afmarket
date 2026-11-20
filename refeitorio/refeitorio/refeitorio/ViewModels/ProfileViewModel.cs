namespace refeitorio.ViewModels
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string UserNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public string UserType { get; set; } = "Student";
        public decimal Balance { get; set; }
    }
}
