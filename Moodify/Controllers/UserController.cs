using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Moodify.BAL.Interfaces;
using Moodify.BAL.Services;
using Moodify.DTO;
using Moodify.Models;
using Moodify.Services;
using Moodify.Shared.DTOs.Users;
using Newtonsoft.Json.Linq;
using System;
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

		MoodifyDbContext db;
		private readonly IConfiguration configuration;
		private readonly BAL.Interfaces.IEmailSender emailSender;
		private readonly IAuthService authService;
		public UserController(MoodifyDbContext db, IConfiguration configuration,BAL.Interfaces.IEmailSender emailSender,IAuthService authService)
		{
			this.db = db;
			this.configuration = configuration;
			this.emailSender = emailSender;
			this.authService = authService;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] Shared.DTOs.Users.RegisterModel model)
		{
			var result = await authService.RegisterAsync(model);

			if (!string.IsNullOrEmpty(result.Message) && result.IsAuthenticated == false)
			{
				if (result.RefreshToken != null) // using RefreshToken as placeholder for email token
				{
					var confirmationLink = Url.Action(
						nameof(ConfirmEmail),
						"User",
						new { userId = result.Userid, token = result.RefreshToken },
						Request.Scheme);

					await emailSender.SendEmailAsync(result.Email, "Confirm your email",
						$"Please confirm your account by clicking <a href='{confirmationLink}'>here</a>");
				}

				return Ok(result.Message);
			}

			return BadRequest(result.Message);
		}
		[HttpGet("ConfirmEmail")]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			var result = await authService.ConfirmEmailAsync(userId, token);

			if (result == "Email confirmed successfully")
				return Ok(result);

			return BadRequest(result);
		}
		[HttpPost("Login")]
		public async Task<IActionResult> Login(TokenRequestModel log)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var result = await authService.GetTokenAsync(log);
			if (!result.IsAuthenticated)
				return BadRequest(result.Message);
			return Ok(result);
		}
		[HttpPost("RefreshToken")]
		public async Task<IActionResult> RefreshToken(RefreshTokenDTO refreshTokenDTO)
		{
			if(string.IsNullOrEmpty(refreshTokenDTO.RefreshToken))
				return BadRequest("Invalid Token");
			var result = await authService.RefreshTokenAsync(refreshTokenDTO.RefreshToken);
			if (!result.IsAuthenticated)
				return BadRequest(result.Message);
			return Ok(result);
		}
		[HttpPost("ForgotPassword")]
		public async Task<IActionResult> ForgotPassword(ForgetPasswordDto model)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
				return BadRequest("User does not exist or email not confirmed");

			var token = await userManager.GeneratePasswordResetTokenAsync(user);

			var resetLink = Url.Action(
				nameof(ResetPassword),
				"Account",
				new { token, email = user.Email },
				Request.Scheme);

			// Send via email
			await emailSender.SendEmailAsync(user.Email, "Reset Password",
				$"Click <a href='{resetLink}'>here</a> to reset your password.");

			return Ok("Password reset link sent. Please check your email.");
		}
		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return BadRequest("Invalid request");

			var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
			if (result.Succeeded)
				return Ok("Password reset successfully");

			return BadRequest(result.Errors);
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
		[Authorize]
		[HttpGet("UserInfo")]
		public async Task<IActionResult> GetUserInfo()
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (user == null)
			{
				return NotFound("user Not Found");
			}
			var userdto = new UserDataDTO
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				image = user.Photo
			};
			return Ok(userdto);
		}
	}
}
