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
		private readonly IUserService userService;
		private readonly UserManager<User> userManager;
		public UserController(MoodifyDbContext db, IConfiguration configuration,BAL.Interfaces.IEmailSender emailSender,IAuthService authService,IUserService userService,UserManager<User> userManager)
		{
			this.db = db;
			this.configuration = configuration;
			this.emailSender = emailSender;
			this.authService = authService;
			this.userService = userService;
			this.userManager = userManager;
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
			var result = await authService.ForgetPasswordAsync(model.Email, model.origin);
			if (result == "If an account with that email exists, a password reset link has been sent.")
				return Ok(result);
			return BadRequest(result);
		}
		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
		{
			var result = await authService.ResetPasswordAsync(model);

			if (result.Succeeded)
				return Ok("Password reset successfully");

			return BadRequest(result.Errors.Select(e => e.Description));
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
				return NotFound($"User with ID {userId} not found.");

			var result = await userService.UpdateUserProfileAsync(updateInfoDto, user);

			return result == "Profile updated successfully"
				? Ok(new { Message = result })
				: BadRequest(result);
		}
		[Authorize]
		[HttpPost("uploadpicture")]
		public async Task<IActionResult> uploadpicture([FromBody]IFormFile file)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await userService.UploadProfilePictureAsync(userId, file);

			return result == "Profile picture uploaded successfully"
				? Ok(new { Message = result })
				: BadRequest(result);
		}
		[Authorize]
		[HttpGet("GetPhoto")]
		public async Task<IActionResult> GetPhoto()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var photo = await userService.GetProfilePictureAsync(userId);

			if (photo == null)
				return NotFound("No Photo Found");

			return File(photo, "image/png");
		}
		[Authorize]
		[HttpGet("UserInfo")]
		public async Task<IActionResult> GetUserInfo()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userDto = await userService.GetInfoAsync(userId);

			return userDto == null
				? NotFound("User Not Found")
				: Ok(userDto);
		}
	}
}
