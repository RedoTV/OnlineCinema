using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using OnlineCinema.Backend.Events;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Auth;
using OnlineCinema.Backend.Repositories.UnitOfWork;
using OnlineCinema.Backend.Repositories.Users;
using OnlineCinema.Backend.Services.Helpers;

namespace OnlineCinema.Backend.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _publisher;

    public AuthService(IUserRepository users, IUnitOfWork uow, IConfiguration configuration, IMapper mapper, IEventPublisher publisher)
    {
        _users = users;
        _uow = uow;
        _configuration = configuration;
        _mapper = mapper;
        _publisher = publisher;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _users.FindByEmailOrUsernameAsync(request.Email, request.Username);

        if (existingUser != null)
        {
            var message = existingUser.Email == request.Email
                ? "User with this email already exists"
                : "User with this username already exists";

            return new AuthResponseDto
            {
                Success = false,
                Message = message
            };
        }

        var salt = PasswordHasher.GenerateSalt();
        var hashedPassword = PasswordHasher.HashPassword(request.Password, salt);

        var user = _mapper.Map<User>(request);
        user.PasswordHash = hashedPassword;
        user.PasswordSalt = salt;

        _users.Add(user);
        await _uow.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        var response = _mapper.Map<AuthResponseDto>(user);
        response.Success = true;
        response.Message = "Registration successful";
        response.Token = token;

        await _publisher.PublishAsync("user.registered", new
        {
            userId = user.Id,
            username = user.Username,
            email = user.Email,
            happenedAt = DateTime.UtcNow
        });

        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _users.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }

        var hashedPassword = PasswordHasher.HashPassword(request.Password, user.PasswordSalt);

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
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Role, user.Role)
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