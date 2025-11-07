using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs;

namespace OnlineCinema.Backend.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, IMapper mapper)
    {
        _context = context;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User already exists"
            };
        }

        var salt = GenerateSalt();
        var hashedPassword = HashPassword(request.Password, salt);

        var user = _mapper.Map<User>(request);
        user.PasswordHash = hashedPassword;
        user.PasswordSalt = salt;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        var response = _mapper.Map<AuthResponseDto>(user);
        response.Success = true;
        response.Message = "Registration successful";
        response.Token = token;

        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }

        var hashedPassword = HashPassword(request.Password, user.PasswordSalt);

        if (hashedPassword != user.PasswordHash)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }

        var token = GenerateJwtToken(user);

        var response = _mapper.Map<AuthResponseDto>(user);
        response.Success = true;
        response.Message = "Login successful";
        response.Token = token;

        return response;
    }

    private string GenerateSalt()
    {
        var buffer = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        return Convert.ToBase64String(buffer);
    }

    private string HashPassword(string password, string salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["JwtSettings:Secret"];
        var jwtIssuer = _configuration["JwtSettings:Issuer"];
        var jwtAudience = _configuration["JwtSettings:Audience"];
        var jwtExpirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"]!);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}