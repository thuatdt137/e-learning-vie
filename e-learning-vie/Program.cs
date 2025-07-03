using e_learning_vie.Middlewares;
using e_learning_vie.Models;
using e_learning_vie.Services;
using e_learning_vie.Services.Implements;
using e_learning_vie.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<SchoolManagementContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));




// dang ky service o day
builder.Services.AddScoped<ISchoolService, SchoolService>();
builder.Services.AddScoped<JwtTokenService>();




// config Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<SchoolManagementContext>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddHttpContextAccessor();



// logging
builder.Services.AddLogging(logging =>
{
	logging.AddConsole();
	logging.AddDebug();
	logging.SetMinimumLevel(LogLevel.Information);
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();



// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			// Nếu token không có trong header thì lấy từ cookie
			var accessToken = context.Request.Cookies["accessToken"];
			if (!string.IsNullOrEmpty(accessToken))
			{
				context.Token = accessToken;
			}
			return Task.CompletedTask;
		}
	};

	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = false,
		ValidateAudience = false,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
		)
	};
});


var app = builder.Build();


app.UseExceptionHandler();


// Initialize roles
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
	var roles = new[] { "Student", "Teacher", "TrainingDepartment", "VicePrincipal", "MinistryOfEducation" };
	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
		}
	}
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors(c =>
{
	c.AllowAnyHeader();
	c.AllowCredentials();
	c.AllowAnyMethod();
});

app.UseHttpsRedirection();



app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();