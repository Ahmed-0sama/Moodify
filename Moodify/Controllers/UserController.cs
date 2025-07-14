using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moodify.DTO;
using Moodify.Models;
using Moodify.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Moodify.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{

		private readonly UserManager<User> userManager;
		private readonly IConfiguration configuration;
		public UserController(UserManager<User> userManager, IConfiguration configuration)
		{
			this.userManager = userManager;
			this.configuration = configuration;

		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(registerDTO dto)
		{
			if (ModelState.IsValid)
			{
				User user = new User
				{
					FirstName = dto.Fname,
					LastName = dto.Lname,
					Email = dto.email,
					UserName=dto.email
				};
				IdentityResult result = await userManager.CreateAsync(user, dto.password);
				if (result.Succeeded)
				{
					return Ok("Created");
				}
				foreach (var errors in result.Errors)
				{
					ModelState.AddModelError(string.Empty, errors.Description);
				}
			}
			return BadRequest(ModelState);
		}
		[HttpPost("login")]
		public async Task<IActionResult> login(loginDTO dto)
		{
			if (ModelState.IsValid)
			{
				var userdb = await userManager.FindByEmailAsync(dto.email);
				if (userdb != null)
				{
					var userClaims = new List<Claim>
					{
						new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
						new Claim(ClaimTypes.NameIdentifier,userdb.Id),
						new Claim(ClaimTypes.Name,userdb.UserName),
					};
					var userRole = await userManager.GetRolesAsync(userdb);
					foreach (var role in userRole)
					{
						userClaims.Add(new Claim(ClaimTypes.Role, role));
					}
					var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
					var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
					var token = new JwtSecurityToken(
						issuer: configuration["Jwt:Issuer"],
						audience: configuration["Jwt:Audience"],
						claims: userClaims,
						expires: DateTime.UtcNow.AddMinutes(30),
						signingCredentials: creds
					);

					var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

					string refreshToken = TokenReqest.GenerateRefreshToken();
					userdb.RefreshToken = refreshToken;
					userdb.RefreshTokenExpirytime = DateTime.UtcNow.AddDays(7);
					await userManager.UpdateAsync(userdb);

					return Ok(new
					{
						token = accessToken,
						expiration = token.ValidTo,
						refreshToken = refreshToken
					});
				}
				ModelState.AddModelError("UserName", "Invalid UserName or Password");
			}
				return BadRequest(ModelState);
			}
		[HttpPost("RefreshToken")]
		public async Task<IActionResult> RefreshToken(RefreshTokenDTO refreshTokenDTO)
		{
			if (string.IsNullOrEmpty(refreshTokenDTO.RefreshToken) || string.IsNullOrEmpty(refreshTokenDTO.AccessToken))
			{
				return BadRequest("Invalid token request");
			}

			var principal = TokenReqest.GetPrincipalFromExpiredToken(
			refreshTokenDTO.AccessToken,
			configuration["Jwt:Key"],
			configuration["Jwt:Issuer"],
			configuration["Jwt:Audience"]
			);
			if (principal == null)
			{
				return Unauthorized("Invalid or expired access token");
			}

			var username = principal.Identity?.Name;
			var userdb = await userManager.FindByNameAsync(username);

			// Fixed: Added refresh token validation and changed to UtcNow
			if (userdb == null || userdb.RefreshToken != refreshTokenDTO.RefreshToken || userdb.RefreshTokenExpirytime <= DateTime.UtcNow)
			{
				return Unauthorized("Invalid or expired refresh token");
			}

			var userClaims = new List<Claim>
			{
				new Claim (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier,userdb.Id),
				new Claim(ClaimTypes.Name,userdb.UserName)
			};
			var userRoles = await userManager.GetRolesAsync(userdb);
			foreach (var role in userRoles)
			{
				userClaims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var newToken = new JwtSecurityToken(
				issuer: configuration["Jwt:Issuer"],
				audience: configuration["Jwt:Audience"],
				claims: userClaims,
				expires: DateTime.UtcNow.AddMinutes(30),
				signingCredentials: creds
			);

			var newAccessToken = new JwtSecurityTokenHandler().WriteToken(newToken);

			string newRefreshToken = TokenReqest.GenerateRefreshToken();
			userdb.RefreshToken = newRefreshToken;
			userdb.RefreshTokenExpirytime = DateTime.UtcNow.AddDays(7);
			await userManager.UpdateAsync(userdb);

			return Ok(new
			{
				token = newAccessToken,
				expiration = newToken.ValidTo,
				refreshToken = newRefreshToken
			});
		}
		[Authorize]
		[HttpPut("UpdateInfo")]
		public async Task<IActionResult> UpdateInfo([FromBody] updateinfodto updateInfoDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User ID not found in token.");
			}
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound($"User with ID {userId} not found.");
			}

			user.FirstName = updateInfoDto.fname;
			user.LastName = updateInfoDto.lname;
		
			var result = await userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				return Ok(new { Message = "User information updated successfully." });
			}
			else
			{
				return BadRequest(result.Errors);
			}
		}
		[Authorize]
		[HttpPost("uploadpicture")]
		public async Task<IActionResult> uploadpicture([FromBody]IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest("Invalid File");
			}
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			using (var memorystream = new MemoryStream())
			{
				await file.CopyToAsync(memorystream);
				user.Photo = memorystream.ToArray();

			}
			await userManager.UpdateAsync(user);
			return Ok(new { Message = "Photo uploaded Sucessfully!" });
		}
		[Authorize]
		[HttpGet("GetPhoto")]
		public async Task<IActionResult> GetPhoto()
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			if (user.Photo == null)
			{
				return NotFound("No Photo Found");
			}
			return File(user.Photo, "image/png");
		}
	}
}
