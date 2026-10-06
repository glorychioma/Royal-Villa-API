using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla.DTO;
using RoyalVilla_API.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace RoyalVilla_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public AuthService(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task<bool> IsEmailExists(string email)
        {
            var user = await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
            return user;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try { 
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());
            if (user == null || user.Password != loginRequestDTO.Password)
            {
                return null;
            }
                //Generate Token
                var token = GenerateJwtToken(user);
            return new LoginResponseDTO
            {
                UserDTO = _mapper.Map<UserDTO>(user),
                Token = token
            };
            }catch(Exception ex)
            {
                //Handles unexpected errors
                //throw new InvalidOperationException("An unexpected error occurred during the user login", ex);
                Console.WriteLine($"LOGIN ERROR: {ex}");
                throw;
            }
        }

        public async Task<UserDTO> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            try { 
            if (await IsEmailExists(registrationRequestDTO.Email))
            {
                throw new InvalidOperationException($"User with {registrationRequestDTO.Email} already exists");
            }
            User user = new()
            {
                Email = registrationRequestDTO.Email,
                Name = registrationRequestDTO.Name,
                Password = registrationRequestDTO.Password,
                Role = string.IsNullOrEmpty(registrationRequestDTO.Role)? "Customer" : registrationRequestDTO.Role
            };
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var response = _mapper.Map<UserDTO>(user);
            return response;
            }
            catch(Exception ex)
            {
                //Handles unexpected errors
                throw new InvalidOperationException("An unexpected error occurred during the user registration", ex);
            };
        }
        private string GenerateJwtToken(User user)
        {
            // this extracts and encrypts the secret key, then stores it in a Key variable
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}
