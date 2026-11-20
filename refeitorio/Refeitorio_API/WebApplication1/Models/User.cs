namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserNumber { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ProfileImage { get; set; }
        public decimal Balance { get; set; }
        public string UserType { get; set; }
    }
}
