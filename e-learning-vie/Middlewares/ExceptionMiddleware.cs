﻿using e_learning_vie.Commons;
using System.Net;
using System.Text.Json;

namespace e_learning_vie.Middlewares
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionMiddleware> _logger;

		public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context); // Chạy tiếp pipeline
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception");

				context.Response.ContentType = "application/json";
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

				var response = ApiResponse<object>.Error("Internal server error");
				var json = JsonSerializer.Serialize(response);

				await context.Response.WriteAsync(json);
			}
		}
	}
}
