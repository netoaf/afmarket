using Microsoft.AspNetCore.Identity;
using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(UserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDTO> AuthenticateAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUserNumberAsync(loginDto.UserNumber);
            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                UserNumber = user.UserNumber,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                ProfileImage = user.ProfileImage,
                Balance = user.Balance,
                UserType = user.UserType
            };
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                UserNumber = user.UserNumber,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                ProfileImage = user.ProfileImage,
                Balance = user.Balance,
                UserType = user.UserType
            };
        }

        public async Task<UserDTO> GetByUserNumberAsync(string userNumber)
        {
            var user = await _userRepository.GetByUserNumberAsync(userNumber);
            if (user == null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                UserNumber = user.UserNumber,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                ProfileImage = user.ProfileImage,
                Balance = user.Balance,
                UserType = user.UserType
            };
        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                UserNumber = u.UserNumber,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                ProfileImage = u.ProfileImage,
                Balance = u.Balance,
                UserType = u.UserType
            }).ToList();
        }

        public async Task<UserDTO> CreateAsync(RegisterUserDTO registerDto)
        {
            var user = new User
            {
                UserNumber = registerDto.UserNumber,
                Name = registerDto.Name,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                UserType = registerDto.UserType,
                Balance = 0
            };
            user.Password = _passwordHasher.HashPassword(user, registerDto.Password);

            await _userRepository.AddAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                UserNumber = user.UserNumber,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                ProfileImage = user.ProfileImage,
                Balance = user.Balance,
                UserType = user.UserType
            };
        }

        public async Task<bool> UpdateAsync(UserDTO userDto)
        {
            var user = await _userRepository.GetByIdAsync(userDto.Id);
            if (user == null)
                return false;

            user.Name = userDto.Name;
            user.Email = userDto.Email;
            user.Phone = userDto.Phone;
            user.ProfileImage = userDto.ProfileImage;
            user.UserType = userDto.UserType;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            await _userRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> AddBalanceAsync(int id, decimal amount)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            user.Balance += amount;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateProfileImageAsync(int id, string imageUrl)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            user.ProfileImage = imageUrl;
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
