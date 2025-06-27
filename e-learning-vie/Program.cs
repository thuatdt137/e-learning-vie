using e_learning_vie.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;

var builder = WebApplication.CreateBuilder(args);

// Thêm DbContext
builder.Services.AddDbContext<SchoolManagementContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireLowercase = false;
})
	.AddEntityFrameworkStores<SchoolManagementContext>()
	.AddApiEndpoints();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();


builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
	.AddBearerToken(IdentityConstants.BearerScheme);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{

	static async Task CreateRolesAsync(IServiceProvider services)
	{
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
		var roles = new[] { "Student", "Teacher", "TrainingDepartment", "VicePrincipal", "MinistryOfEducation" };
		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
			}
		}
	}

}

app.MapIdentityApi<IdentityUser>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(c =>
{
	c.AllowAnyHeader();
	c.AllowAnyMethod();
	c.AllowAnyOrigin();
});

app.UseHttpsRedirection();
app.UseMiddleware<e_learning_vie.Middlewares.ExceptionMiddleware>();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
