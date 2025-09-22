using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moodify.BAL.Helpers;
using Moodify.BAL.Interfaces;
using Moodify.DAL.Entities;
using Moodify.DTO;
using Moodify.Models;
using Moodify.Services;
using Moodify.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly JWT _jwt;
		private readonly IEmailSender _emailSender;
		public AuthService(UserManager<User> userManager,RoleManager<IdentityRole> roleManager,IOptions<JWT>jwt)
		{
			_userManager = userManager;
			_jwt = jwt.Value;
			this.roleManager = roleManager;
		}
		public async Task<AuthModel> RegisterAsync(RegisterModel model)
		{
			if (await _userManager.FindByEmailAsync(model.Email) != null)
			{
				return new AuthModel { Message = "Email is already Registered!" };
			}
			if (await _userManager.FindByNameAsync(model.Username) != null)
			{
				return new AuthModel { Message = "Username is already Registered!" };
			}
			var user = new User
			{
				UserName = model.Username,
				Email = model.Email,
				FirstName = model.Fname,
				LastName = model.Lname

			};
			var  result=await _userManager.CreateAsync(user, model.Password);
			if(!result.Succeeded)
			{
				var errors = string.Empty;
				foreach(var error in result.Errors)
				{
					errors += $"{error.Description},";
				}
				return new AuthModel { Message = errors };
			}
			await _userManager.AddToRoleAsync(user, "User");
			var jwtSecurityToken = await CreateJwtToken(user);
			// generate confirmation token
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
			return new AuthModel
			{
				Userid=user.Id,
				Email = user.Email,
				ExpireOn=jwtSecurityToken.ValidTo,
				IsAuthenticated = false,
				UserRoles = new List<string> { "User" },
				Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
				Username = user.UserName
			};
		}
		public async Task<string>AddRoleAsync(AddRoleModel model)
		{
			var user =await _userManager.FindByIdAsync(model.userId);
			if(user==null || await roleManager.RoleExistsAsync(model.role))
				return "User Not Found or Already in the Role";
			if (!await roleManager.RoleExistsAsync(model.role))
				return "User Already Assinged to this role";
			var result= await _userManager.AddToRoleAsync(user, model.role);
			return result.Succeeded ? string.Empty : "Something went wrong";
		}
		public async Task<AuthModel> RefreshTokenAsync(string refreshToken)
		{
			var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

			if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
				return new AuthModel { Message = "Invalid or expired refresh token." };

			var jwtSecurityToken = await CreateJwtToken(user);

			var authModel = new AuthModel
			{
				Email = user.Email,
				Username = user.UserName,
				IsAuthenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
				ExpireOn = jwtSecurityToken.ValidTo,
				UserRoles = (await _userManager.GetRolesAsync(user)).ToList()
			};

			// Rotate refresh token
			user.RefreshToken = TokenRequest.GenerateRefreshToken();
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
			await _userManager.UpdateAsync(user);

			authModel.RefreshToken = user.RefreshToken;

			return authModel;
		}
		public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
		{
			var authModel = new AuthModel();
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
			{
				authModel.Message = "Username or Password is incorrect!";
				return authModel;
			}
			if (!user.EmailConfirmed)
			{
				authModel.Message = "Email not confirmed yet!";
				return authModel;
			}
			var jwtSecurityToken = await CreateJwtToken(user);
			var rolesList = await _userManager.GetRolesAsync(user);
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			authModel.Email = user.Email;
			authModel.ExpireOn = jwtSecurityToken.ValidTo;
			authModel.Username = user.UserName;
			authModel.UserRoles = rolesList.ToList();
			return authModel;
		}
		public async Task<string> ConfirmEmailAsync(string userId, string token)
		{
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
				return "Invalid email confirmation request";

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return "User not found";

			var decodedBytes = WebEncoders.Base64UrlDecode(token);
			var decodedToken = Encoding.UTF8.GetString(decodedBytes);

			var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

			return result.Succeeded ? "Email confirmed successfully" : "Email confirmation failed";
		}
		public async Task<string>ForgetPasswordAsync(string email, string origin)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				return "User not found or Email not confirmed";
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
			var resetUrl = $"{origin}/resetpassword?token={encodedToken}&email={email}";
			await _emailSender.SendEmailAsync(email, "Reset Password",
				$"Please reset your password by clicking <a href='{resetUrl}'>here</a>");
			return "Password reset link has been sent to your email";
		}
		public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return IdentityResult.Failed(new IdentityError
				{
					Description = "User not found"
				});
			var decodedBytes = WebEncoders.Base64UrlDecode(model.Token);
			var decodedToken = Encoding.UTF8.GetString(decodedBytes);
			 return await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
		
		}
		private async Task<JwtSecurityToken>CreateJwtToken(User user)
		{
			var userClaims = await _userManager.GetClaimsAsync(user);
			var roles = await _userManager.GetRolesAsync(user);
			var roleClaims = new List<Claim>();
			foreach (var role in roles)
				roleClaims.Add(new Claim("roles", role));
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim("uid", user.Id)
			}
			.Union(userClaims)
			.Union(roleClaims);
			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
			var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
			var jwtSecurityToken = new JwtSecurityToken(
				issuer: _jwt.Issuer,
				audience: _jwt.Audience,
				claims: claims,
				expires: DateTime.Now.AddDays(_jwt.DurationInMinutes),
				signingCredentials: signingCredentials);
			return jwtSecurityToken;
		}

	}
}
