namespace e_learning_vie.DTOs.auth
{
	public class LoginModel
	{
		public string Username { get; set; } = null!;
		public string Password { get; set; } = null!;
	}

	public class RefreshTokenModel
	{
		public string RefreshToken { get; set; } = null!;
	}
}
