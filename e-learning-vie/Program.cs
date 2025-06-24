using e_learning_vie.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Thêm DbContext
builder.Services.AddDbContext<SchoolManagementContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

builder.Services.AddIdentityCore<User>()
	.AddEntityFrameworkStores<SchoolManagementContext>()
	.AddApiEndpoints();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();


builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme)
	.AddBearerToken(IdentityConstants.BearerScheme);

var app = builder.Build();
app.MapIdentityApi<User>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
