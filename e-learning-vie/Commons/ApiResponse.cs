namespace e_learning_vie.Commons
{
	public class ApiResponse<T>
	{
		public string Status { get; set; } = null!; // success, fail, error
		public string Message { get; set; } = null!;
		public T? Data { get; set; }
		public object? Errors { get; set; }

		public static ApiResponse<T> Success(string message, T? data = default) =>
			new() { Status = "success", Message = message, Data = data };

		public static ApiResponse<T> Fail(string message, object? errors = null) =>
			new() { Status = "fail", Message = message, Errors = errors };

		public static ApiResponse<T> Error(string message) =>
			new() { Status = "error", Message = message };
	}
}
