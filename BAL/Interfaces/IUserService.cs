using Microsoft.AspNetCore.Http;
using Moodify.DTO;
using Moodify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public interface IUserService
	{
		Task<string> UpdateUserProfileAsync(updateinfodto model, User user);
		Task<string>UploadProfilePictureAsync(string userId,IFormFile file);
		Task<byte[]>GetProfilePictureAsync(string userId);
		Task<string>DeleteProfilePictureAsync(string userId);
		Task<UserDataDTO>GetInfoAsync(string userId);
	}
}
