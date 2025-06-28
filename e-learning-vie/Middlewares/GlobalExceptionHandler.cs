using System.Net;
using System.Text.Json;
using e_learning_vie.Commons;
using Microsoft.AspNetCore.Diagnostics;

namespace e_learning_vie.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		httpContext.Response.ContentType = "application/json";
		httpContext.Response.StatusCode = 500;

		var response = ApiResponse<object>.Error("Internal server error");

		var json = JsonSerializer.Serialize(response);

		await httpContext.Response.WriteAsync(json, cancellationToken);
		return true;
	}
}
