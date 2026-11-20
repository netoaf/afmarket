namespace WebApplication1.DTOs
{
    public class LoginDTO
    {
        public string UserNumber { get; set; }
        public string Password { get; set; }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string UserNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ProfileImage { get; set; }
        public decimal Balance { get; set; }
        public string UserType { get; set; }
    }

    public class RegisterUserDTO
    {
        public string UserNumber { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UserType { get; set; } = "Student";
    }
}
