using e_learning_vie.Commons;
using e_learning_vie.DTOs.auth;
using e_learning_vie.Models;
using e_learning_vie.Services.Implements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace e_learning_vie.Controllers.Auth
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly JwtTokenService _jwtTokenService;

		public AuthController(UserManager<User> userManager, JwtTokenService jwtTokenService)
		{
			_userManager = userManager;
			_jwtTokenService = jwtTokenService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			var user = await _userManager.FindByNameAsync(model.Username);
			if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
			{
				return Unauthorized(ApiResponse<object>.Fail("Invalid email or password"));
			}

			var accessToken = await _jwtTokenService.GenerateAccessToken(user);
			var refreshToken = _jwtTokenService.GenerateRefreshToken(user);

			var response = new
			{
				userId = user.Id,
				email = user.Email,
				accessToken,
				refreshToken
			};

			// check xem FE la web hay j
			var clientPlatform = Request.Headers["X-Client-Platform"].ToString();
			var isMobile = clientPlatform == "android";

			if (!isMobile)
			{
				SetTokenCookies(accessToken, refreshToken);
				return Ok(ApiResponse<string>.Success("Login successful (cookie mode)"));
			}

			return Ok(ApiResponse<object>.Success("Login successful", response));
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel? model)
		{
			var refreshToken = model?.RefreshToken ?? Request.Cookies["refreshToken"];
			if (string.IsNullOrEmpty(refreshToken))
				return Unauthorized(ApiResponse<object>.Fail("Missing refresh token"));

			var (accessToken, newRefreshToken) = await _jwtTokenService.RefreshTokenAsync(refreshToken);
			if (accessToken == null)
				return Unauthorized(ApiResponse<object>.Fail("Invalid or expired refresh token"));


			// check xem FE la web hay j
			var clientPlatform = Request.Headers["X-Client-Platform"].ToString();
			var isMobile = clientPlatform == "android";

			if (!isMobile)
			{
				SetTokenCookies(accessToken, newRefreshToken!);
				return Ok(ApiResponse<string>.Success("Token refreshed (cookie mode)"));
			}

			return Ok(ApiResponse<object>.Success("Token refreshed", new
			{
				accessToken,
				refreshToken = newRefreshToken
			}));
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetCurrentUser()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return Unauthorized(ApiResponse<object>.Fail("User not found"));

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return Unauthorized(ApiResponse<object>.Fail("User not found"));

			var response = new
			{
				id = user.Id,
				email = user.Email,
				userName = user.UserName,
				studentId = user.StudentId,
				teacherId = user.TeacherId
			};

			return Ok(ApiResponse<object>.Success("User info retrieved", response));
		}

		[HttpPost("logout")]
		public IActionResult Logout()
		{
			// Chỉ cần xóa cookie nếu có
			Response.Cookies.Delete("accessToken");
			Response.Cookies.Delete("refreshToken");

			return Ok(ApiResponse<string>.Success("Logged out successfully"));
		}

		private void SetTokenCookies(string accessToken, string refreshToken)
		{
			Response.Cookies.Append("accessToken", accessToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddMinutes(30)
			});
			Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddDays(7)
			});
		}

	}
}
