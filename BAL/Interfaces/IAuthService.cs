using Microsoft.AspNetCore.Identity;
using Moodify.DAL.Entities;
using Moodify.DTO;
using Moodify.Models;
using Moodify.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface IAuthService
	{
		Task<AuthModel> RegisterAsync(RegisterModel model);
		Task<AuthModel> GetTokenAsync(TokenRequestModel model);
		Task<AuthModel> RefreshTokenAsync(string refreshToken);
		Task<string>AddRoleAsync(AddRoleModel model);
		Task<string>ConfirmEmailAsync(string userId, string token);
		Task<string> ForgetPasswordAsync(string email, string origin);
		Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model);
	}
}
