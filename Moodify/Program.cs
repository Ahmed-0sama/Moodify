using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Moodify.BAL.Helpers;
using Moodify.BAL.Interfaces;
using Moodify.BAL.Services;
using Moodify.Models;
using Moodify.Services;
using System;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;
// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<SpotifyTokenManager>();
builder.Services.AddDbContext<MoodifyDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>()
	.AddEntityFrameworkStores<MoodifyDbContext>()
	.AddDefaultTokenProviders();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<RoleSeederService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
// JWT Configuration
builder.Services.Configure<JWT>(builder.Configuration.GetSection("Jwt"));
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.SaveToken = true;
	options.RequireHttpsMetadata = false; // Set to true in production
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = key,
		ValidIssuer = jwtSection["Issuer"],
		ValidAudience = jwtSection["Audience"],
		RoleClaimType = ClaimTypes.Role,
		ClockSkew = TimeSpan.FromMinutes(5)
	};
});
// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		Description = "Enter 'Bearer' followed by a space and your JWT token"
	});

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] { }
		}
	});
});
builder.Services.AddScoped<IEmailSender, EmailSender>();
var app = builder.Build();

// Use Swagger in development
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
	var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeederService>();
	await roleSeeder.SeedRolesAndAdminAsync();
}
// Middleware pipeline
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MusicHub>("/musichub");
app.Run();