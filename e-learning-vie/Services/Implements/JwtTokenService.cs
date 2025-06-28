using e_learning_vie.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace e_learning_vie.Services.Implements
{

	public class JwtTokenService
	{
		private readonly UserManager<User> _userManager;
		private readonly IConfiguration _config;

		public JwtTokenService(UserManager<User> userManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_config = configuration;
		}

		public async Task<string> GenerateAccessToken(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};

			if (user.TeacherId.HasValue)
			{
				claims.Add(new Claim("TeacherId", user.TeacherId.Value.ToString()));
			}
			if (user.StudentId.HasValue)
			{
				claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
			}

			// Thêm roles vào claims
			var roles = await _userManager.GetRolesAsync(user);
			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.UtcNow.AddMinutes(30);

			var token = new JwtSecurityToken(
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public string GenerateRefreshToken(User user)
		{
			var claims = new List<Claim>
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim("token_type", "refresh"),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				claims: claims,
				expires: DateTime.UtcNow.AddDays(7),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public async Task<(string? accessToken, string? newRefreshToken)> RefreshTokenAsync(string refreshToken)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

			try
			{
				var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero
				}, out SecurityToken validatedToken);

				// Check token_type
				var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
				if (tokenType != "refresh")
					return (null, null);

				// Lấy userId từ claim
				var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
				if (userId == null)
					return (null, null);

				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
					return (null, null);

				var accessToken = await GenerateAccessToken(user);
				var newRefreshToken = GenerateRefreshToken(user);

				return (accessToken, newRefreshToken);
			}
			catch
			{
				return (null, null);
			}
		}
	}
}