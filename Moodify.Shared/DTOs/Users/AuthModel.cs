using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.DAL.Entities
{
	public class AuthModel
	{
		public bool IsAuthenticated { get; set; }
		public string Message { get; set; }

		// User info for the client
		public string Username { get; set; }
		public string Userid { get; set; }
		public string EmailConfirmationToken {get; set; }
		public string Email { get; set; }
		public List<string> UserRoles { get; set; }

		// Tokens
		public string Token { get; set; }
		public DateTime ExpireOn { get; set; }
		public string? RefreshToken { get; set; }
		public DateTime RefreshTokenExpiryTime { get; set; }
	}
}
