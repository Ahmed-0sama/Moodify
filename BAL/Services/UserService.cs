using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moodify.BAL.Interfaces;
using Moodify.DTO;
using Moodify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<User> userManager;
		public UserService(UserManager<User> userManager)
		{
			this.userManager = userManager;
		}

		public async Task<string> DeleteProfilePictureAsync(string userId)
		{
			var user = userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return "User not found";
			}
			if (user.Result.Photo == null || user.Result.Photo.Length == 0)
			{
				return "No profile picture to delete";
			}
			user.Result.Photo = null;
			var result = await userManager.UpdateAsync(user.Result);
			if (result.Succeeded)
			{
				return "Profile picture deleted successfully";
			}
			return "Error deleting profile picture";
		}

		public async Task<UserDataDTO> GetInfoAsync(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return null;
			}
			var userdto = new UserDataDTO()
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				image = user.Photo
			};
			return userdto;
		}

		public async Task<byte[]> GetProfilePictureAsync(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null || user.Photo == null || user.Photo.Length == 0)
			{
				return null;
			}
			return user.Photo;
		}
		public async Task<string> UpdateUserProfileAsync(updateinfodto model, string userid)
		{
			var user = await userManager.FindByIdAsync(userid);
			if (user == null)
			{
				return "User not found";
			}
			user.FirstName = model.fname;
			user.LastName = model.lname;
			var result = await userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				return "Profile updated successfully";
			}
			return "Error updating profile";
		}

		public async Task<string> UploadProfilePictureAsync(string userId, IFormFile file)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
				return "User not found";

			if (file == null || file.Length == 0)
				return "Invalid file";

			using (var ms = new MemoryStream())
			{
				await file.CopyToAsync(ms);
				user.Photo = ms.ToArray();
			}

			var result = await userManager.UpdateAsync(user);

			return result.Succeeded ? "Profile picture uploaded successfully"
									: string.Join(", ", result.Errors.Select(e => e.Description));
		}
	}
}
